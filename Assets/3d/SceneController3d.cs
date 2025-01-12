using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;



public class SceneController3d : MonoBehaviour, IGameStateObserver
{
    public PlayerPlane maxPlanePrefab;
    public EnemyPlane enemyPlanePrefab;
    public GameObject visibleAreaMarkerPrefab;
    public GameObject balloonParentPrefab;
    public GameObject bombSplashPrefab;
    public GameObject bombCraterPrefab;
    public GameObject mushroomCloudPrefab;
    public GameObject refobject;
    public float width = 1;
    public float height = 1;
    public float riverSectionHeight = 20f;
    public float maxSegmentHeight = 3.0f;
    public float minSegmentHeight = 0.5f;
    public float minDistanceRiverAirstrip = 5.0f;
    public float maxDistanceRiverToAdjust = 2.0f;
    public float roadHeight = 0.4f;
    public static readonly float[] riverSlopes = new float[] {0.5f, 0.5f, 1.0f, 2.0f, 2.0f};
    public static readonly int neutralRiverSlopeIndex = 2;
    float levelWidth;
    public float levelLength = 80f;
    public float activationDistance = 8f;
    public float deactivationDistance = 8f;
    public float minRestartWaitSeconds = 1.0f;
    public float fuelRateLow = 0.6f;
    public float fuelRateHigh = 0.9f;
    public float refuelRate = 4.1f;
    public float fuelFullTankMargin = 0.01f;
    public float fuelRefillMargin = 0.05f;
    public float bombLoadTimeSec = 0.08f;
    public float repairTimeSec = 0.8f;
    public float enemyPlaneSpeedMax = 2.5f;
    public float enemyPlaneSpeedMin = 1.5f;
    public float enemyPlaneOncomingSpeedMin = -1.5f;
    public float enemyPlaneOncomingSpeedMax = -0.5f;
    public float enemyPlaneIntervalSecMax = 15f;
    public float enemyPlaneIntervalSecMin = 5f;
    public float windIntervalSecMax = 10f;
    public float windIntervalSecMin = 5f;
    public float carOffsetX = -5f;
    public float vipProbability = 0.5f;
    public float carProbability = 0.5f;
    public float enemyPlaneOncomingProbability = 0.3f;
    public float windProbability = 0.6f;
    public float riverBankWidth = 0.1f;
    public float parllelRoadSideWidth = 0.1f;
    public float parallelRoadWidth = 0.9f;
    public bool asyncLevelBuild = false;
    public int leftTrim = 2;
    public int rightTrim = 5;
    public float visibleAreaMarkerWidth = 4f;
    public float visibleAreaMarkerHeight = 3f;
    public LevelType startLevelType = LevelType.NORMAL;

    //// Game status
    MaxCamera maxCamera;
    int level = -1;
    float prepTimeForNextLevelQuotient = 0.90f;
    float lastLevelStartZ = 0f;
    int currentLevelIndex = 0;
    static int nofLevels = 2;
    GameObject[] levels;
    LevelContents latestLevel;
    Task<LevelContents> newLevelTask;
    int framesToBuildLevelDbg;
    PlayerPlane maxPlane;
    float landingStripStartZ;
    float landingStripEndZ;
    float landingStripWidth;
    public SceneBuilder sceneBuilder;
    GameState gameState;
    List<GameObjectCollection> pendingActivation = new List<GameObjectCollection>();
    List<GameObjectCollection> activeObjects = new List<GameObjectCollection>();
    float restartCoolDownSeconds = 0f;
    float bombLoadCooldownSec = 0f;
    float repairCooldownSec = 0f;
    float enemyPlaneCooldown = 0f;
    float windCooldown = 0f;
    GameObject riverSectionGameObject;
    List<Vector2> riverVerts;
    List<float> roadNearEdgesZ;
    TvSimDocument tvSimDocumentObject;
    GameObject balloonParent;
    ////
    
    GameObject GetLevel() => levels[currentLevelIndex];

    void RotateLevels()
    {
        currentLevelIndex = (currentLevelIndex + 1) % nofLevels; 
        var oldLevel = levels[currentLevelIndex];
        if (oldLevel != null)
        {
            Destroy(oldLevel);
        }
        
        level++;
        var llcz = level * levelLength;
        var newLevel = Instantiate(new GameObject(), new Vector3(0f, 0f, llcz), Quaternion.identity);
        levels[currentLevelIndex] = newLevel;
        lastLevelStartZ = llcz;
        balloonParent = Instantiate(balloonParentPrefab, newLevel.transform);
        InterfaceHelper.GetInterface<BalloonManager>(balloonParent).SetRefTransform(refobject.transform);
    }

    

