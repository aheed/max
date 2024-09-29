using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameEvent
{
    START,
    RESTART_REQUESTED
}

public class GameStateContents
{
    public float speed = 0f;    
    public GameStatus gameStatus = GameStatus.ACCELERATING;
}

public interface IGameStateObserver
{
    void OnGameStatusChanged(GameStatus gameStatus);
    void OnGameEvent(GameEvent gameEvent);
}

public class GameState : MonoBehaviour
{
    public float maxSpeed = 2.0f;
    public float safeTakeoffSpeedQuotient = 0.8f;
    public float acceleration = 0.4f;
    GameStateContents gameStateContents = new GameStateContents();
    public GameStateContents GetStateContents() => gameStateContents;

    List<IGameStateObserver> observers = new List<IGameStateObserver>();

    public void RegisterObserver(IGameStateObserver observer)
    {
        observers.Add(observer);
    }

    public void SetStatus(GameStatus gameStatus)
    {
        if (gameStatus == gameStateContents.gameStatus)
        {
            return;
        }
        
        gameStateContents.gameStatus = gameStatus;

        foreach (var observer in observers)
        {
            observer.OnGameStatusChanged(gameStatus);
        }
    }

    public void ReportEvent(GameEvent gameEvent)
    {
        foreach (var observer in observers)
        {
            observer.OnGameEvent(gameEvent);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
