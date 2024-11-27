using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boat1 : MonoBehaviour, IPositionObservable
{
    public GameObject sunkBoatPrefab;
    GameState gameState;
    int health = 3;

    // Start is called before the first frame update
    void Start()
    {
        gameState = FindObjectOfType<GameState>();
    }

    void Sink()
    {
        var collider = gameObject.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        var parent = gameObject.transform.parent;
        Instantiate(sunkBoatPrefab, transform.position, Quaternion.identity, parent);
        gameObject.SetActive(false);

        // Todo: report destroyed boat for scoring
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
