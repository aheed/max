using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameEvent
{
    START,
    RESTART_REQUESTED,
    SPEED_CHANGED,
    ALT_CHANGED,
    DAMAGE_CHANGED,
    FUEL_CHANGED,
    ALERT,
    BOMBS_CHANGED,
    SCORE_CHANGED,
}

public enum DamageIndex
{
    F = 0,
    B,
    M,
    G
}

public class GameStateContents
{
    public float speed = 0f;    
    public GameStatus gameStatus = GameStatus.ACCELERATING;
    public float altitude = 0f;
    public float fuel = 0f;
    public int bombs = 0;
    public int score = 0;
    public string alert = "";
    public bool[] damages = new bool[] { false, false, false, false};
}

public interface IGameStateObserver
{
    void OnGameStatusChanged(GameStatus gameStatus);
    void OnGameEvent(GameEvent gameEvent);
}

public class GameState : MonoBehaviour
{
    public float maxSpeed = 2.0f;
    public float maxAltitude = 2.0f;
    public float safeTakeoffSpeedQuotient = 0.8f;
    public float acceleration = 0.4f;
    public int maxBombs = 30;
    public float maxFuel = 100f;
    public float startFuelQuotient = 0.90f;

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

    public void SetSpeed(float speed)
    {
        if (speed != gameStateContents.speed)
        {
            gameStateContents.speed = speed;
            ReportEvent(GameEvent.SPEED_CHANGED);
        }
    }

    public void SetAltitude(float altitude)
    {
        if (altitude != gameStateContents.altitude)
        {
            gameStateContents.altitude = altitude;
            ReportEvent(GameEvent.ALT_CHANGED);
        }
    }

    public void IncrementBombs(int deltaBombs)
    {
        gameStateContents.bombs += deltaBombs;
        ReportEvent(GameEvent.BOMBS_CHANGED);
    }

    public void SetFuel(float fuel)
    {
        if (fuel != gameStateContents.fuel)
        {
            gameStateContents.fuel = fuel;
            ReportEvent(GameEvent.FUEL_CHANGED);
        }
    }

    public float GetSafeTakeoffSpeed() => safeTakeoffSpeedQuotient * maxSpeed;

    public void ReportEvent(GameEvent gameEvent)
    {
        foreach (var observer in observers)
        {
            observer.OnGameEvent(gameEvent);
        }
    }

    public bool GotDamage(DamageIndex letter) => gameStateContents.damages[(int)letter];

    public void SetRandomDamage()
    {
        var nofDamages = gameStateContents.damages.Length;
        var index = UnityEngine.Random.Range(0, nofDamages);
        var candidates = 0;
        while (candidates < nofDamages)
        {
            if (!gameStateContents.damages[index])
            {
                gameStateContents.damages[index] = true;
                ReportEvent(GameEvent.DAMAGE_CHANGED);
                return;
            }
            index = (index + 1) % nofDamages;
            candidates++;
        }
        SetStatus(GameStatus.KILLED_BY_FLACK);
    }

    public void Reset()
    {
        gameStateContents.speed = 0f;
        gameStateContents.gameStatus = GameStatus.REFUELLING;
        gameStateContents.altitude = 0f;
        gameStateContents.fuel = maxFuel * startFuelQuotient;
        gameStateContents.bombs = maxBombs;
        gameStateContents.score = 0;
        gameStateContents.alert = "";
        gameStateContents.damages = new bool[] { false, false, false, false};
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
