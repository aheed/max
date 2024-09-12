using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletControl : MonoBehaviour
{
    Vector3 speed;
    Vector3 startPosition;
    float range = 10.0f;

    // Start is called before the first frame update
    void Start()
    {
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
            Debug.Log("Bullet out of sight");
            Destroy(gameObject);
        }
    }
}