    void CreateLevel()
    {
        RotateLevels();
        var newGameObjects = sceneBuilder.PopulateScene(latestLevel)
        .Select(goc => new GameObjectCollection {zCoord = goc.zCoord + lastLevelStartZ, gameObjects = goc.gameObjects})
        .ToList();
        pendingActivation.AddRange(newGameObjects);
    }

    int GetTargetHitsAtStartOfLevel(LevelPrerequisite levelPrereq)
    {
        switch (levelPrereq.levelType)
        {
            case LevelType.NORMAL:
            case LevelType.ROAD:
            case LevelType.BALLOONS:
                return 0;
            case LevelType.CITY:
                return levelPrereq.enemyHQsBombed.Where(hq => hq).Count();
            default:
                Debug.LogError($"invalid level type {levelPrereq.levelType}");
                return 0;
        }
    }

    int GetTargetHitsMin(LevelPrerequisite levelPrereq)
    {
        switch (levelPrereq.levelType)
        {
            case LevelType.NORMAL:
                return gameState.targetsHitMin1;
            case LevelType.ROAD:
                return gameState.targetsHitMin2;
            case LevelType.CITY:
                return levelPrereq.enemyHQsBombed.Count();
            case LevelType.BALLOONS:
                return 99;
            default:
                Debug.LogError($"invalid level type {levelPrereq.levelType}");
                return 0;
        }
    }

    void StartNewGame()
    {
        levelWidth = (levelLength * LevelContents.gridWidth) / LevelContents.gridHeight;
        level = -1;
        //var levelLowerLeftCornerX = 0f;
        var refObjStartOffset = 0.8f;
        //var newRefObjPos = new Vector3(levelLowerLeftCornerX + levelWidth / 2 + refObjStartOffset, refObjStartOffset, 0f);
        var newRefObjPos = new Vector3(0f, 0f, refObjStartOffset);
        refobject.transform.position = newRefObjPos;

        if (maxPlane == null)
        {
            maxPlane = Instantiate(maxPlanePrefab, refobject.transform);
            maxPlane.refObject = refobject.transform;
        }
        maxPlane.transform.localPosition = Vector3.zero;

        if (levels != null)
        {
            foreach (var level in levels)
            {
                if (level != null)
                {
                    Destroy(level);
                }
            }
        }
        levels = new GameObject[nofLevels];

        pendingActivation.Clear();
        activeObjects.Clear();
        roadNearEdgesZ = new();
        newLevelTask = null;
        gameState = GetGameState();
        var stateContents = gameState.GetStateContents();
        gameState.Reset();
        stateContents.latestLevelPrereq = new LevelPrerequisite 
            {
                levelType = startLevelType,
                riverLeftOfAirstrip=true,
                enemyHQsBombed = new List<bool> {false, false, false}
            };
        latestLevel = new LevelBuilder().Build(stateContents.latestLevelPrereq);
        CreateLevel();
        PreventRelanding();
        stateContents.enemyHQs = null;
        stateContents.targetsHitMin = GetTargetHitsMin(stateContents.latestLevelPrereq);
        gameState.ReportEvent(GameEvent.START);
    }

    void Start()
    {   
        var camObject = GameObject.Find("Main Camera");
        tvSimDocumentObject = FindAnyObjectByType<TvSimDocument>(FindObjectsInactive.Include);
        maxCamera = InterfaceHelper.GetInterface<MaxCamera>(camObject);

        Settings.Update();

        StartNewGame();
    }

    bool IsOverRoad(Vector3 position)
    {
        foreach (var nearEdgeZ in roadNearEdgesZ)
        {
            if (position.z < (nearEdgeZ + roadHeight))
            {
                return position.z > nearEdgeZ;
            }
        }
        return false;
    }

    bool IsOverLandingStrip(Vector3 position)
    {
        return position.z > landingStripStartZ && 
            position.z < landingStripEndZ &&            
            Math.Abs((refobject.transform.position.x - position.x)) < landingStripWidth / 2;
    }

