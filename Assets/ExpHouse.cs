using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteInEditMode]
public class ExpHouse : MonoBehaviour, IPositionObservable, IVip
{
    public FlipBook front;
    public FlipBook roof;
    public FlipBook side;
    public GameObject tl_mask;
    public GameObject tr_mask;
    public GameObject lr_mask;
    public Target targetPrefab;
    public float targetOffset;
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

        float half_mask_width = depth * q / 2;
        
        float tl_x = xOffset + half_mask_width;
        float tl_y = yOffset + half_mask_width + fronth;
        tl_mask.transform.localPosition = new Vector2(tl_x, tl_y);

        float tr_x = tl_x + frontw;
        float tr_y = tl_y;
        tr_mask.transform.localPosition = new Vector2(tr_x, tr_y);

        float lr_x = tr_x;
        float lr_y = yOffset + half_mask_width;
        lr_mask.transform.localPosition = new Vector2(lr_x, lr_y);
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

    // Start is called before the first frame update
    void Start()
    {
        SetSize(5, 2, 2); //TEMP
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        var collObjName = CollisionHelper.GetObjectWithOverlappingAltitude(this, col.gameObject);
        if (!collObjName.StartsWith("bomb"))
        {
            return;
        }

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
    public float GetAltitude() => 0.1f;
    public float GetHeight() => 0.4f;
}
