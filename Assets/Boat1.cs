using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boat1 : ManagedObject, IPositionObservable
{
    static readonly int maxHealth = 3;
    public GameObject sunkBoatPrefab;
    GameState gameState;
    int health = maxHealth;
    static readonly int points = 50;

    // Start is called before the first frame update
    void Start()
    {
        gameState = GameState.GetInstance();
    }

    public override void Deactivate()
    {
        gameObject.GetComponent<Collider2D>().enabled = false;
        gameObject.SetActive(false);
    }

    public override void Reactivate()
    {
        gameObject.GetComponent<Collider2D>().enabled = true;
        gameObject.SetActive(true);
        health = maxHealth;
    }

    void Sink()
    {
        var parent = transform.parent;
        Instantiate(sunkBoatPrefab, transform.position, Quaternion.identity, parent);
        Release();
        gameState.AddScore(points);
    }

    void HandleCollision(Collider2D col)
    {
        var collObjName = CollisionHelper.GetObjectWithOverlappingAltitude(this, col.gameObject);
        if (collObjName.StartsWith("bullet"))
        {
            --health;
            if (health <= 0)
            {
                gameState.ReportEvent(GameEvent.MEDIUM_BANG);
                Sink();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.name.StartsWith("bomb"))
        {
            var bomb = col.gameObject.GetComponent<Bomb>();
            gameState.BombLanded(bomb, null);
            gameState.ReportEvent(GameEvent.SMALL_DETONATION);
            gameState.ReportEvent(GameEvent.MEDIUM_BANG);
            Sink();
            return;
        }

        HandleCollision(col);
    }

    public Vector2 GetPosition() => transform.position;
    public float GetAltitude() => Altitudes.unsafeAltitude / 2;
    public float GetHeight() => Altitudes.unsafeAltitude;
}
