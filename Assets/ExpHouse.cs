using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class ExpHouse : MonoBehaviour, IPositionObservable, IVip
{
    public FlipBook front;
    public FlipBook roof;
    public FlipBook side;
    public GameObject tl_mask;
    public GameObject tr_mask;
    public GameObject lr_mask;
    public GameObject tr_side_mask;
    public Target targetPrefab;
    float targetOffset = 0.25f;
    Target target;
    
    public float q = 0.16f; // size quantum

    public void SetSize(int width, int height, int depth)
    {
        float xOffset = - (width + depth) * q / 2;
        float yOffset = - (height + depth) * q / 2;

        float frontw = width * q;
        float fronth = height * q;
        float frontX = xOffset + width * q / 2;
        float frontY = yOffset + height * q / 2;

        float roofw = (width + depth) * q;
        float roofh = depth * q;
        float roofx = xOffset + roofw / 2;
        float roofy = yOffset + fronth + roofh / 2;

        float sidew = depth * q;
        float sideh = (height + depth) * q;
        float sidex = xOffset + frontw + sidew / 2;
        float sidey = yOffset + sideh / 2;

        SpriteRenderer frontRenderer = front.GetComponent<SpriteRenderer>();
        SpriteRenderer roofRenderer = roof.GetComponent<SpriteRenderer>();
        SpriteRenderer sideRenderer = side.GetComponent<SpriteRenderer>();

        frontRenderer.size = new Vector2(frontw, fronth);
        front.transform.localPosition = new Vector2(frontX, frontY);

        roofRenderer.size = new Vector2(roofw, roofh);
        roof.transform.localPosition = new Vector2(roofx, roofy);

        sideRenderer.size = new Vector2(sidew, sideh);
        side.transform.localPosition = new Vector2(sidex, sidey);

        var mask_scale = new Vector3((float)depth, (float)depth, (float)depth);
        tl_mask.transform.localScale = mask_scale;
        tr_mask.transform.localScale = mask_scale;
        lr_mask.transform.localScale = mask_scale;
        tr_side_mask.transform.localScale = mask_scale;        

        float half_mask_width = depth * q / 2;
        
        float tl_x = xOffset + half_mask_width;
        float tl_y = yOffset + half_mask_width + fronth;
        tl_mask.transform.localPosition = new Vector2(tl_x, tl_y);

        float tr_x = tl_x + frontw;
        float tr_y = tl_y;
        tr_mask.transform.localPosition = new Vector2(tr_x, tr_y);
        tr_side_mask.transform.localPosition = new Vector2(tr_x, tr_y);

        float lr_x = tr_x;
        float lr_y = yOffset + half_mask_width;
        lr_mask.transform.localPosition = new Vector2(lr_x, lr_y);

        PolygonCollider2D collider = GetComponent<PolygonCollider2D>();
        var colliderPoints = new Vector2[6];

        //clockwise, starting in lower left corner
        float x = xOffset;
        float y = yOffset;
        colliderPoints[0] = new Vector2(x, y);

        y += fronth;
        colliderPoints[1] = new Vector2(x, y);

        x += roofh;
        y += roofh;
        colliderPoints[2] = new Vector2(x, y);

        x += frontw;
        colliderPoints[3] = new Vector2(x, y);
        
        y -= fronth;
        colliderPoints[4] = new Vector2(x, y);
        
        x -= roofh;
        y -= roofh;
        colliderPoints[5] = new Vector2(x, y);

        collider.points = colliderPoints;

        targetOffset = yOffset + fronth + roofh / 2;
    }

    public void SetVip()
    {
        target = Instantiate(targetPrefab, gameObject.transform);
        var localPos = target.transform.localPosition;
        localPos.y += targetOffset;
        target.transform.localPosition = localPos;
    }

    public bool IsVip()
    {
        return target != null;
    }

    public void SetColor(Color color)
    {
        SpriteRenderer frontRenderer = front.GetComponent<SpriteRenderer>();
        SpriteRenderer sideRenderer = side.GetComponent<SpriteRenderer>();

        frontRenderer.color = color;
        //var sideColor = color * 0.8f;
        var sideColor = new Color (color.r * 0.8f, color.g * 0.8f, color.b * 0.8f);
        sideRenderer.color = sideColor;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    void OnTriggerEnter2D(Collider2D col)
    {
        var collObjName = CollisionHelper.GetObjectWithOverlappingAltitude(this, col.gameObject);
        if (!collObjName.StartsWith("bomb"))
        {
            return;
        }

        SetColor(Color.white);
        front.Activate();
        side.Activate();
        roof.Activate();
        if (target != null)
        {
            Destroy(target.gameObject);
            target = null;
        }
    }

    public Vector2 GetPosition() => transform.position;
    public float GetAltitude() => Altitudes.unsafeAltitude / 2;
    public float GetHeight() => Altitudes.unsafeAltitude;
}
