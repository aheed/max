using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHangar : ManagedObject
{
    public FlipBook bombed;
    static readonly int points = 50;

    void OnTriggerEnter2D(Collider2D col)
    {        
        if (!col.name.StartsWith("bomb"))
        {
            return;
        }

        bombed.Activate();
        
        gameObject.GetComponent<Collider2D>().enabled = false;

        var gameState = GameState.GetInstance();
        gameState.ReportEvent(GameEvent.SMALL_DETONATION);
        gameState.ReportEvent(GameEvent.MEDIUM_BANG);

        gameState.AddScore(points);
    }
}
