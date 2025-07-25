using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

public class MaxControl : MonoBehaviour, IPlaneObservable
{
    public Transform refObject;    
    public float glideDescentRate = 0.3f;
    public float deadDescentRate = 1.5f;
    public float collidedDescentRate = 1.2f;
    public float speedDamageFactor = 0.5f;
    public float bombDamageProbability = 0.5f;
    public float gunDamageProbability = 0.5f;
    public float damagePeriodSec = 2.0f;
    public float offsetDecreaseRate = 0.3f;
    public float bulletIntervalSeconds = 0.1f;
    public  float bombIntervalSeconds = 0.5f;    
    public float minSafeTurnAltitude = 0.2f;
    public static readonly float landingAltitude = 0.11f;
    public float collidedMoveInterval = 0.03f;
    float bulletCooldown = 0.0f;
    float bombCooldown = 0.0f;
    float damageCooldown = 0f;
    float collidedCooldown = 0f;
    Vector2 lastCollidedMove;
    bool bombDamage = false;
    bool gunDamage = false;
    public InputAction DebugFlackAction;
    public InputAction DebugRepairAction;
    public InputAction DebugAuxAction;
    public float joystickDeadZone = 0.2f;
    Rigidbody2D rigidbody2d;
    Vector2 move;
    Vector2 lastMove;
    Vector2 lastApparentMove;
    float lastAltitude;
    public GameObject bulletPrefab;
    public Bomb bombPrefab;
    public Sprite leftSprite;
    public Sprite rightSprite;
    public Sprite straightSprite;
    public Sprite crashedSprite;
    SpriteRenderer spriteR;
    float offsetY = 0;
    GameState gameState;    
    private Vector2 touchStartPosition, touchEndPosition;
    private float maxMove = 1.0f;
    private float minMove = 4.0f;
    private float directionFactor = 0.6f; //tan(pi/8) ~ 0.41

    // Start is called before the first frame update
    void Start()
    {
        EnhancedTouchSupport.Enable();
        DebugFlackAction.Enable();
        DebugRepairAction.Enable();
        DebugAuxAction.Enable();
	    rigidbody2d = GetComponent<Rigidbody2D>();
        spriteR = gameObject.GetComponent<SpriteRenderer>();
        gameState = GameState.GetInstance();
        gameState.Subscribe(GameEvent.GAME_STATUS_CHANGED, OnGameStatusChanged);
        gameState.Subscribe(GameEvent.START, OnGameStart);
        lastCollidedMove = new Vector2(0, 0);
    }

    void FireBullet(GameStatus gameStatus)
    {
        switch (gameStatus)
        {
            case GameStatus.FINISHED:
            case GameStatus.DEAD:
                gameState.ReportEvent(GameEvent.RESTART_REQUESTED);
                return;
            case GameStatus.FLYING:
            case GameStatus.COLLIDED:
            case GameStatus.OUT_OF_FUEL:
                break;
            default:
                return;
        }

        if (bulletCooldown > 0 || 
            (gameState.GotDamage(DamageIndex.G) && gunDamage))
        {
            return;
        }

        Instantiate(bulletPrefab, transform.position, Quaternion.identity, refObject);
        bulletCooldown = bulletIntervalSeconds;
    }

