using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bridge_midsection : MonoBehaviour, IPositionObservable
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        //Debug.Log($"midsection collided with {col.name}");
        ITrigger2D parentTrigger = InterfaceHelper.GetInterface<ITrigger2D>(gameObject.transform.parent.gameObject);
        parentTrigger.OnTriggerEnter2D(col);
    }

    public Vector2 GetPosition() => transform.position;
    public float GetAltitude() => 0.25f;
    public float GetHeight() => 0.1f;
}
