using System.Globalization;
using UnityEngine;

public class EnemyHQ3d : MonoBehaviour, IEnemyHQ
{
    private bool bombed = false;

    public void SetBombed()
    {
        var collider = gameObject.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        var aliveGameObject = transform.GetChild(0);
        aliveGameObject.gameObject.SetActive(false);
        var bombedGameObject = transform.GetChild(1);
        bombedGameObject.gameObject.SetActive(true);

        bombed = true;
    }

    public bool IsBombed() => bombed;

    void OnTriggerEnter(Collider col)
    {
        if (!col.name.StartsWith("bomb", true, CultureInfo.InvariantCulture))
        {
            return;
        }

        Destroy(col.gameObject);
        SetBombed();
        var gameState = GameState.GetInstance();
        gameState.ReportEvent(GameEvent.BIG_DETONATION);
        gameState.ReportEvent(GameEvent.BIG_BANG);
        gameState.TargetHit();
    }
}
