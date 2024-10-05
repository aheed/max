using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree1 : MonoBehaviour, IPositionObservable
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector2 GetPosition() => transform.position;
    public float GetAltitude() => 0f;
    public float GetHeight() => 0.2f;
}