    // Todo: remove param xOffset (?)
    float GetRiverLeftEdgeX(float yCoord, float xOffset, float yOffset)
    {
        // find segment
        var segmentIndex = 0;
        var maxSegmentIndex = (int)Math.Floor ((double)(riverVerts.Count - 1) / 4);
        while (segmentIndex <= maxSegmentIndex)
        {
            if ((riverVerts[segmentIndex * 4].y + yOffset) < yCoord &&
                (riverVerts[segmentIndex * 4 + 2].y + yOffset) >= yCoord)
            {
                break;
            }
            segmentIndex++;
        }

        if (segmentIndex > maxSegmentIndex)
        {
            Debug.LogWarning("Could not find river edge coordinates");
            return 0f;
        }

        // interpolate river edges x
        var ydiff = yCoord - (riverVerts[segmentIndex*4].y + yOffset);
        var xdiff = ydiff * ((riverVerts[segmentIndex*4 + 2].x + xOffset) - (riverVerts[segmentIndex*4].x + xOffset)) / ((riverVerts[segmentIndex*4 + 2].y + yOffset) - (riverVerts[segmentIndex*4].y + yOffset));

        return riverVerts[segmentIndex*4].x + xOffset + xdiff;
    }


    bool IsOverRiver(Vector3 position)
    {
        var xOffset = riverSectionGameObject.transform.position.x;
        var yOffset = riverSectionGameObject.transform.position.z;

        var leftEdgeX = GetRiverLeftEdgeX(position.z, xOffset, yOffset);
        var rightEdgeX = leftEdgeX + LevelBuilder.riverWidth * (levelWidth / LevelContents.gridWidth);

        // compare to position x
        var overRiverSegment =
            position.x > leftEdgeX &&
            position.x < rightEdgeX;
        return overRiverSegment;
    }

    void PreventRelanding()
    {
        landingStripEndZ = landingStripStartZ;
    }

    GameState GetGameState() 
    {
        if (gameState == null)
        {
            gameState = FindAnyObjectByType<GameState>();
            gameState.RegisterObserver(this);
        }
        return gameState;
    }

    void SetEnemyPlaneCooldown()
    {
        enemyPlaneCooldown = UnityEngine.Random.Range(enemyPlaneIntervalSecMin, enemyPlaneIntervalSecMax);
    }

    void SetWindCooldown()
    {
        windCooldown = UnityEngine.Random.Range(windIntervalSecMin, windIntervalSecMax);
    }

    void SpawnEnemyPlane()
    {
        Debug.Log("Time to spawn enemy plane");
        return; //TEMP
        var startPos = refobject.transform.position;
        //startPos = transform.position;
        bool oncoming = UnityEngine.Random.Range(0f, 1.0f) < enemyPlaneOncomingProbability;
        if (oncoming)
        {
            startPos.x += UnityEngine.Random.Range(-gameState.maxHorizPosition, gameState.maxHorizPosition) + 2.0f * gameState.maxAltitude;
            startPos.y += 2.0f * gameState.maxAltitude;
        }
        else
        {
            startPos.x += UnityEngine.Random.Range(-gameState.maxHorizPosition - 2 * gameState.maxAltitude, gameState.maxHorizPosition - 2 * gameState.maxAltitude);
            startPos.y += -gameState.maxAltitude;
        }

        
        startPos.z = UnityEngine.Random.Range(gameState.minSafeAltitude, gameState.maxAltitude);
        EnemyPlane enemyPlane = Instantiate(enemyPlanePrefab, startPos, Quaternion.identity);
        enemyPlane.refObject = refobject.transform;
        var minSpeed = oncoming ? enemyPlaneOncomingSpeedMin : enemyPlaneSpeedMin * gameState.maxSpeed;
        var maxSpeed = oncoming ? enemyPlaneOncomingSpeedMax : enemyPlaneSpeedMax * gameState.maxSpeed;

        enemyPlane.SetSpeed(UnityEngine.Random.Range(minSpeed, maxSpeed));
        if (UnityEngine.Random.Range(0f, 1.0f) < vipProbability && 
            LevelBuilder.PossibleVipTargets(gameState.GetStateContents().latestLevelPrereq.levelType))
        {
            enemyPlane.SetVip();
        }
    }

