using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowControl : MonoBehaviour
{
    public float shadowCoeffX = 0.0f;
    public float shadowCoeffY = -1.0f;
    public Sprite turnSprite;
    public Sprite straightSprite;
    public Sprite deadSprite;
    private SpriteRenderer spriteR;
    private IPlaneObservable plane;

    void Start()
    {
        spriteR = gameObject.GetComponent<SpriteRenderer>();
        plane = InterfaceHelper.GetInterface<IPlaneObservable>(gameObject.transform.parent.gameObject);
    }

    void Update()
    {
        var planeAltitude = plane.GetAltitude();
        transform.localPosition = new Vector3(planeAltitude * shadowCoeffX, planeAltitude * shadowCoeffY);

        var planeMoveX = plane.GetMoveX();
        var newSprite = planeMoveX == 0 ? straightSprite : turnSprite;
        if (!plane.IsAlive())
        {
            newSprite = deadSprite;
        }

        if (newSprite != spriteR.sprite)
        {
            spriteR.sprite = newSprite;
        }
    }
}
