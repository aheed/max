using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatSunk : MonoBehaviour
{
    public float lifeSpanSec = 1.2f;
    float timeToLiveSec;

    // Start is called before the first frame update
    void Start()
    {
        timeToLiveSec = lifeSpanSec;    
    }

    // Update is called once per frame
    void Update()
    {
        timeToLiveSec -= Time.deltaTime;
        if (timeToLiveSec < 0f)
        {
            Destroy(gameObject);
        }
    }
}
