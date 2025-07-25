using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHQ : MonoBehaviour, IPositionObservable, IEnemyHQ
{
    public Sprite bombedSprite;
    private SpriteRenderer spriteR;
    private bool bombed = false;
    static readonly int points = 250;

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

        bombed = true;
    }

    public bool IsBombed() => bombed;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.name.StartsWith("bomb"))
        {
            return;
        }

        SetBombed();
        var gameState = GameState.GetInstance();
        gameState.ReportEvent(GameEvent.BIG_DETONATION);
        gameState.ReportEvent(GameEvent.BIG_BANG);
        gameState.TargetHit();
        gameState.AddScore(points);
    }

    public Vector2 GetPosition() => transform.position;
    public float GetAltitude() => Altitudes.unsafeAltitude / 2;
    public float GetHeight() => Altitudes.unsafeAltitude;
}
