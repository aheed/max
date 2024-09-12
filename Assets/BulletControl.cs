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

    bool IsOverlappingAltitude(float altitude1, float height1, float altitude2, float height2)
    {
        return (((altitude1 + height1 / 2) >= (altitude2 - height2 / 2)) &&
            ((altitude1 - height1 / 2) <= (altitude2 + height2 / 2)));
    }

    bool IsOverlappingAltitude(IPositionObservable obj1, GameObject obj2)
    {
        var tempMonoArray = obj2.GetComponents<MonoBehaviour>();

        foreach (var monoBehaviour in tempMonoArray)
        {
            if (monoBehaviour is IPositionObservable posobs)
            {
                var alti = posobs.GetAltitude();
                return IsOverlappingAltitude(obj1.GetAltitude(), obj1.GetHeight(), posobs.GetAltitude(), posobs.GetHeight());
            }
        }

        // No altitude info found in obj2. Assume it overlaps.
        return true;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!IsOverlappingAltitude(this, col.gameObject))
        {
            return; //no actual collision, different altitudes
        }                

        Debug.Log($"Hit!!!! Bullet at altitude {GetAltitude()} collided with {col.name}");
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
