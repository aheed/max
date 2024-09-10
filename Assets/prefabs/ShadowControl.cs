using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowControl : MonoBehaviour
{
    public float shadowCoeffX = 0.7f;
    public float shadowCoeffY = -1.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        MaxControl maxC = FindObjectOfType <MaxControl>();
        var planePos = maxC.GetPosition();
        var planeAltitude = maxC.GetAltitude();
        Vector2 newPos = planePos + new Vector2(planeAltitude * shadowCoeffX, planeAltitude * shadowCoeffY);

        transform.position = newPos;
    }
}
