using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : ManagedObject, IPositionObservable
{
    public Sprite normalSprite;
    public Sprite shotSprite;
    private bool shot = false;

    void HandleCollision(Collider2D col)
    {
        var collObjName = CollisionHelper.GetObjectWithOverlappingAltitude(this, col.gameObject);
        if (collObjName.StartsWith("bullet"))
        {
            shot = true;
            var spriteR = gameObject.GetComponent<SpriteRenderer>();
            spriteR.sprite = shotSprite;
            gameObject.GetComponent<Collider2D>().enabled = false;
            var gameState = GameState.GetInstance();
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
            GameState.GetInstance().BombLanded(bomb, gameObject);
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

        var spriteR = gameObject.GetComponent<SpriteRenderer>();
        spriteR.sprite = normalSprite;
        gameObject.GetComponent<Collider2D>().enabled = true;
    }
}
