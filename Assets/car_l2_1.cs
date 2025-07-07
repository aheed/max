using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class car_l2_1 : ManagedObject, IPositionObservable
{
    public Sprite shotSprite;
    static readonly int points = 10;

    void HandleCollision(Collider2D col)
    {
        var collObjName = CollisionHelper.GetObjectWithOverlappingAltitude(this, col.gameObject);
        if (collObjName.StartsWith("bullet"))
        {
            var spriteR = gameObject.GetComponent<SpriteRenderer>();
            spriteR.sprite = shotSprite;
            gameObject.GetComponent<Collider2D>().enabled = false;
            GameState.GetInstance().AddScore(points);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.name.StartsWith("bomb"))
        {
            var bomb = col.gameObject.GetComponent<Bomb>();
            GameState.GetInstance().BombLanded(bomb, gameObject);
            GameState.GetInstance().AddScore(points);
            return;
        }

        HandleCollision(col);
    }

    public Vector2 GetPosition() => transform.position;
    public float GetAltitude() => Altitudes.strafeMaxAltitude / 2;
    public float GetHeight() => Altitudes.strafeMaxAltitude;
}
