using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class House : MonoBehaviour, IPositionObservable
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        //Debug.Log($"********************** House at {transform.position} collided with {col.name} at {col.transform.position}");
        var collObjName = CollisionHelper.GetObjectWithOverlappingAltitude(this, col.gameObject);
        if (!collObjName.StartsWith("bomb"))
        {
            return;
        }

        Debug.Log($"House!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! hit by {col.name}");
        //Destroy(gameObject);
        //gameObject.SetActive(false);
    }

    public Vector2 GetPosition() => transform.position;
    public float GetAltitude() => 0.1f;
    public float GetHeight() => 0.4f;
}