    void HandleMove(Vector2 move)
    {
        GameStateContents stateContents = gameState.GetStateContents();
        Vector2 apparentMove = move;
        var forcedDescent = 0f;

        switch(stateContents.gameStatus)
        {
            case GameStatus.FINISHED:
            case GameStatus.DEAD:
                return;
            case GameStatus.KILLED_BY_FLACK:
                forcedDescent = deadDescentRate;
                apparentMove = Vector2.zero;
                break;
            case GameStatus.ACCELERATING:
                if (move.y < 0f && stateContents.speed < gameState.GetSafeTakeoffSpeed())
                {
                    apparentMove.y = 0; // no takeoff at low speed
                }
                break;
            case GameStatus.DECELERATING:
            case GameStatus.REFUELLING:
            case GameStatus.LOADING_BOMBS:
            case GameStatus.REPAIRING:
                apparentMove.y = 0;                
                break;
            case GameStatus.OUT_OF_FUEL:
                forcedDescent = glideDescentRate;
                apparentMove.y = Math.Max(apparentMove.y, 0f);
                break;
            case GameStatus.COLLIDED:
                forcedDescent = collidedDescentRate;
                collidedCooldown -= Time.deltaTime;
                if (collidedCooldown <= 0f)
                {
                    lastCollidedMove.x = UnityEngine.Random.Range(-1, 2);
                    collidedCooldown = collidedMoveInterval;
                }
                apparentMove = lastCollidedMove;
                break;
            default:
                break;
        }
        
        if (apparentMove.x != 0f && GetAltitude() < minSafeTurnAltitude)
        {
            apparentMove.x = 0f; // no turns at low altitude
        }
        
        Vector3 tmpLocalPosition = transform.localPosition;
        var deltaOffsetY = 0f;
        var speedFactor = gameState.GotDamage(DamageIndex.M) ? speedDamageFactor : 1.0f;
        var significantWind = stateContents.wind && 
            (stateContents.gameStatus == GameStatus.FLYING ||
             stateContents.gameStatus == GameStatus.OUT_OF_FUEL ||
             stateContents.gameStatus == GameStatus.KILLED_BY_FLACK);
        tmpLocalPosition.x += apparentMove.x * GameState.horizontalSpeed * speedFactor * Time.deltaTime +
            (significantWind ? stateContents.windDirection.x * GameState.windSpeed * Time.deltaTime : 0f);

        var moveY = -apparentMove.y * GameState.verticalSpeed * speedFactor * Time.deltaTime;
        //if (apparentMove.x == 0f || (offsetY <= 0 && moveY < 0f))
        if (apparentMove.x == 0f)
        {
            tmpLocalPosition.z += moveY;
        }
        else if (moveY != 0f)
        {
            deltaOffsetY = moveY;
        }
        
        //if (stateContents.gameStatus == GameStatus.FLYING &&
        if (GetAltitude() > landingAltitude &&
            deltaOffsetY == 0f &&
            offsetY > 0)
        {
            deltaOffsetY = -offsetDecreaseRate * Time.deltaTime;
            tmpLocalPosition.x += deltaOffsetY * SceneController.riverSlopes[SceneController.neutralRiverSlopeIndex];
        }
        var tmpOffsetY = offsetY + deltaOffsetY;        

        tmpLocalPosition.z += -forcedDescent * Time.deltaTime +
            (significantWind ? stateContents.windDirection.y * GameState.windSpeed * Time.deltaTime : 0f);

        if (tmpLocalPosition.z < gameState.minAltitude) 
        {
            tmpLocalPosition.z = gameState.minAltitude;
        }
        if (tmpLocalPosition.z > gameState.maxAltitude)
        {
            tmpLocalPosition.z = gameState.maxAltitude;
        }
        if (tmpLocalPosition.z + tmpOffsetY > gameState.maxAltitude)
        {
            tmpLocalPosition.z = transform.localPosition.z;
            tmpOffsetY = offsetY;
        }
        if (tmpOffsetY < 0f)
        {
            tmpOffsetY = 0f;
        }
        if (tmpLocalPosition.x > gameState.maxHorizPosition)
        {
            tmpLocalPosition.x = gameState.maxHorizPosition;
        }
        if (tmpLocalPosition.x < -gameState.maxHorizPosition)
        {
            tmpLocalPosition.x = -gameState.maxHorizPosition;
        }
        tmpLocalPosition.y = tmpLocalPosition.z + offsetY;
        if (transform.localPosition.z != tmpLocalPosition.z)
        {
            gameState.SetAltitude(tmpLocalPosition.z);
        }
        transform.localPosition = tmpLocalPosition;
        offsetY = tmpOffsetY;        

        if (apparentMove.x != lastApparentMove.x &&
            stateContents.gameStatus != GameStatus.DEAD &&
            stateContents.gameStatus != GameStatus.KILLED_BY_FLACK)
        {
            var newSprite = straightSprite;
            if (apparentMove.x < 0)
            {
                newSprite = leftSprite;
            }
            else if (apparentMove.x > 0)
            {
                newSprite = rightSprite;
            }
            spriteR.sprite = newSprite;
            lastApparentMove = apparentMove;
        }        
    }

