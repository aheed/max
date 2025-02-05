using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TvSimEventHandler : MonoBehaviour, IGameStateObserver
{   
    TvSimDocument tvSimDocument;

    void Start()
    {
        // Assume the parent game object is a TvSimDocument
        tvSimDocument = InterfaceHelper.GetInterface<TvSimDocument>(transform.parent.gameObject);
        GameState.GetInstance().RegisterObserver(this);
    }

    public void OnGameEvent(GameEvent gameEvent)
    {
        if (gameEvent == GameEvent.VIEW_MODE_CHANGED)
        {
            tvSimDocument.OnViewModeChanged();
        }
    }

    public void OnGameStatusChanged(GameStatus gameStatus) {}

    public void OnBombLanded(GameObject bomb, GameObject hitObject) {}

    public void OnEnemyPlaneStatusChanged(EnemyPlane enemyPlane, bool active) {}
}