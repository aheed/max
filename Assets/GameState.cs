using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateContents
{
    public float speed = 0f;    
    public GameStatus gameStatus = GameStatus.ACCELERATING;
}

public interface IGameStateObserver
{
    void OnGameStatusChanged(GameStatus gameStatus);
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
        gameStateContents.gameStatus = gameStatus;

        foreach (var observer in observers)
        {
            observer.OnGameStatusChanged(gameStatus);
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
