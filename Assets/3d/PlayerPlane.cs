using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPlane : MonoBehaviour, IPlaneObservable
{
    public Material normalWingMaterial;
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
    public float bombIntervalSeconds = 0.5f;
    public float minSafeTurnAltitude = 0.2f;
    public static readonly float landingAltitude = 0.11f;
    public float collidedMoveInterval = 0.03f;
    public InputAction DebugFlakAction;
    public InputAction DebugRepairAction;
    public InputAction DebugAuxAction1;
    public InputAction DebugAuxAction2;
    public InputAction DebugAuxAction3;
    public InputAction CameraCycleAction;
    public InputAction TvSimToggleAction;
    public InputAction GamePadMoveAction;
    public InputAction GamePadFireAction;
    public GameObject bulletPrefab;
    public GameObject bombPrefab;
    public GameObject bouncingBombPrefab;
    public GameObject lightArcPrefab;
    private Vector2 touchStartPosition, touchEndPosition;
    private float maxMove = 1.0f;
    private float minMove = 4.0f;
    private float directionFactor = 0.6f; //tan(pi/8) ~ 0.41
    PlaneController controller;

    // state
    GameState gameState;
    Vector2 move;
    Vector2 lastMove;
    Vector2 lastApparentMove;
    float bulletCooldown = 0.0f;
    float bombCooldown = 0.0f;
    float damageCooldown = 0f;
    float collidedCooldown = 0f;
    Vector2 lastCollidedMove;
    float offsetZ = 0;
    bool bombDamage = false;
    bool gunDamage = false;
    bool lastAlive = false;
    GameObject currentBombPrefab;
    public float minStickXForZDiff = 0.1f;

    PlaneController GetController()
    {
        if (controller == null)
        {
            controller = InterfaceHelper.GetInterface<PlaneController>(transform.GetChild(1).gameObject);
        }
        return controller;
    }

    void SetAppearance(float moveX, float moveY, bool alive)
    {
        GetController().SetAppearance(moveX, moveY, alive);
        if (alive != lastAlive)
        {
            lastAlive = alive;
            transform.GetChild(2).gameObject.SetActive(!alive);
        }
    }

    public void SetAltitudeLights(bool on)
    {
        var lights = transform.GetChild(0).GetChild(1).gameObject;
        lights.SetActive(on);
    }

    public void SetArmaments(ArmamentType armamentType)
    {
        currentBombPrefab = armamentType == ArmamentType.STANDARD ? bombPrefab : bouncingBombPrefab;
    }

    // Start is called before the first frame update
    void Start()
    {
        UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Enable();
        DebugFlakAction.Enable();
        DebugRepairAction.Enable();
        DebugAuxAction1.Enable();
        DebugAuxAction2.Enable();
        DebugAuxAction3.Enable();
        CameraCycleAction.Enable();
        TvSimToggleAction.Enable();
        GamePadMoveAction.Enable();
        GamePadFireAction.Enable();
        gameState = GameState.GetInstance();
        gameState.Subscribe(GameEvent.DEBUG_FLAK_ACTION, OnDebugFlakAction);
        gameState.Subscribe(GameEvent.DEBUG_REPAIR_ACTION, OnDebugRepairAction);
        gameState.Subscribe(GameEvent.START, OnStart);
        gameState.Subscribe(GameEvent.GAME_STATUS_CHANGED, OnGameStatusChanged);
        gameState.Subscribe(GameEvent.DEBUG_ACTION1, OnDebugCallback1);
        gameState.Subscribe(GameEvent.DEBUG_ACTION2, OnDebugCallback2);
        gameState.Subscribe(GameEvent.DEBUG_ACTION3, OnDebugCallback3);
        OnStart();
        Reset();
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

        if (!gameState.bulletManager.TryFireBullet())
        {
            return;
        }

        Instantiate(bulletPrefab, transform.position, controller.planeModel.transform.rotation, refObject);
        bulletCooldown = bulletIntervalSeconds;
        gameState.ReportEvent(GameEvent.BULLET_FIRED);
    }

    void HandleMove(Vector2 move)
    {
        GameStateContents stateContents = gameState.GetStateContents();
        Vector2 apparentMove = move;
        var forcedDescent = 0f;

        if (apparentMove.x != 0f && GetAltitude() < (stateContents.floorAltitude + minSafeTurnAltitude))
        {
            //Debug.Log("No turn at low altitude");
            apparentMove.x = 0f;
        }

        switch (stateContents.gameStatus)
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
                    //Debug.Log("No takeoff at low speed");
                    apparentMove.y = 0;
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



        Vector3 tmpLocalPosition = transform.localPosition;
        var deltaOffsetZ = 0f;
        var speedFactor = gameState.GotDamage(DamageIndex.M) ? speedDamageFactor : 1.0f;
        var significantWind = stateContents.wind &&
            (stateContents.gameStatus == GameStatus.FLYING ||
             stateContents.gameStatus == GameStatus.OUT_OF_FUEL ||
             stateContents.gameStatus == GameStatus.KILLED_BY_FLACK);
        tmpLocalPosition.x += apparentMove.x * GameState.horizontalSpeed * speedFactor * Time.deltaTime +
            (significantWind ? stateContents.windDirection.x * GameState.windSpeed * Time.deltaTime : 0f);

        var moveY = -apparentMove.y * GameState.verticalSpeed * speedFactor * Time.deltaTime;

        if (Math.Abs(apparentMove.x) <= minStickXForZDiff)
        {
            tmpLocalPosition.y += moveY;
        }
        else if (moveY != 0f)
        {
            deltaOffsetZ = moveY;
        }

        if (GetAltitude() > landingAltitude &&
            deltaOffsetZ == 0f &&
            offsetZ > 0)
        {
            deltaOffsetZ = -offsetDecreaseRate * Time.deltaTime;
        }
        var tmpOffsetZ = offsetZ + deltaOffsetZ;

        tmpLocalPosition.y += -forcedDescent * Time.deltaTime +
            (significantWind ? stateContents.windDirection.y * GameState.windSpeed * Time.deltaTime : 0f);

        if (tmpLocalPosition.y < stateContents.floorAltitude)
        {
            tmpLocalPosition.y = stateContents.floorAltitude;
        }

        if (tmpLocalPosition.y > gameState.maxAltitude)
        {
            tmpLocalPosition.y = gameState.maxAltitude;
        }
        if (tmpLocalPosition.y + tmpOffsetZ > gameState.maxAltitude)
        {
            tmpLocalPosition.y = transform.localPosition.y;
            tmpOffsetZ = offsetZ;
        }
        if (tmpOffsetZ < 0f)
        {
            tmpOffsetZ = 0f;
        }
        if (tmpLocalPosition.x > gameState.maxHorizPosition)
        {
            tmpLocalPosition.x = gameState.maxHorizPosition;
        }
        if (tmpLocalPosition.x < -gameState.maxHorizPosition)
        {
            tmpLocalPosition.x = -gameState.maxHorizPosition;
        }
        //tmpLocalPosition.y = tmpLocalPosition.z + offsetZ;
        if (transform.localPosition.y != tmpLocalPosition.y)
        {
            gameState.SetAltitude(tmpLocalPosition.y);
        }

        offsetZ = tmpOffsetZ;
        transform.localPosition = tmpLocalPosition;

        if (stateContents.gameStatus != GameStatus.DEAD &&
            stateContents.gameStatus != GameStatus.KILLED_BY_FLACK)
        {
            SetAppearance(apparentMove.x, apparentMove.y, true);
            lastApparentMove = apparentMove;
        }
    }

    void HandleFlackHit()
    {
        //Debug.Log($"Ouch !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! hit by Flack");
        controller.Tilt();
        gameState.SetRandomDamage(true);
    }

    // Update is called once per frame
    void Update()
    {
        GameStateContents stateContents = gameState.GetStateContents();

        move = Input.GetAxis("Horizontal") * Vector2.right +
               Input.GetAxis("Vertical") * Vector2.up;

        if (move == Vector2.zero)
        {
            move = GamePadMoveAction.ReadValue<Vector2>();
        }

        if (!Settings.GetPilotControl())
        {
            move.y = move.y * -1f;
        }

        bool fireTouch = false;
        foreach (var theTouch in UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches)
        {
            if (theTouch.screenPosition.x > (Screen.width / 4) ||
                theTouch.screenPosition.y > (Screen.height / 2))
            {
                if ((theTouch.phase == UnityEngine.InputSystem.TouchPhase.Moved ||
                     theTouch.phase == UnityEngine.InputSystem.TouchPhase.Ended))
                {
                    touchEndPosition = theTouch.screenPosition;

                    float x = touchEndPosition.x - touchStartPosition.x;
                    float y = touchEndPosition.y - touchStartPosition.y;

                    float x2 = Mathf.Abs(x) > minMove ? x : 0f;
                    float y2 = Mathf.Abs(y) > minMove ? y : 0f;

                    float x3 = x2;
                    float y3 = y2;

                    if (x3 != 0f && Mathf.Abs(y3 / x3) < directionFactor)
                    {
                        y3 = 0f;
                    }

                    if (y3 != 0f && Mathf.Abs(x3 / y3) < directionFactor)
                    {
                        x3 = 0f;
                    }

                    move.x = x3 == 0f ? 0f : x3 > 0f ? maxMove : -maxMove;
                    move.y = y3 == 0f ? 0f : y3 > 0f ? -maxMove : maxMove;

                    //Debug.Log($"Got Move {x},{y} {x2},{y2} {x3},{y3} {move.x},{move.y}");
                }
                else if (theTouch.phase == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    touchStartPosition = theTouch.screenPosition;
                }
            }
            else
            {
                fireTouch = true;
                //Debug.Log($"Touch Fire at {theTouch.position}");
            }
        }

        if (fireTouch || Input.GetButton("Fire") || GamePadFireAction.IsPressed())
        {
            FireBullet(stateContents.gameStatus);
            if (move.y > 0)
            {
                DropBomb();
            }
        }

        if (DebugFlakAction.WasPressedThisFrame())
        {
            gameState.ReportDebugEvent(GameEvent.DEBUG_FLAK_ACTION);
        }

        if (DebugRepairAction.WasPressedThisFrame())
        {
            gameState.ReportDebugEvent(GameEvent.DEBUG_REPAIR_ACTION);
        }

        if (DebugAuxAction1.WasPressedThisFrame())
        {
            //controller.Roll(true);
            gameState.ReportDebugEvent(GameEvent.DEBUG_ACTION1);
            //gameState.ReportEvent(GameEvent.BIG_DETONATION);
            //gameState.SetViewMode(gameState.viewMode == ViewMode.NORMAL ? ViewMode.TV_SIM : ViewMode.NORMAL);
        }

        if (DebugAuxAction2.WasPressedThisFrame())
        {
            //controller.Roll(false);
            gameState.ReportDebugEvent(GameEvent.DEBUG_ACTION2);
        }

        if (DebugAuxAction3.WasPressedThisFrame())
        {
            gameState.ReportDebugEvent(GameEvent.DEBUG_ACTION3);
        }
        
        if (CameraCycleAction.WasPressedThisFrame())
        {
            gameState.ReportEvent(GameEvent.CAMERA_CHANGE_REQUESTED);
        }

        if (TvSimToggleAction.WasPressedThisFrame())
        {
            gameState.ReportEvent(GameEvent.TV_SIM_TOGGLE_REQUESTED);
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
    }

    public Vector2 GetPosition()
    {
        return transform.position;
    }

    public float GetAltitude()
    {
        return transform.localPosition.y;
    }

    public float GetHeight()
    {
        var collider = GetComponent<BoxCollider>();
        return collider.size.y;
    }

    public float GetMoveX()
    {
        return move.x;
    }

    public bool IsAlive() => gameState != null && gameState.GetStateContents().gameStatus != GameStatus.DEAD;

    public void Reset()
    {
        move = Vector2.zero;
        lastMove = Vector2.zero;
        lastApparentMove = Vector2.zero;
        bulletCooldown = 0.0f;
        bombCooldown = 0.0f;
        damageCooldown = 0f;
        collidedCooldown = 0f;
        lastCollidedMove = Vector2.zero;
        offsetZ = 0;
        bombDamage = false;
        gunDamage = false;
        var tmpPos = transform.localPosition;
        tmpPos.y = GameState.GetInstance().minAltitude;
        transform.localPosition = tmpPos;
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

        Instantiate(currentBombPrefab, transform.position, Quaternion.identity);
        bombCooldown = bombIntervalSeconds;
        gameState.IncrementBombs(-1);
        gameState.ReportEvent(GameEvent.BOMB_DROPPED);
    }

    public void OnGameStatusChanged()
    {
        var gameStatus = GameState.GetInstance().GetStateContents().gameStatus;
        if (gameStatus == GameStatus.DEAD || gameStatus == GameStatus.KILLED_BY_FLACK)
        {
            SetAppearance(0, 0, false);
            SetAltitudeLights(false);
            if (gameStatus == GameStatus.DEAD)
            {
                gameState.ReportEvent(GameEvent.BIG_BANG);
            }
        }
    }

    public void OnStart()
    {
        GetController().normalWingMaterial = normalWingMaterial;
        lastAlive = false;
        SetAppearance(0, 0, true);
        Vector3 tmpLocalPosition = transform.localPosition;
        if (tmpLocalPosition.y < landingAltitude)
        {
            tmpLocalPosition.y = landingAltitude;
        }
        transform.localPosition = tmpLocalPosition;
        offsetZ = 0f;
    }

    void OnTriggerEnter(Collider col)
    {
        //Debug.Log($"Plane collision !!!!!!!!!!!!!!!  with {col.gameObject.name}");

        if (col.gameObject.name.StartsWith("ground") ||
            col.gameObject.name.StartsWith("riversection"))
        {
            return;
        }

        if (col.gameObject.name.StartsWith("FlakEx"))
        {
            HandleFlackHit();
            return;
        }

        if (col.gameObject.name.StartsWith("EnemyPlane"))
        {
            gameState.SetStatus(GameStatus.COLLIDED);
            return;
        }

        if (col.gameObject.name.StartsWith("House") ||
            col.gameObject.name.StartsWith("Tree") ||
            col.gameObject.name.StartsWith("Bridge") ||
            col.gameObject.name.StartsWith("Boat") ||
            col.gameObject.name.StartsWith("Billboard") ||
            col.gameObject.name.StartsWith("Power") ||
            col.gameObject.name.StartsWith("Dam"))
        {
            Debug.Log($"Crash !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! hit by {col.gameObject.name}");
            gameState.SetStatus(GameStatus.DEAD);
            if (col.gameObject.name.StartsWith("PowerWire"))
            {
                Instantiate(lightArcPrefab, transform.position, Quaternion.identity, refObject);
            }
            return;
        }

        if (col.gameObject.name.StartsWith("Boss"))
        {
            Debug.Log($"Crash !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! hit by {col.gameObject.name}");
            gameState.SetStatus(GameStatus.KILLED_BY_FLACK);
            return;
        }
    }

    private void OnDebugFlakAction()
    {
        HandleFlackHit();
    }

    private void OnDebugRepairAction()
    {
        gameState.SetRandomDamage(false);
    }

    private void OnDebugCallback1()
    {
        Debug.Log("PlayerPlane.OnDebugAction1");
        transform.parent.position += new Vector3(0f, 0f, 0.05f);
    }

    private void OnDebugCallback2()
    {
        Debug.Log("PlayerPlane.OnDebugAction2");
        //transform.parent.position -= new Vector3(0f, 0f, 0.05f);
    }

    private void OnDebugCallback3()
    {
        Debug.Log("PlayerPlane.OnDebugAction3");
        //Instantiate(lightArcPrefab, transform.position, Quaternion.identity, refObject);
        gameState.ReportEvent(GameEvent.SMALL_DETONATION);
    }
    
}