    LevelPrerequisite GetNewLevelPrereq()
    {
        var latestLevelType = gameState.GetStateContents().latestLevelPrereq.levelType;
        var newLevelType = latestLevelType;
        var reachedTargetLimit = gameState.GetTargetsHit() >= gameState.GetStateContents().targetsHitMin;
        if (latestLevelType == LevelType.CITY)
        {
            newLevelType = LevelType.CITY;
        }
        else if (reachedTargetLimit)
        {
            if (latestLevelType == LevelType.NORMAL)
            {
                newLevelType = LevelType.ROAD;
            }
            else // if (latestLevelType == LevelType.ROAD) 
            {
                newLevelType = LevelType.CITY;
            }
        }
        else if (latestLevelType == LevelType.ROAD)
        {
            newLevelType = LevelType.NORMAL;
        }

        var enemyHQsBombed = latestLevelType == LevelType.CITY ?
            gameState.GetStateContents().enemyHQs.Select(hq => hq.IsBombed()) :
            new List<bool> {false, false, false};
        return new LevelPrerequisite {
            levelType = newLevelType,
            riverLeftOfAirstrip=latestLevel.riverEndsLeftOfAirstrip,
            enemyHQsBombed = enemyHQsBombed
        };
    }

    // Update is called once per frame
    void Update()
    {
        gameState = GetGameState();
        GameStateContents stateContents = gameState.GetStateContents();

        if (refobject.transform.position.z > (lastLevelStartZ + levelLength * prepTimeForNextLevelQuotient))
        {
            if (!asyncLevelBuild)
            {
                Debug.Log("Time to build new level (sync) ***************");
                stateContents.latestLevelPrereq = GetNewLevelPrereq();
                latestLevel = new LevelBuilder().Build(stateContents.latestLevelPrereq);
                CreateLevel();
                gameState.SetTargetsHit(
                    GetTargetHitsAtStartOfLevel(stateContents.latestLevelPrereq),
                    GetTargetHitsMin(stateContents.latestLevelPrereq));
            }
            else 
            {
                if (newLevelTask == null)
                {
                    Debug.Log("Time to build new level asynchronously ***************");
                    stateContents.latestLevelPrereq = GetNewLevelPrereq();
                    gameState.SetTargetsHit(
                        GetTargetHitsAtStartOfLevel(stateContents.latestLevelPrereq),
                        GetTargetHitsMin(stateContents.latestLevelPrereq));
                    newLevelTask = new LevelBuilder().BuildAsync(stateContents.latestLevelPrereq);
                    framesToBuildLevelDbg = 0;
                }
                else 
                {
                    ++framesToBuildLevelDbg;
                    if (newLevelTask.IsCompleted)
                    {
                        Debug.Log($"New level built in {framesToBuildLevelDbg} frames ***************");
                        latestLevel = newLevelTask.Result;
                        CreateLevel();
                        newLevelTask = null;
                    }
                }
            }
        }

        while (pendingActivation.Count > 0 && refobject.transform.position.z + activationDistance > pendingActivation.First().zCoord)
        {
            //Debug.Log($"Time to activate more game objects at {refobject.transform.position.z} {pendingActivation.First().zCoord}");
            var activeCollection = pendingActivation.First();
            // Instantiate game objects, never mind return value
            activeCollection.gameObjects = activeCollection.gameObjects.ToArray();
            pendingActivation.RemoveAt(0);
            activeObjects.Add(activeCollection);
            break;
        }

        while (activeObjects.Count > 0 && refobject.transform.position.z - deactivationDistance > activeObjects.First().zCoord)
        {
            //Debug.Log($"Time to destroy game objects at {refobject.transform.position.z} {activeObjects.First().zCoord}");

            var collection = activeObjects.First();
            foreach (var gameObject in collection.gameObjects)
            {
                Destroy(gameObject);
            }

            activeObjects.RemoveAt(0);
        }

        while (roadNearEdgesZ.Count > 0 && refobject.transform.position.z - deactivationDistance > roadNearEdgesZ.First())
        {
            roadNearEdgesZ.RemoveAt(0);
        } 

        var distanceDiff = refobject.transform.position.z - lastLevelStartZ;
        gameState.SetApproachingLanding(
            (distanceDiff > levelLength * (1-LevelBuilder.finalApproachQuotient)) ||
            distanceDiff < 0);

        // Update game state
        if (stateContents.gameStatus == GameStatus.KILLED_BY_FLACK ||
            stateContents.gameStatus == GameStatus.COLLIDED)
        {
            if (maxPlane.GetAltitude() <= gameState.minAltitude)
            {
                gameState.SetStatus(GameStatus.DEAD);
            }
        }

        if (stateContents.gameStatus == GameStatus.FLYING ||
            stateContents.gameStatus == GameStatus.ACCELERATING ||
            stateContents.gameStatus == GameStatus.OUT_OF_FUEL)
        {
            if (stateContents.gameStatus == GameStatus.FLYING ||
                stateContents.gameStatus == GameStatus.OUT_OF_FUEL)
            {
                //Debug.Log($"Alt: {maxPlane.GetAltitude()} ({MaxControl.landingAltitude})");
                if (maxPlane.GetAltitude() <= MaxControl.landingAltitude)
                {
                    //Debug.Log($"Low {maxPlane.GetPosition()}");
                    if (IsOverLandingStrip(maxPlane.GetPosition()))
                    {
                        Debug.Log(">>>>>>>>> Landing <<<<<<<<<");
                        PreventRelanding();
                        gameState.SetStatus(GameStatus.DECELERATING);
                    }
                }

                enemyPlaneCooldown -= Time.deltaTime;
                if (enemyPlaneCooldown <= 0)
                {
                    SpawnEnemyPlane();
                    SetEnemyPlaneCooldown();
                }

                windCooldown -= Time.deltaTime;
                if (windCooldown <= 0)
                {
                    stateContents.windDirection = GameStateContents.windDirections[UnityEngine.Random.Range(0, GameStateContents.windDirections.Length)];
                    gameState.SetWind(UnityEngine.Random.Range(0f, 1f) < windProbability);
                    SetWindCooldown();
                    //Debug.Log($"New wind {stateContents.windDirection} {stateContents.wind}");
                }
            }

            if (stateContents.speed < gameState.maxSpeed)
            {
                var newSpeed = stateContents.speed + gameState.acceleration * gameState.maxSpeed * Time.deltaTime;
                if (newSpeed > gameState.maxSpeed)
                {
                    newSpeed = gameState.maxSpeed;
                }
                if (newSpeed >= gameState.GetSafeTakeoffSpeed())
                {
                    gameState.SetStatus(GameStatus.FLYING);
                }
                gameState.SetSpeed(newSpeed);
            }
        }
        else if (stateContents.gameStatus == GameStatus.DECELERATING)
        {
            var newSpeed = stateContents.speed - gameState.acceleration * gameState.maxSpeed * Time.deltaTime;
            if (newSpeed < 0f)
            {
                newSpeed = 0f;
                var enemyHQs = gameState.GetStateContents().enemyHQs;
                gameState.SetStatus(enemyHQs != null && enemyHQs.Count > 0 && enemyHQs.All(hq => hq.IsBombed()) ?
                    GameStatus.FINISHED : GameStatus.REFUELLING);
            }
            gameState.SetSpeed(newSpeed);
        }
        else if (stateContents.gameStatus == GameStatus.REFUELLING)
        {
            
            if (stateContents.fuel < (gameState.maxFuel - fuelFullTankMargin))
            {
                gameState.SetFuel(Math.Min(stateContents.fuel + refuelRate * Time.deltaTime, gameState.maxFuel));
            }
            else
            {
                bombLoadCooldownSec = bombLoadTimeSec;
                gameState.SetStatus(GameStatus.LOADING_BOMBS);
            }
        }
        else if (stateContents.gameStatus == GameStatus.LOADING_BOMBS)
        {
            if (stateContents.bombs < gameState.maxBombs)
            {
                if (bombLoadCooldownSec <= 0)
                {
                    gameState.IncrementBombs(1);
                    bombLoadCooldownSec = bombLoadTimeSec;    
                }
                bombLoadCooldownSec -= Time.deltaTime;
            }
            else
            {
                repairCooldownSec = repairTimeSec;
                gameState.SetStatus(GameStatus.REPAIRING);
            }
        }
        else if (stateContents.gameStatus == GameStatus.REPAIRING)
        {
            if (stateContents.damages.Any(d => d))
            {
                if (repairCooldownSec <= 0)
                {
                    gameState.SetRandomDamage(false);
                    repairCooldownSec = repairTimeSec;
                }
                repairCooldownSec -= Time.deltaTime;
            }
            else 
            {
                if (stateContents.fuel < (gameState.maxFuel - fuelRefillMargin))
                {
                    gameState.SetStatus(GameStatus.REFUELLING);
                }
                else
                {
                    gameState.SetStatus(GameStatus.ACCELERATING);
                }
            }
        }
        else if (stateContents.gameStatus == GameStatus.DEAD || 
                 stateContents.gameStatus == GameStatus.FINISHED)
        {
            if (restartCoolDownSeconds > 0f)
            {
                restartCoolDownSeconds -= Time.deltaTime;
            }
        }

        if (!(stateContents.gameStatus == GameStatus.FINISHED ||
              stateContents.gameStatus == GameStatus.DEAD ||
              stateContents.gameStatus == GameStatus.KILLED_BY_FLACK ||
              stateContents.gameStatus == GameStatus.DECELERATING ||
              stateContents.gameStatus == GameStatus.REFUELLING))
        {
            var fuelRate = gameState.GotDamage(DamageIndex.F) ? fuelRateHigh : fuelRateLow;
            gameState.SetFuel(Math.Max(stateContents.fuel - fuelRate * Time.deltaTime, 0f));
            if (stateContents.fuel <= 0f)
            {
                gameState.SetStatus(GameStatus.OUT_OF_FUEL);
            }
        }

        // Update refobject position
        Vector3 levelVelocity = new(0, 0, stateContents.speed);
        Vector3 delta = levelVelocity * Time.deltaTime;
        refobject.transform.position += delta;
    }

