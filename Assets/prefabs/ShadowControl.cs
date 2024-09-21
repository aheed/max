using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowControl : MonoBehaviour
{
    public float shadowCoeffX = 0.0f;
    public float shadowCoeffY = -1.0f;
    public Sprite turnSprite;
    public Sprite straightSprite;
    private SpriteRenderer spriteR;
    private IPositionObservable plane;

    void Start()
    {
        spriteR = gameObject.GetComponent<SpriteRenderer>();
        plane = PositionObservableHelper.GetPositionObservable(gameObject.transform.parent.gameObject);
    }

    void Update()
    {
        var planeAltitude = plane.GetAltitude();
        transform.localPosition = new Vector3(planeAltitude * shadowCoeffX, planeAltitude * shadowCoeffY);

        var planeMoveX = plane.GetMoveX();
        var newSprite = planeMoveX == 0 ? straightSprite : turnSprite;
        if (newSprite != spriteR.sprite)
        {
            spriteR.sprite = newSprite;
        }
    }    
}
