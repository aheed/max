using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPlane : MonoBehaviour, IPositionObservable
{
    public float enemyPlaneSpeed = 0.1f;

    public Vector2 GetPosition()
    {
        return transform.position;
    }

    public float GetAltitude()
    {
        return transform.position.z;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 progress = new (enemyPlaneSpeed * Time.deltaTime, enemyPlaneSpeed * Time.deltaTime, 0.0f);
        transform.position += progress;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log("Enemyplane collided with " + col.name);
    }

}
