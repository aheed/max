using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class car_l2_1 : MonoBehaviour, IPositionObservable
{
    public Sprite shotSprite;
    GameState gameState;
    private SpriteRenderer spriteR;

    // Start is called before the first frame update
    void Start()
    {
        spriteR = gameObject.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameState == null)
        {
            gameState = FindObjectOfType<GameState>();
        }
    }

    void HandleCollision(Collider2D col)
    {
        var collObjName = CollisionHelper.GetObjectWithOverlappingAltitude(this, col.gameObject);
        if (collObjName.StartsWith("bullet"))
        {
            spriteR.sprite = shotSprite;
            var collider = gameObject.GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            // Todo: report destroyed vehicle for scoring
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.name.StartsWith("bomb"))
        {
            var bomb = col.gameObject.GetComponent<Bomb>();
            gameState.BombLanded(bomb, gameObject);
            return;
        }

        HandleCollision(col);
    }

    public Vector2 GetPosition() => transform.position;
    public float GetAltitude() => 0.1f;
    public float GetHeight() => 0.41f;
}
