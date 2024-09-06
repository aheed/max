using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowControl : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        MaxControl maxC = FindObjectOfType <MaxControl>();
        var maxPos = maxC.GetPosition();
        Vector2 newPos = maxPos + new Vector2(2.0f, -3.0f);

        transform.position = newPos;
    }
}
