using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mushroom_scroll : MonoBehaviour
{
    float offset = 0f;
    public float verticalSpeed = 0.1f;
    //float minOffset = 0.3f;
    float maxOffset;
    //private SpriteRenderer spriteR;

    // Start is called before the first frame update
    void Start()
    {
        var spriteR = gameObject.GetComponent<SpriteRenderer>();
        maxOffset = spriteR.bounds.size.y / 4;
    }

    // Update is called once per frame
    void Update()
    {
        offset += verticalSpeed * Time.deltaTime;
        if (offset > maxOffset)
        {
            offset = -maxOffset;
        }

        var localPos = transform.localPosition;
        localPos.y = offset;
        transform.localPosition = localPos;
    }
}
