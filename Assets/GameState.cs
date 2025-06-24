using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum GameEvent
{
    START,
    RESTART_REQUESTED,
    RESTART_TIMER_EXPIRED,
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
    TARGETS_CHANGED,
    TARGET_HIT,
    VIEW_MODE_CHANGED,
    BOMB_LANDED,
    GAME_STATUS_CHANGED,
    ENEMY_PLANE_STATUS_CHANGED,
    DEBUG_ACTION1,
    DEBUG_ACTION2,
    DEBUG_ACTION3,
    BULLET_FIRED,
    BOMB_DROPPED,
}

public enum DamageIndex
{
    F = 0,
    B,
    M,
    G
}

public enum ViewMode
{
    NORMAL = 0,
    TV_SIM,
}

public class BombLandedEventArgs
{
    public GameObject bomb;
    public GameObject hitObject;
}

public class GameStateContents
{
    public static Vector2[] windDirections = new Vector2[] { new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(-1f, 1f), new Vector2(-1f, 0f) };

    public float speed = 0f;
    public GameStatus gameStatus = GameStatus.ACCELERATING;
    public float altitude = 0f;
    public float floorAltitude = 0f;
    public float fuel = 0f;
    public int bombs = 0;
    public int score = 0;
    public bool approachingLanding = false;
    public bool wind = false;
    public bool[] damages = new bool[] { false, false, false, false };
    public Vector2 windDirection = new Vector2(0f, 0f);
    public int targetsHit;
    public int targetsHitMin;
    public List<IEnemyHQ> enemyHQs;
    public LevelPrerequisite latestLevelPrereq;
    public Dictionary<GameObject, float> enemyPlaneAltitudes;
    public float maxAltitudeDiffForPlaneCollision;
    public bool bossDefeated;
    public GameObject boss;
    public float restartCoolDownSeconds = 0f;
}

public class GameState : MonoBehaviour
{
    public static Material carBlinkMaterial;
    public static Material boatBlinkMaterial;
    public static Material planeBlinkMaterial;
    public static Material targetBlinkMaterial;
    public float maxSpeed = 2.0f;
    public float minAltitude = 0.1f;
    public float maxAltitude = 2.0f;
    public float minSafeAltitude = 0.2f;
    public float riverAltitude = -0.3f;
    public float craterAltitude = 0.01f;
    public float searchLightAltitude = 0.15f;
    public float maxHorizPosition = 2.0f;
    public float safeTakeoffSpeedQuotient = 0.8f;
    public float acceleration = 0.4f;
    public ViewMode viewMode = ViewMode.NORMAL;
    public static float horizontalSpeed = 4.0f;
    public static float verticalSpeed = 2.5f;
    public static float windSpeed = 0.2f;
    public static string landingAlert = "L";
    public static string windAlert = "W";
    public static string enemyPlaneAlert = "P";
    public static float minRestartWaitSeconds = 2.0f;
    public int maxBombs = 30;
    public float maxFuel = 100f;
    public float startFuelQuotient = 0.90f;
    public int targetsHitMin1 = 10;
    public int targetsHitMin2 = 10;
    GameStateContents gameStateContents = new GameStateContents();
    public GameStateContents GetStateContents() => gameStateContents;
    static GameState singletonInstance;
    public Vector3 playerPosition;
    private EventPubSubNoArg pubSub = new();
    private EventPubSub<BombLandedEventArgs> bombLandedPubSub = new();

    public void SetPlaneHeights(float playerPlaneHeight, float enemyPlaneHeight)
    {
        gameStateContents.maxAltitudeDiffForPlaneCollision =
            (playerPlaneHeight + enemyPlaneHeight) / 2;
    }
    
    public void Subscribe(GameEvent gameEvent, Action callback)
    {
        pubSub.Subscribe(gameEvent, callback);
    }

    public void Unsubscribe(GameEvent gameEvent, Action callback)
    {
        pubSub.Unsubscribe(gameEvent, callback);
    }

    public void SubscribeToBombLandedEvent(Action<BombLandedEventArgs> callback)
    {
        bombLandedPubSub.Subscribe(GameEvent.BOMB_LANDED, callback);
    }

    public static GameState GetInstance()
    {
        if (singletonInstance == null)
        {
            singletonInstance = FindAnyObjectByType<GameState>();
        }
        return singletonInstance;
    }

    public bool IsNightTime()
    {
        return gameStateContents.latestLevelPrereq?.nightTime ?? false;
    }

