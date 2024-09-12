using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletControl : MonoBehaviour
{
    Vector3 speed;
    Vector3 startPosition;
    float range = 10.0f;
    Rigidbody2D rigidbody2d;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        Debug.Log($"Bullet starting at {transform.position} {rigidbody2d.position}");
        startPosition = transform.position;
        speed = new Vector3(4.0f, 4.0f, 0);
        //speed = new Vector3(0.4f, 0.4f, 0);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 position = transform.position;
        position += speed * Time.deltaTime;
        transform.position = position;
        if (transform.position.x > (startPosition.x + range))
        {
            Debug.Log($"Bullet out of sight at {transform.position} {rigidbody2d.position}");
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log("Bullet collided with " + col.name);
        var tempMonoArray = col.gameObject.GetComponents<MonoBehaviour>();

        foreach (var monoBehaviour in tempMonoArray)
        {
            if (monoBehaviour is IPositionObservable posobs)
            {
                var alti = posobs.GetAltitude();
                Debug.Log($"Hit!!!! Bullet collided with something as altitude {alti}");
            }
        } 

        if (col.gameObject.GetComponent<EnemyPlane>())
        {

        }
        Destroy(gameObject);
    }
}
