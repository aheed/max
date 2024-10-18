using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    VipBlinker vipBlinker;

    // Start is called before the first frame update
    void Start()
    {
        vipBlinker = new(gameObject.GetComponent<SpriteRenderer>());    
    }

    // Update is called once per frame
    void Update()
    {
        vipBlinker.Update(Time.deltaTime);
    }
}
