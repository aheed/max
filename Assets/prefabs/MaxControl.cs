using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MaxControl : MonoBehaviour, IPositionObservable
{
    public Transform refObject;
    Vector3 offset = new(0.0f, 0.0f, 0.8f);
    public float horizontalSpeed = 3.0f;
    public float verticalSpeed = 2.0f;
    public float bulletIntervalSeconds = 0.1f;
    float bulletCooldown = 0.0f;
    public InputAction MoveAction;
    public InputAction FireAction;
    Rigidbody2D rigidbody2d;
    Vector2 move;
    Vector2 lastMove;
    float lastAltitude;
    public GameObject bulletPrefab;
    public Sprite leftSprite;
    public Sprite rightSprite;
    public Sprite straightSprite;
    private SpriteRenderer spriteR;

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
            GameObject projectileObject = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
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
        }
        
        bulletCooldown -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        //Vector2 position = (Vector2)rigidbody2d.position + move * playerSpeed * Time.deltaTime;
        //rigidbody2d.MovePosition(position);
        //Vector3 position = transform.position;
        //position.x = position.x + move.x * playerSpeed * Time.deltaTime;
        //position.z = position.z + move.y * playerSpeed * Time.deltaTime;
        //transform.position = position;
        offset.x = offset.x + move.x * horizontalSpeed * Time.deltaTime;
        offset.z = offset.z - move.y * verticalSpeed * Time.deltaTime;
        if (offset.z < 0) 
        {
            offset.z = 0;
        }
        offset.y = offset.z;
        transform.position = refObject.transform.position + offset;
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
        return offset.z;
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
}
