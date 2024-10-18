using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletControl : MonoBehaviour, IPositionObservable
{
    public float speed = 8.0f;
    public float range = 10.0f;
    Vector3 velocity;
    Vector3 startPosition;
    
    Rigidbody2D rigidbody2d;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
        velocity = new Vector3(speed, speed, 0);

        SpriteRenderer spriteR = gameObject.GetComponent<SpriteRenderer>();
        spriteR.sortingOrder = (int)(GetAltitude() * 100.0f);
        //Debug.Log($"Bullet altitude/sortingorder {GetAltitude()}/{spriteR.sortingOrder}");
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 position = transform.position;
        position += velocity * Time.deltaTime;
        transform.position = position;
        if (transform.position.x > (startPosition.x + range))
        {
            //Debug.Log($"Bullet out of sight at {transform.position} {rigidbody2d.position}");
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.name.StartsWith("max") || 
            col.name.StartsWith("flack") ||
            col.name.StartsWith("bomb"))
        {
            return;
        }

        var collObjName = CollisionHelper.GetObjectWithOverlappingAltitude(this, col.gameObject);
        if (collObjName == CollisionHelper.NoObject)
        {
            return; //no actual collision, different altitudes
        }

        //Debug.Log($"Hit!!!!!!!!!!!!!!! Bullet at altitude {GetAltitude()} collided with {col.name} {collObjName}");
        Destroy(gameObject);
    }

    public Vector2 GetPosition()
    {
        return transform.position;
    }

    public float GetAltitude()
    {
        return transform.position.z;
    }

    public float GetHeight()
    {
        return 0.0f;
    }
}
