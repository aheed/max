using UnityEngine;

public class DamTarget : MonoBehaviour
{
    static readonly int points = 250;

    void OnTriggerEnter(Collider collider)
    {
        Debug.Log($"********* 3D Dam Target Hit!!!!!!!!!!!!!!! with {collider.gameObject.name}");
        if (!collider.gameObject.name.StartsWith("Bomb"))
        {
            return;
        }

        var meshCollider = GetComponentInChildren<MeshCollider>();
        meshCollider.enabled = false;
        var gameState = GameState.GetInstance();
        gameState.ReportEvent(GameEvent.BIG_DETONATION);
        gameState.ReportEvent(GameEvent.BIG_BANG);
        gameState.TargetHit();
        gameState.AddScore(points);
    }
}
