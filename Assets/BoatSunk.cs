using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatSunk : MonoBehaviour
{
    public float lifeSpanSec = 1.2f;
    float timeToLiveSec;
    private SpriteRenderer spriteR;

    // Start is called before the first frame update
    void Start()
    {
        spriteR = gameObject.GetComponent<SpriteRenderer>();
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

        var newOpacity = timeToLiveSec / lifeSpanSec;
        var newColor = new Color(1f, 1f, 1f, newOpacity);
        spriteR.color = newColor;
    }
}
