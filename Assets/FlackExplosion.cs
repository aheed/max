using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class FlackExplosion : MonoBehaviour
{    
    public Sprite[] sprites;
    private SpriteRenderer spriteR;
    public float lifeSpanSec = 2.0f;
    float timeToChangeSpriteSec;
    public int totalSpriteChanges = 10;
    int spriteChangesRemaining;    

    void ResetSpriteClock() => timeToChangeSpriteSec = lifeSpanSec / totalSpriteChanges;

    // Start is called before the first frame update
    void Start()
    {
        ResetSpriteClock();
        spriteChangesRemaining = totalSpriteChanges; 
        spriteR = gameObject.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        timeToChangeSpriteSec -= Time.deltaTime;
        if (timeToChangeSpriteSec < 0f)
        {
            spriteChangesRemaining--;
            if (spriteChangesRemaining <= 0)
            {
                Destroy(gameObject);
                return;
            }

            var newSpriteIndex = Random.Range(0, sprites.Length);
            spriteR.sprite = sprites[newSpriteIndex];
            ResetSpriteClock();
        }
    }
}
