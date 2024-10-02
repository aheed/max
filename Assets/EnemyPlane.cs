using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPlane : MonoBehaviour, IPlaneObservable
{
    public float enemyPlaneSpeed = 0.1f;
    float lastAltitude;
    private SpriteRenderer spriteR;

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
        return 0.3f;
    }

    public float GetMoveX() => 0;

    public bool IsAlive() => true;

    // Start is called before the first frame update
    void Start()
    {
        spriteR = gameObject.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 progress = new (enemyPlaneSpeed * Time.deltaTime, enemyPlaneSpeed * Time.deltaTime, 0.0f);
        transform.position += progress;

        if (GetAltitude() != lastAltitude)
        {
            lastAltitude = GetAltitude();
            spriteR.sortingOrder = (int)(lastAltitude * 100.0f);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        var collObjName = CollisionHelper.GetObjectWithOverlappingAltitude(this, col.gameObject);
        if (!collObjName.StartsWith("bullet"))
        {
            return; //no bullet collision
        }                

        Debug.Log($"Enemy plane down!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! hit by {collObjName}");
        //Destroy(gameObject);
        gameObject.SetActive(false);
    }

}