    public void OnGameStatusChanged(GameStatus gameStatus)
    {
        Debug.Log($"New State: {gameStatus}");
        if(gameStatus == GameStatus.DEAD ||
           gameStatus == GameStatus.FINISHED)
        {
            gameState.SetSpeed(0f);
            restartCoolDownSeconds = minRestartWaitSeconds;
        }
    }

    public void OnGameEvent(GameEvent gameEvent)
    {
        if (gameEvent == GameEvent.RESTART_REQUESTED)
        {
            if (restartCoolDownSeconds > 0f)
            {
                //Debug.Log("Too early to restart");
                return;
            }

            Debug.Log("Starting a new game");

            StartNewGame();            
        }
        else if (gameEvent == GameEvent.START)
        {
            gameState.SetSpeed(0f);
            gameState.SetStatus(GameStatus.REFUELLING);
        }
        else if (gameEvent == GameEvent.BIG_DETONATION && maxCamera != null)
        {
            maxCamera.OnDetonation();
        }
        else if (gameEvent == GameEvent.VIEW_MODE_CHANGED && maxCamera != null)
        {
            maxCamera.OnViewModeChanged();
            tvSimDocumentObject.OnViewModeChanged();
        }
    }

    public void OnBombLanded(GameObject bomb, GameObject hitObject) 
    {
        if (hitObject == null)
        {
            var prefab = IsOverRiver(bomb.transform.position) ? bombSplashPrefab : bombCraterPrefab;
            if (IsOverRoad(bomb.transform.position))
            {
                prefab = mushroomCloudPrefab;
                //todo: report road or bridge hit for scoring
            }
            Vector3 craterPosition = bomb.transform.position;
            craterPosition.z = -0.25f;
            Instantiate(prefab, craterPosition, Quaternion.identity, GetLevel().transform);
            if (prefab != bombSplashPrefab)
            {
                gameState.ReportEvent(GameEvent.SMALL_DETONATION);
                gameState.ReportEvent(GameEvent.SMALL_BANG);
            }
        }
        else
        {
            if (!IsOverRiver(hitObject.transform.position) || IsOverRoad(hitObject.transform.position))
            {
                Instantiate(mushroomCloudPrefab, hitObject.transform.position, Quaternion.identity, GetLevel().transform);
                gameState.ReportEvent(GameEvent.SMALL_DETONATION);
                gameState.ReportEvent(GameEvent.MEDIUM_BANG);
            }
            Destroy(hitObject);
        }
    
        if (bomb != null)
        {
            Destroy(bomb.gameObject);
        }
    }

    public void OnEnemyPlaneStatusChanged(EnemyPlane enemyPlane, bool active) {}
}
