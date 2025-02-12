using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : ManagedObject3, IPositionObservable
{
    public Sprite normalSprite;
    public Sprite shotSprite;
    private SpriteRenderer spriteR;
    private bool shot = false;

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
            shot = true;
            spriteR.sprite = shotSprite;
            if (gameObject.TryGetComponent<Collider2D>(out var collider))
            {
                collider.enabled = false;
            }
            var gameState = FindAnyObjectByType<GameState>();
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
            FindAnyObjectByType<GameState>().BombLanded(bomb, gameObject);
            return;
        }

        HandleCollision(col);
    }

    public Vector2 GetPosition() => transform.position;
    public float GetAltitude() => Altitudes.strafeMaxAltitude / 2;
    public float GetHeight() => Altitudes.strafeMaxAltitude;

    // Override
    public override void Reactivate()
    {
        if (!shot)
        {
            return;
        }
        shot = false;

        spriteR.sprite = normalSprite;
        if (gameObject.TryGetComponent<Collider2D>(out var collider))
        {
            collider.enabled = false;
        }
    }
}
