using CrazyGames;
using UnityEngine;

public class CrazyGamesController : MonoBehaviour
{
    void Start()
    {
        if (!CrazySDK.IsInitialized)
        {
            return;
        }

        GameState.GetInstance().Subscribe(GameEvent.PAUSE_GAME, CrazySDK.Game.GameplayStop);
        GameState.GetInstance().Subscribe(GameEvent.UNPAUSE_GAME, CrazySDK.Game.GameplayStart);
        GameState.GetInstance().Subscribe(GameEvent.GAME_STATUS_CHANGED, OnGameStatusChanged);
        CrazySDK.Game.GameplayStart();
    }

    void OnGameStatusChanged()
    {
        var status = GameState.GetInstance().GetStateContents().gameStatus;
        if (status == GameStatus.FINISHED)
        {
            CrazySDK.Game.HappyTime();
        }
    }

    void OnDestroy()
    {
        if (CrazySDK.IsInitialized)
        {
            CrazySDK.Game.GameplayStop();
        }
    }
}
