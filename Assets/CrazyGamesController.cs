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

        var gameState = GameState.GetInstance();
        gameState.Subscribe(GameEvent.PAUSE_GAME, CrazySDK.Game.GameplayStop);
        gameState.Subscribe(GameEvent.UNPAUSE_GAME, CrazySDK.Game.GameplayStart);
        gameState.Subscribe(GameEvent.GAME_STATUS_CHANGED, OnGameStatusChanged);
        gameState.GetStateContents().fullScreenButtonVisible = false;
        gameState.ReportEvent(GameEvent.FULLSCREEN_BUTTON_UPDATED);
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
