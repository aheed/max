using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletControl : MonoBehaviour, IPositionObservable
{
    Vector3 speed;
    Vector3 startPosition;
    float range = 10.0f;
    Rigidbody2D rigidbody2d;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
        speed = new Vector3(4.0f, 4.0f, 0);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 position = transform.position;
        position += speed * Time.deltaTime;
        transform.position = position;
        if (transform.position.x > (startPosition.x + range))
        {
            //Debug.Log($"Bullet out of sight at {transform.position} {rigidbody2d.position}");
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        var collObjName = CollisionHelper.GetObjectWithOverlappingAltitude(this, col.gameObject);
        if (collObjName == CollisionHelper.NoObject)
        {
            return; //no actual collision, different altitudes
        }                

        Debug.Log($"Hit!!!!!!!!!!!!!!! Bullet at altitude {GetAltitude()} collided with {col.name} {collObjName}");
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
