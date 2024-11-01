using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VipTarget : MonoBehaviour, IPositionObservable
{
    public Sprite bombedSprite;
    GameState gameState;
    private SpriteRenderer spriteR;

    public void SetBombed()
    {
        if (spriteR == null)
        {
            spriteR = gameObject.GetComponent<SpriteRenderer>();
        }
        
        spriteR.sprite = bombedSprite;
        var collider = gameObject.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (gameState == null)
        {
            gameState = FindObjectOfType<GameState>();
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.name.StartsWith("bomb"))
        {
            return;
        }

        SetBombed();

        // Todo: report destroyed VIP target
    }

    public Vector2 GetPosition() => transform.position;
    public float GetAltitude() => Altitudes.unsafeAltitude / 2;
    public float GetHeight() => Altitudes.unsafeAltitude;
}