    public void SetStatus(GameStatus gameStatus)
    {
        if (gameStatus == gameStateContents.gameStatus)
        {
            return;
        }

        gameStateContents.gameStatus = gameStatus;
        Debug.Log($"New State: {gameStatus}");

        if (gameStatus == GameStatus.DEAD ||
           gameStatus == GameStatus.FINISHED)
        {
            SetSpeed(0f);
            gameStateContents.restartCoolDownSeconds = minRestartWaitSeconds;
        }

        pubSub.Publish(GameEvent.GAME_STATUS_CHANGED);
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

    public void SetViewMode(ViewMode viewMode)
    {
        this.viewMode = viewMode;
        ReportEvent(GameEvent.VIEW_MODE_CHANGED);
    }

    public float GetSafeTakeoffSpeed() => safeTakeoffSpeedQuotient * maxSpeed;

    public void ReportEvent(GameEvent gameEvent)
    {
        pubSub.Publish(gameEvent);
    }

    public void BombLanded(Bomb bomb, GameObject hitObject = null)
    {
        BombLanded(bomb == null ? null : bomb.gameObject, hitObject);
    }

    public void BombLanded(GameObject bomb, GameObject hitObject = null)
    {
        bombLandedPubSub.Publish(GameEvent.BOMB_LANDED, new BombLandedEventArgs { bomb = bomb, hitObject = hitObject });
    }

    public void AddEnemyPlane(GameObject enemyPlane, float altitude)
    {
        gameStateContents.enemyPlaneAltitudes[enemyPlane] = altitude;
        pubSub.Publish(GameEvent.ENEMY_PLANE_STATUS_CHANGED);
    }

    public void RemoveEnemyPlane(GameObject enemyPlane)
    {
        gameStateContents.enemyPlaneAltitudes.Remove(enemyPlane);
        pubSub.Publish(GameEvent.ENEMY_PLANE_STATUS_CHANGED);
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

        if (!damage && gameStateContents.gameStatus == GameStatus.KILLED_BY_FLACK)
        {
            SetStatus(GameStatus.FLYING);
        }
    }

    public void SetTargetsHit(int hits, int hitsMin)
    {
        gameStateContents.targetsHit = hits;
        gameStateContents.targetsHitMin = hitsMin;
        ReportEvent(GameEvent.TARGETS_CHANGED);
    }

    public void TargetHit()
    {
        ++gameStateContents.targetsHit;
        ReportEvent(GameEvent.TARGET_HIT);
    }

    public int GetTargetsHit()
    {
        if (gameStateContents.latestLevelPrereq.levelType == LevelType.ROBOT_BOSS ||
            gameStateContents.latestLevelPrereq.levelType == LevelType.RED_BARON_BOSS)
        {
            return gameStateContents.bossDefeated ? 1 : 0;
        }

        return gameStateContents.latestLevelPrereq.levelType == LevelType.CITY && gameStateContents.enemyHQs != null ?
            gameStateContents.enemyHQs.Where(h => h.IsBombed()).Count() :
            gameStateContents.targetsHit;
    }

    public bool IsGameOver()
    {
        return gameStateContents.gameStatus == GameStatus.DEAD;
    }

    public void ReportBossDefeated()
    {
        gameStateContents.bossDefeated = true;
        ReportEvent(GameEvent.TARGETS_CHANGED);
    }

    public void Reset()
    {
        gameStateContents.speed = 0f;
        gameStateContents.gameStatus = GameStatus.REFUELLING;
        gameStateContents.altitude = minAltitude;
        gameStateContents.floorAltitude = minAltitude;
        gameStateContents.fuel = maxFuel * startFuelQuotient;
        gameStateContents.bombs = maxBombs;
        gameStateContents.score = 0;
        gameStateContents.wind = false;
        gameStateContents.damages = new bool[] { false, false, false, false };
        gameStateContents.targetsHit = 0;
        gameStateContents.targetsHitMin = 0;
        gameStateContents.latestLevelPrereq = null;
        gameStateContents.enemyHQs = null;
        gameStateContents.enemyPlaneAltitudes = new();
        gameStateContents.bossDefeated = false;
        gameStateContents.boss = null;
        gameStateContents.restartCoolDownSeconds = 0f;
    }

    public bool AnyEnemyPlaneAtCollisionAltitude()
    {
        return gameStateContents.enemyPlaneAltitudes.Values.Any(alt => 
            Math.Abs(alt - gameStateContents.altitude) < gameStateContents.maxAltitudeDiffForPlaneCollision);
    }

    public bool AnyEnemyPlanes()
    {
        return gameStateContents.enemyPlaneAltitudes.Any();
    }

    public void UpdateRestartTimer(float deltaTime)
    {
        if (gameStateContents.restartCoolDownSeconds > 0f)
        {
            gameStateContents.restartCoolDownSeconds -= deltaTime;
            if (gameStateContents.restartCoolDownSeconds <= 0f)
            {
                gameStateContents.restartCoolDownSeconds = 0f;
                ReportEvent(GameEvent.RESTART_TIMER_EXPIRED);
            }
        }
    }

    public bool IsRestartAllowed()
    {
        return (gameStateContents.gameStatus == GameStatus.DEAD ||
                gameStateContents.gameStatus == GameStatus.FINISHED) &&
                gameStateContents.restartCoolDownSeconds <= 0f;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }
}
