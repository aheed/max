using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class prop : MonoBehaviour
{
    public Sprite leftSprite;
    public Sprite rightSprite;
    private SpriteRenderer spriteR;
    public float spriteSwapIntervalSeconds = 0.1f;
    private float spriteSwapCooldown = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        spriteR = gameObject.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        spriteSwapCooldown -= Time.deltaTime;
        if (spriteSwapCooldown < 0.0f)
        {
            spriteR.sprite = spriteR.sprite == leftSprite ? rightSprite : leftSprite;
            spriteSwapCooldown = spriteSwapIntervalSeconds;
        }
    }
}