    void HandleFlackHit()
    {
        //Debug.Log($"Ouch !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! hit by Flack");
        gameState.SetRandomDamage(true);
    }

    void HandleCollision(Collider2D col)
    {
        var collObjName = CollisionHelper.GetObjectWithOverlappingAltitude(this, col.gameObject);
        //Debug.Log($"========== {col.name} {collObjName}");
        if (collObjName.StartsWith("enemy"))
        {
            gameState.SetStatus(GameStatus.COLLIDED);
        }
        else if (collObjName.StartsWith("house") ||
                 collObjName.StartsWith("tree") ||
                 collObjName.StartsWith("bridge") ||
                 collObjName.StartsWith("boat") ||
                 collObjName.StartsWith("ExpHouse", true, CultureInfo.InvariantCulture))
        {
            gameState.SetStatus(GameStatus.DEAD);
        }

        //no collision
    }

    // Update is called once per frame
    void Update()
    {        
        GameStateContents stateContents = gameState.GetStateContents();

        var move = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        if (move.x < -joystickDeadZone)
        {
            move.x = -1f;
        }
        else if (move.x > joystickDeadZone)
        {
            move.x = 1f;
        }
        else
        {
            move.x = 0f;
        }

        if (move.y < -joystickDeadZone)
        {
            move.y = -1f;
        }
        else if (move.y > joystickDeadZone)
        {
            move.y = 1f;
        }
        else
        {
            move.y = 0f;
        }

        if (!Settings.GetPilotControl())
        {
            move.y = move.y * -1f;
        }

        ///////////////////
        bool fireTouch = false;
        foreach (var theTouch in UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches)
        {       
            if (theTouch.screenPosition.x < (Screen.width / 4) && 
                theTouch.screenPosition.y < (Screen.height / 2))
            {
                fireTouch = true;
                //Debug.Log($"Touch Fire at {theTouch.position}");
            }
            else 
            {
                if ((theTouch.phase == UnityEngine.InputSystem.TouchPhase.Moved ||
                     theTouch.phase == UnityEngine.InputSystem.TouchPhase.Ended))// &&
                        //theTouch.position.x > (Screen.width / 2) &&
                        //theTouch.fingerId == moveFingerId)
                {
                    touchEndPosition = theTouch.screenPosition;

                    float x = touchEndPosition.x - touchStartPosition.x;
                    float y = touchEndPosition.y - touchStartPosition.y;

                    float x2 = Mathf.Abs(x) > minMove ? x : 0f;
                    float y2 = Mathf.Abs(y) > minMove ? y : 0f;

                    float x3 = x2;
                    float y3 = y2;

                    if (x3 != 0f && Mathf.Abs(y3/x3) < directionFactor)
                    {
                        y3 = 0f;
                    }

                    if (y3 != 0f && Mathf.Abs(x3/y3) < directionFactor)
                    {
                        x3 = 0f;
                    }

                    move.x = x3 == 0f? 0f : x3 > 0f ? maxMove : -maxMove;
                    move.y = y3 == 0f? 0f : y3 > 0f ? -maxMove : maxMove;

                    Debug.Log($"Got Move {x},{y} {x2},{y2} {x3},{y3} {move.x},{move.y}");
                    //touchStartPosition = theTouch.position;
                }
                else if (theTouch.phase == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    touchStartPosition = theTouch.screenPosition;
                }
            }
        }
        
        //////////////////
        if (fireTouch || Input.GetButton("Fire1") || Input.GetButton("Fire2") || Input.GetButton("Fire3") || Input.GetButton("Fire4"))
        {
            FireBullet(stateContents.gameStatus);
            if (move.y > 0)
            {
                DropBomb();
            }
        }

        if (DebugFlackAction.WasPressedThisFrame())
        {
            HandleFlackHit();
        }

        if (DebugRepairAction.WasPressedThisFrame())
        {
            gameState.SetRandomDamage(false);
        }

        if (DebugAuxAction.WasPressedThisFrame())
        {
            //gameState.ReportEvent(GameEvent.BIG_DETONATION);
            //gameState.SetViewMode(gameState.viewMode == ViewMode.NORMAL ? ViewMode.TV_SIM : ViewMode.NORMAL);
            gameState.ReportEvent(GameEvent.DEBUG_ACTION1);
        }
        
        bulletCooldown -= Time.deltaTime;
        bombCooldown -= Time.deltaTime;
        damageCooldown -= Time.deltaTime;
        if (damageCooldown <= 0)
        {
            bombDamage = UnityEngine.Random.Range(0f, 1f) < bombDamageProbability;
            gunDamage = UnityEngine.Random.Range(0f, 1f) < gunDamageProbability;
            damageCooldown = damagePeriodSec;
        }

        if (move != lastMove)
        {
            lastMove = move;
        }
        HandleMove(move);

        if (GetAltitude() != lastAltitude)
        {
            lastAltitude = GetAltitude();
            spriteR.sortingOrder = (int)(lastAltitude * 100.0f);
            //Debug.Log($"Max new altitude/sortingorder {lastAltitude}/{spriteR.sortingOrder}");
        }
    }

