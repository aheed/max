using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlipBook : MonoBehaviour
{
    public Sprite[] sprites;
    public float timeToChangeSpriteSec = 0.05f;
    public bool randomSprite = false;
    private SpriteRenderer spriteR;
    private float timerSec;
    private int spriteIndex = 0;

    void ResetSpriteClock() => timerSec = timeToChangeSpriteSec;

    void ChangeSpriteIndex()
    {
        if (randomSprite)
        {
            spriteIndex = Random.Range(0, sprites.Length);
            return;
        }

        if (spriteIndex++ >= sprites.Length)
        {
            spriteIndex = 0;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        spriteR = gameObject.GetComponent<SpriteRenderer>();
        ResetSpriteClock();
    }

    // Update is called once per frame
    void Update()
    {
        timerSec -= Time.deltaTime;
        if (timerSec < 0f)
        {
            ChangeSpriteIndex();
            spriteR.sprite = sprites[spriteIndex];
            ResetSpriteClock();
        }
    }
}
