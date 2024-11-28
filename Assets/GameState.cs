using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum GameEvent
{
    START,
    RESTART_REQUESTED,
    SPEED_CHANGED,
    ALT_CHANGED,
    DAMAGE_SUSTAINED,
    DAMAGE_REPAIRED,
    BOMBS_CHANGED,
    SCORE_CHANGED,
    SMALL_DETONATION,
    BIG_DETONATION,
    LANDING_CHANGED,
    WIND_CHANGED,
    SMALL_BANG,
    MEDIUM_BANG,
    BIG_BANG,
    TARGET_HIT
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
    public static Vector2[] windDirections = new Vector2[] {new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(-1f, 1f), new Vector2(-1f, 0f)};

    public float speed = 0f;    
    public GameStatus gameStatus = GameStatus.ACCELERATING;
    public float altitude = 0f;
    public float fuel = 0f;
    public int bombs = 0;
    public int score = 0;
    public bool approachingLanding = false;
    public bool wind = false;
    public bool[] damages = new bool[] { false, false, false, false};
    public Vector2 windDirection = new Vector2(0f, 0f);
    public int targetsHit;
    public int targetsHitMin;
    public List<EnemyHQ> enemyHQs;
    public LevelPrerequisite latestLevelPrereq;
}

public interface IGameStateObserver
{
    void OnGameStatusChanged(GameStatus gameStatus);
    void OnGameEvent(GameEvent gameEvent);
    void OnBombLanded(Bomb bomb, GameObject hitObject);
    void OnEnemyPlaneStatusChanged(EnemyPlane enemyPlane, bool active);
}

public class GameState : MonoBehaviour
{
    public float maxSpeed = 2.0f;
    public float minAltitude = 0.1f;
    public float maxAltitude = 2.0f;
    public float minSafeAltitude = 0.3f;
    public float maxHorizPosition = 2.0f;
    public float safeTakeoffSpeedQuotient = 0.8f;
    public float acceleration = 0.4f;
    public static float horizontalSpeed = 3.0f;
    public static float verticalSpeed = 2.0f;
    public static float windSpeed = 0.2f;
    public static string landingAlert = "L";
    public static string windAlert = "W";
    public static string enemyPlaneAlert = "P";
    public int maxBombs = 30;
    public float maxFuel = 100f;
    public float startFuelQuotient = 0.90f;
    public int targetsHitMin1 = 10;
    public int targetsHitMin2 = 10;

    GameStateContents gameStateContents = new GameStateContents();
    public GameStateContents GetStateContents() => gameStateContents;

    List<IGameStateObserver> observers = new List<IGameStateObserver>();

    public void RegisterObserver(IGameStateObserver observer)
    {
        observers.Add(observer);
    }

    public void UnregisterObserver(IGameStateObserver observer)
    {
        observers.Remove(observer);
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
        }
    }

    public void SetApproachingLanding(bool approachingLanding)
    {
        if (approachingLanding != gameStateContents.approachingLanding)
        {
            gameStateContents.approachingLanding = approachingLanding;
            ReportEvent(GameEvent.LANDING_CHANGED);
        }
    }

    public void SetWind(bool wind)
    {
        if (wind != gameStateContents.wind)
        {
            gameStateContents.wind = wind;
            ReportEvent(GameEvent.WIND_CHANGED);
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

    public void BombLanded(Bomb bomb, GameObject hitObject = null)
    {
        foreach (var observer in observers)
        {
            observer.OnBombLanded(bomb, hitObject);
        }
    }

    public void EnemyPlaneStatusChanged(EnemyPlane enemyPlane, bool active)
    {
        foreach (var observer in observers)
        {
            observer.OnEnemyPlaneStatusChanged(enemyPlane, active);
        }
    }

    public bool GotDamage(DamageIndex letter) => gameStateContents.damages[(int)letter];

    public void SetRandomDamage(bool damage)
    {        
        var nofDamages = gameStateContents.damages.Length;
        var index = UnityEngine.Random.Range(0, nofDamages);
        var candidates = 0;
        while (candidates < nofDamages)
        {
            if (gameStateContents.damages[index] != damage)
            {
                gameStateContents.damages[index] = damage;
                break;
            }
            index = (index + 1) % nofDamages;
            candidates++;
        }

        ReportEvent(damage ? GameEvent.DAMAGE_SUSTAINED : GameEvent.DAMAGE_REPAIRED);

        if (damage && candidates >= nofDamages)
        {
            SetStatus(GameStatus.KILLED_BY_FLACK);
        }
    }

    public void SetTargetsHit(int hits, int hitsMin)
    {
        gameStateContents.targetsHit = hits;
        gameStateContents.targetsHitMin = hitsMin;
        ReportEvent(GameEvent.TARGET_HIT);
    }

    public void IncrementTargetsHit()
    {
        SetTargetsHit(gameStateContents.targetsHit + 1,
            gameStateContents.targetsHitMin); // unchanged
    }

    public int GetTargetsHit()
    {
        return gameStateContents.latestLevelPrereq.levelType == LevelType.CITY && gameStateContents.enemyHQs != null ?
            gameStateContents.enemyHQs.Where(h => h.IsBombed()).Count() :
            gameStateContents.targetsHit;
    }

    public void Reset()
    {
        gameStateContents.speed = 0f;
        gameStateContents.gameStatus = GameStatus.REFUELLING;
        gameStateContents.altitude = minAltitude;
        gameStateContents.fuel = maxFuel * startFuelQuotient;
        gameStateContents.bombs = maxBombs;
        gameStateContents.score = 0;
        gameStateContents.wind = false;
        gameStateContents.damages = new bool[] { false, false, false, false};
        gameStateContents.targetsHit = 0;
        gameStateContents.targetsHitMin = 0;
        gameStateContents.latestLevelPrereq = null;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }
}
