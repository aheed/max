using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour, IPositionObservable
{
    public Sprite shotSprite;
    private SpriteRenderer spriteR;

    // Start is called before the first frame update
    void Start()
    {
        spriteR = gameObject.GetComponent<SpriteRenderer>();
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
            var gameState = FindObjectOfType<GameState>();
            gameState.ReportEvent(GameEvent.SMALL_DETONATION);
            gameState.ReportEvent(GameEvent.SMALL_BANG);

            // Todo: report destroyed tank for scoring
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.name.StartsWith("bomb"))
        {
            var bomb = col.gameObject.GetComponent<Bomb>();
            FindObjectOfType<GameState>().BombLanded(bomb, gameObject);
            return;
        }

        HandleCollision(col);
    }

    public Vector2 GetPosition() => transform.position;
    public float GetAltitude() => Altitudes.strafeMaxAltitude / 2;
    public float GetHeight() => Altitudes.strafeMaxAltitude;
}
