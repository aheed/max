using UnityEngine;

public class DamTarget : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.name.StartsWith("Bomb"))
        {
            return;
        }

        var gameState = GameState.GetInstance();
        gameState.ReportEvent(GameEvent.BIG_DETONATION);
        gameState.ReportEvent(GameEvent.BIG_BANG);
        gameState.TargetHit();
    }
}
