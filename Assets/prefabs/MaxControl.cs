using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class MaxControl : MonoBehaviour, IPositionObservable, IGameStateObserver
{
    public Transform refObject;
    public float horizontalSpeed = 3.0f;
    public float verticalSpeed = 2.0f;
    public static readonly float bulletIntervalSeconds = 0.1f;
    public static readonly float bombIntervalSeconds = 0.5f;
    public static readonly float minAltitude = 0.1f;
    public static readonly float landingAltitude = 0.11f;
    float bulletCooldown = 0.0f;
    float bombCooldown = 0.0f;
    public InputAction MoveAction;
    public InputAction FireAction;
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
	    rigidbody2d = GetComponent<Rigidbody2D>();
        spriteR = gameObject.GetComponent<SpriteRenderer>();
    }

    void FireBullet(GameStatus gameStatus)
    {
        switch (gameStatus)
        {
            case GameStatus.FINISHED:
            case GameStatus.DEAD:
                // Todo: start a new game
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

    void HandleMove(Vector2 move, GameStatus gameStatus)
    {
        Vector2 apparentMove = move;

        switch(gameStatus)
        {
            case GameStatus.FINISHED:
            case GameStatus.DEAD:
            case GameStatus.KILLED_BY_FLACK:
                return;
            case GameStatus.ACCELERATING:
            case GameStatus.DECELERATING:
            case GameStatus.REFUELLING:
                apparentMove.y = 0;
                if (move.x != 0)
                {
                    gameState.SetStatus(GameStatus.DEAD);
                    return;
                }
                break;
            case GameStatus.COLLIDED:
                // Todo: reassign apparentMove to left,right,neutral randomly + down
                break;
            default:
                break;
        }

        if (apparentMove != Vector2.zero || !initialized)
        {
            Vector3 tmpLocalPosition = transform.localPosition;
            tmpLocalPosition.x += apparentMove.x * horizontalSpeed * Time.deltaTime;
            tmpLocalPosition.z -= apparentMove.y * verticalSpeed * Time.deltaTime;
            if (tmpLocalPosition.z < minAltitude) 
            {
                tmpLocalPosition.z = minAltitude;
            }
            tmpLocalPosition.y = tmpLocalPosition.z;
            transform.localPosition = tmpLocalPosition;
            initialized = true;
        }

        if (apparentMove.x != lastMove.x)
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
                DropBomb(stateContents.gameStatus);
            }
        }
        
        bulletCooldown -= Time.deltaTime;
        bombCooldown -= Time.deltaTime;

        HandleMove(move, stateContents.gameStatus);

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

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.name.StartsWith("flack_expl"))
        {
            Debug.Log($"Ouch !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! hit by Flack");
        }                
    }

    void DropBomb(GameStatus gameStatus)
    {
        switch (gameStatus)
        {
            case GameStatus.FLYING:
            case GameStatus.OUT_OF_FUEL:
                break;
            default:
                return;
        }
    
        if (bombCooldown > 0)
        {
            return;
        }

        var bomb = Instantiate(bombPrefab, transform.position, Quaternion.identity, refObject);
        bombCooldown = bombIntervalSeconds;
    }

    public void OnGameStatusChanged(GameStatus gameStatus)
    {
        if(gameStatus == GameStatus.DEAD || gameStatus == GameStatus.KILLED_BY_FLACK)
        {
            spriteR.sprite = crashedSprite;
        }
    }
}
