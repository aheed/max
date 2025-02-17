using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class car_l2_1 : ManagedObject4, IPositionObservable
{
    public Sprite shotSprite;

    void HandleCollision(Collider2D col)
    {
        var collObjName = CollisionHelper.GetObjectWithOverlappingAltitude(this, col.gameObject);
        if (collObjName.StartsWith("bullet"))
        {
            var spriteR = gameObject.GetComponent<SpriteRenderer>();
            spriteR.sprite = shotSprite;
            gameObject.GetComponent<Collider2D>().enabled = false;

            // Todo: report destroyed vehicle for scoring
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
}