    public Vector2 GetPosition()
    {
        return rigidbody2d.position;
    }

    public float GetAltitude()
    {
        return transform.localPosition.z;
    }

    public float GetHeight()
    {
        return Altitudes.planeHeight;
    }

    public float GetMoveX()
    {
        return move.x;
    }

    public bool IsAlive() => gameState != null && gameState.GetStateContents().gameStatus != GameStatus.DEAD;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.name.StartsWith("flack_expl"))
        {
            HandleFlackHit();
        }
        else
        {
            HandleCollision(col);
        }
    }

    void DropBomb()
    {
        switch (gameState.GetStateContents().gameStatus)
        {
            case GameStatus.FLYING:
            case GameStatus.OUT_OF_FUEL:
                break;
            default:
                return;
        }
    
        if (bombCooldown > 0 || 
            gameState.GetStateContents().bombs <= 0 ||
            (gameState.GotDamage(DamageIndex.B) && bombDamage))
        {
            return;
        }

        Instantiate(bombPrefab, transform.position, Quaternion.identity, refObject);
        bombCooldown = bombIntervalSeconds;
        gameState.IncrementBombs(-1);
    }

    void OnGameStatusChanged()
    {
        var gameStatus = gameState.GetStateContents().gameStatus;
        if(gameStatus == GameStatus.DEAD || gameStatus == GameStatus.KILLED_BY_FLACK)
        {
            spriteR.sprite = crashedSprite;
            if (gameStatus == GameStatus.DEAD)
            {
                gameState.ReportEvent(GameEvent.BIG_BANG);
            }
        }
    }

    void OnGameStart() {
        Vector3 tmpLocalPosition = transform.localPosition;
        if (tmpLocalPosition.z < landingAltitude) 
        {
            tmpLocalPosition.z = landingAltitude;
        }
        tmpLocalPosition.y = tmpLocalPosition.z;
        transform.localPosition = tmpLocalPosition;
        spriteR.sprite = straightSprite;
        offsetY = 0f;
    }
}
