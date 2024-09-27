using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class MaxControl : MonoBehaviour, IPositionObservable
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
    private SpriteRenderer spriteR;
    private bool initialized = false;

    // Start is called before the first frame update
    void Start()
    {
        MoveAction.Enable();
        FireAction.Enable();
	    rigidbody2d = GetComponent<Rigidbody2D>();
        spriteR = gameObject.GetComponent<SpriteRenderer>();
    }

    void FireBullet()
    {
        if (bulletCooldown <= 0)
        {
            //GameObject projectileObject = Instantiate(bulletPrefab, rigidbody2d.position, Quaternion.identity);
            //Debug.Log($"Creating bullet at {transform.position}");
            GameObject projectileObject = Instantiate(bulletPrefab, transform.position, Quaternion.identity, refObject);
            bulletCooldown = bulletIntervalSeconds;
        }
    }

    // Update is called once per frame
    void Update()
    {
        move = MoveAction.ReadValue<Vector2>();
        if (FireAction.IsPressed())
        {
            FireBullet();
            if (move.y > 0)
            {
                DropBomb();
            }
        }
        
        bulletCooldown -= Time.deltaTime;
        bombCooldown -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        if (move != Vector2.zero || !initialized)
        {
            Vector3 tmpLocalPosition = transform.localPosition;
            tmpLocalPosition.x += move.x * horizontalSpeed * Time.deltaTime;
            tmpLocalPosition.z -= move.y * verticalSpeed * Time.deltaTime;
            if (tmpLocalPosition.z < minAltitude) 
            {
                tmpLocalPosition.z = minAltitude;
            }
            tmpLocalPosition.y = tmpLocalPosition.z;
            transform.localPosition = tmpLocalPosition;
            initialized = true;
        }
        //Debug.Log($"zzzzzz {offset} {refObject.transform.position} {transform.position}");
        if (move.x != lastMove.x)
        {
            var newSprite = straightSprite;
            if (move.x < 0)
            {
                newSprite = leftSprite;
            }
            else if (move.x > 0)
            {
                newSprite = rightSprite;
            }
            spriteR.sprite = newSprite;
            lastMove = move;
        }

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

    void DropBomb()
    {
        if (bombCooldown > 0)
        {
            return;
        }

        var bomb = Instantiate(bombPrefab, transform.position, Quaternion.identity, refObject);
        bombCooldown = bombIntervalSeconds;
    }
}
