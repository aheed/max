using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPlaneExplosionTimed : MonoBehaviour
{
    public Sprite[] sprites;
    public float lifeSpanSec = 1.2f;
    float timeToLiveSec;
    private SpriteRenderer spriteR;

    // Start is called before the first frame update
    void Start()
    {
        spriteR = gameObject.GetComponent<SpriteRenderer>();
        var spriteIndex = Random.Range(0, sprites.Length);
        spriteR.sprite = sprites[spriteIndex];
        var newColor = new Color(1f, 0.7f + Random.Range(0, 0.3f), 0f, 1f);
        spriteR.color = newColor;
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
