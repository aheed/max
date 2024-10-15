using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlopAnimation : MonoBehaviour
{
    public float horizSpeed = 0.1f;
    float offset = 0f;
    float maxOffset;

    // Start is called before the first frame update
    void Start()
    {
        var spriteM = gameObject.GetComponent<SpriteMask>();
        maxOffset = spriteM.bounds.size.x / 4;
        
    }

    // Update is called once per frame
    void Update()
    {
        offset += horizSpeed * Time.deltaTime;
        if (offset > maxOffset)
        {
            offset = -maxOffset;
        }

        var localPos = transform.localPosition;
        localPos.x = offset;
        transform.localPosition = localPos;
    }
}
