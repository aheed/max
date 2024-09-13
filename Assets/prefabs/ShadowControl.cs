using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowControl : MonoBehaviour
{
    public float shadowCoeffX = 0.7f;
    public float shadowCoeffY = -1.0f;
    public Sprite turnSprite;
    public Sprite straightSprite;
    private SpriteRenderer spriteR;

    //public IPositionObservable Plane {get; set;}
    private IPositionObservable plane;

    // Start is called before the first frame update
    void Start()
    {
        spriteR = gameObject.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (plane == null)
        {
            Debug.Log("No Plane!!");
            Destroy(gameObject);
            return;
        }

        if (plane is MonoBehaviour planeAsMB)
        {
            if (!planeAsMB.isActiveAndEnabled)
            {
                plane = null;
                return;
            }
        }

        var planePos = plane.GetPosition();
        var planeAltitude = plane.GetAltitude();
        Vector2 newPos = planePos + new Vector2(planeAltitude * shadowCoeffX, planeAltitude * shadowCoeffY);
        transform.position = newPos;
        var planeMoveX = plane.GetMoveX();
        var newSprite = planeMoveX == 0 ? straightSprite : turnSprite;
        if (newSprite != spriteR.sprite)
        {
            spriteR.sprite = newSprite;
        }
    }

    public void SetPlane(IPositionObservable plane)
    {
        this.plane = plane;
        Debug.Log("Setting plane instance");
        Debug.Log(this);
        Debug.Log(plane);
    }
}
