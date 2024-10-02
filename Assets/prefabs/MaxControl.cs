using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class MaxControl : MonoBehaviour, IPlaneObservable, IGameStateObserver
{
    public Transform refObject;
    public float horizontalSpeed = 3.0f;
    public float verticalSpeed = 2.0f;
    public float glideDescentRate = 0.3f;
    public float deadDescentRate = 1.5f;
    public float collidedDescentRate = 1.2f;
    public static readonly float bulletIntervalSeconds = 0.1f;
    public static readonly float bombIntervalSeconds = 0.5f;
    public static readonly float minAltitude = 0.1f;
    public static readonly float minSafeTurnAltitude = 0.2f;
    public static readonly float landingAltitude = 0.11f;
    float bulletCooldown = 0.0f;
    float bombCooldown = 0.0f;
    public InputAction MoveAction;
    public InputAction FireAction;
    public InputAction DebugFlackAction;
    public InputAction DebugRepairAction;
    Rigidbody2D rigidbody2d;
    Vector2 move;
    Vector2 lastMove;
    float lastAltitude;
    public GameObject bulletPrefab;
    public Bomb bombPrefab;
    public Sprite leftSprite;
    public Sprite rightSprite;
    public Sprite straightSprite;
    public Sprite crashedSprite;
    private SpriteRenderer spriteR;
    private bool initialized = false;
    GameState gameState;

    // Start is called before the first frame update
    void Start()
    {
        MoveAction.Enable();
        FireAction.Enable();
        DebugFlackAction.Enable();
        DebugRepairAction.Enable();
	    rigidbody2d = GetComponent<Rigidbody2D>();
        spriteR = gameObject.GetComponent<SpriteRenderer>();
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

        if (bulletCooldown > 0)
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
                    gameState.SetStatus(GameStatus.DEAD);
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
                // Todo: reassign apparentMove to left,right,neutral randomly + down
                apparentMove = new Vector2(0f, 0f);
                break;
            default:
                break;
        }

        if (apparentMove != Vector2.zero || !initialized || forcedDescent != 0f)
        {
            Vector3 tmpLocalPosition = transform.localPosition;
            tmpLocalPosition.x += apparentMove.x * horizontalSpeed * Time.deltaTime;
            tmpLocalPosition.z -= apparentMove.y * verticalSpeed * Time.deltaTime;
            tmpLocalPosition.z -= forcedDescent * Time.deltaTime;
            if (tmpLocalPosition.z < minAltitude) 
            {
                tmpLocalPosition.z = minAltitude;
            }
            tmpLocalPosition.y = tmpLocalPosition.z;
            if (transform.localPosition.z != tmpLocalPosition.z)
            {
                gameState.SetAltitude(tmpLocalPosition.z);
            }
            transform.localPosition = tmpLocalPosition;
            initialized = true;
        }

        if (apparentMove.x != lastMove.x &&
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
            lastMove = apparentMove;
        }

        if (apparentMove.x != 0f && GetAltitude() < minSafeTurnAltitude)
        {
            gameState.SetStatus(GameStatus.DEAD);
        }
    }

    void HandleFlackHit()
    {
        Debug.Log($"Ouch !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! hit by Flack");
        gameState.SetRandomDamage(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (gameState == null)
        {
            gameState = FindObjectOfType<GameState>();
            gameState.RegisterObserver(this); 
        }
        GameStateContents stateContents = gameState.GetStateContents();

        move = MoveAction.ReadValue<Vector2>();
        if (FireAction.IsPressed())
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
        
        bulletCooldown -= Time.deltaTime;
        bombCooldown -= Time.deltaTime;

        HandleMove(move);

        if (GetAltitude() != lastAltitude)
        {
            lastAltitude = GetAltitude();
            spriteR.sortingOrder = (int)(lastAltitude * 100.0f);
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
        return 0.1f;
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
    
        if (bombCooldown > 0 || gameState.GetStateContents().bombs <= 0)
        {
            return;
        }

        Instantiate(bombPrefab, transform.position, Quaternion.identity, refObject);
        bombCooldown = bombIntervalSeconds;
        gameState.IncrementBombs(-1);
    }

    public void OnGameStatusChanged(GameStatus gameStatus)
    {
        if(gameStatus == GameStatus.DEAD || gameStatus == GameStatus.KILLED_BY_FLACK)
        {
            spriteR.sprite = crashedSprite;
        }
    }

    public void OnGameEvent(GameEvent gameEvent) {
        if (gameEvent == GameEvent.START)
        {
            Vector3 tmpLocalPosition = transform.localPosition;
            if (tmpLocalPosition.z < landingAltitude) 
            {
                tmpLocalPosition.z = landingAltitude;
            }
            tmpLocalPosition.y = tmpLocalPosition.z;
            transform.localPosition = tmpLocalPosition;
            spriteR.sprite = straightSprite;
        }
    }
}
