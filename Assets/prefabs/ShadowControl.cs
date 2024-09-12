using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowControl : MonoBehaviour
{
    public float shadowCoeffX = 0.7f;
    public float shadowCoeffY = -1.0f;

    //public IPositionObservable Plane {get; set;}
    private IPositionObservable plane;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (plane != null)
        {
            var planePosz = plane.GetPosition();
            var planeAltitudez = plane.GetAltitude();
            Vector2 newPosz = planePosz + new Vector2(planeAltitudez * shadowCoeffX, planeAltitudez * shadowCoeffY);

            transform.position = newPosz;
            return;
        }

        Debug.Log("No Plane!!");

        MaxControl maxC = FindObjectOfType <MaxControl>();
        var planePos = maxC.GetPosition();
        var planeAltitude = maxC.GetAltitude();
        Vector2 newPos = planePos + new Vector2(planeAltitude * shadowCoeffX, planeAltitude * shadowCoeffY);

        transform.position = newPos;
    }

    public void SetPlane(IPositionObservable plane)
    {
        this.plane = plane;
        Debug.Log("Setting plane instance");
        Debug.Log(this);
        Debug.Log(plane);
    }
}
