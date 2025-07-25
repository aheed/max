using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class SceneController3d : MonoBehaviour
{
    public PlayerPlane maxPlanePrefab;
    public EnemyPlane3d enemyPlanePrefab;
    public GameObject visibleAreaMarkerPrefab;
    public GameObject balloonParentPrefab;
    public GameObject bombSplashPrefab;
    public GameObject bombCraterPrefab;
    public GameObject mushroomCloudPrefab;
    public BossShadowCaster bossShadowCasterPrefab;
    public GameObject refobject;
    public Material targetMaterial;
    public Material planeTargetMaterial;
    public Material carTargetMaterial;
    public Material boatTargetMaterial;
    public Material daySkyboxMaterial;
    public Material nightSkyboxMaterial;
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
    public float cellLength = 80f / LevelContents.fullGridHeight;
    public float activationDistance = 8f;
    public float deactivationDistance = 8f;
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
    public float vipProbability = 0.5f;    
    public float enemyPlaneOncomingProbability = 0.3f;
    public float windProbability = 0.6f;
    public float riverBankWidth = 0.1f;
    public bool asyncLevelBuild = false;
    public float visibleAreaMarkerWidth = 4f;
    public float visibleAreaMarkerHeight = 3f;
    public float dayLightIntensity = 1.1f;
    public float nightLightIntensity = 0.5f;
    public Color nightLightColor = new Color(0.5f, 0.3f, 0.95f, 1f);
    public Color dayLightColor = new Color(1f, 1f, 1f, 1f);
    public Color nightAmbientColor = new Color(0.1f, 0.1f, 0.1f, 1f);
    public Color dayAmbientColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    public LevelType startLevelType = LevelType.NORMAL;
    TargetMaterialBlinker targetBlinker;    

    //// Game status
    float prepTimeForNextLevelLength = 20f;
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
    List<GameObjectCollection4> pendingActivation = new();
    List<GameObjectCollection4> activeObjects = new();    
    float bombLoadCooldownSec = 0f;
    float repairCooldownSec = 0f;
    float enemyPlaneCooldown = 0f;
    float windCooldown = 0f;
    GameObject riverSectionGameObject;
    List<Vector3> riverVerts;
    List<float> roadNearEdgesZ;
    List<SceneRiverSegment> riverSegments;
    GameObject balloonParent;
    float debugAcceleration = 0.4f;
    List<Camera> cameras = new();
    int cameraIndex = 0;
    ///    

    GameObject GetLevel() => levels[currentLevelIndex];

    void RotateLevels()
    {
        currentLevelIndex = (currentLevelIndex + 1) % nofLevels; 
        var oldLevel = levels[currentLevelIndex];
        if (oldLevel != null)
        {
            Destroy(oldLevel);
        }
        
        var newLevel = Instantiate(new GameObject("level"), new Vector3(0f, 0f, lastLevelStartZ), Quaternion.identity);
        levels[currentLevelIndex] = newLevel;        
        balloonParent = Instantiate(balloonParentPrefab, newLevel.transform);
    }

    void CreateLevel()
    {
        RotateLevels();
        var sceneInput = new SceneInput
        {
            levelTransform = GetLevel().transform,
            levelWidth = levelWidth,
            levelHeight = cellLength * latestLevel.gridHeight,
            vipProbability = vipProbability,
            balloonParentTransform = balloonParent.transform,
            roadHeight = roadHeight,
            referenceObjectTransform = refobject.transform,
            playerPlaneObject = maxPlane.gameObject
        };
        var sceneOutput = sceneBuilder.PopulateScene(gameState.GetStateContents().latestLevelPrereq, latestLevel, sceneInput);
        var newGameObjects = sceneOutput.gameObjects
        .Select(goc => new GameObjectCollection4 { zCoord = goc.zCoord + lastLevelStartZ, objectRefs = goc.objectRefs })
        .ToList();
        pendingActivation.AddRange(newGameObjects);
        gameState.GetStateContents().enemyHQs = sceneOutput.enemyHQs;
        if (sceneOutput.boss != null)
        {
            gameState.GetStateContents().boss = sceneOutput.boss;
            gameState.GetStateContents().bossDefeated = false;
        }
        landingStripStartZ = sceneOutput.landingStripStartZ;
        landingStripEndZ = sceneOutput.landingStripEndZ;
        landingStripWidth = sceneOutput.landingStripWidth;
        riverVerts = sceneOutput.riverVerts;
        roadNearEdgesZ.AddRange(sceneOutput.roadNearEdgesZ);
        riverSegments.AddRange(sceneOutput.riverSegments);
        var mainLight = GetMainLight();
        if (GameState.GetInstance().IsNightTime())
        {
            mainLight.intensity = nightLightIntensity;
            mainLight.color = nightLightColor;
            RenderSettings.ambientLight = nightAmbientColor;
            RenderSettings.skybox = nightSkyboxMaterial;
        }
        else
        {
            mainLight.intensity = dayLightIntensity;
            mainLight.color = dayLightColor;
            RenderSettings.ambientLight = dayAmbientColor;
            RenderSettings.skybox = daySkyboxMaterial;
        }
        maxPlane.SetAltitudeLights(LevelHelper.AltitudeLights(gameState.GetStateContents().latestLevelPrereq.levelType));
        maxPlane.SetArmaments(LevelHelper.GetArmamentType(gameState.GetStateContents().latestLevelPrereq.levelType));
        
        Debug.Log($"AmbientIntensity={RenderSettings.ambientIntensity} color={RenderSettings.ambientLight} " +
            $"MainLightIntensity={mainLight.intensity} color={mainLight.color}");
        gameState.ReportEvent(GameEvent.VIEW_MODE_CHANGED);
    }

    int GetTargetHitsAtStartOfLevel(LevelPrerequisite levelPrereq)
    {
        switch (levelPrereq.levelType)
        {
            case LevelType.NORMAL:
            case LevelType.ROAD:
            case LevelType.BALLOONS:
            case LevelType.DAM:
                if (AllEnemyHQsBombed())
                {
                    return gameState.GetStateContents().targetsHit;
                }
                return 0;
            case LevelType.CITY:
                return levelPrereq.enemyHQsBombed.Where(hq => hq).Count();
            case LevelType.ROBOT_BOSS:
            case LevelType.RED_BARON_BOSS:
            case LevelType.INTRO:
                return GameState.GetInstance().GetTargetsHit();
            default:
                Debug.LogError($"invalid level type {levelPrereq.levelType}");
                return 0;
        }
    }

    void StartNewGame()
    {
        LevelType firstLevelType = LevelSelection.startLevelOverride ?
            LevelSelection.startLevel : startLevelType;
        levelWidth = cellLength * LevelContents.gridWidth;
        lastLevelStartZ = 0f;
        //var levelLowerLeftCornerX = 0f;
        var refObjStartOffset = 0.8f;
        //var newRefObjPos = new Vector3(levelLowerLeftCornerX + levelWidth / 2 + refObjStartOffset, refObjStartOffset, 0f);
        var newRefObjPos = new Vector3(levelWidth / 2, 0f, refObjStartOffset);
        refobject.transform.position = newRefObjPos;
        gameState = GameState.GetInstance();

        if (maxPlane == null)
        {
            maxPlane = Instantiate(maxPlanePrefab, refobject.transform);
            maxPlane.refObject = refobject.transform;
            gameState.SetPlaneHeights(maxPlane.GetHeight(), maxPlane.GetHeight());
            var cockpitCamera = maxPlane.GetComponentInChildren<Camera>();
            if (cockpitCamera != null)
            {
                cameras.Add(cockpitCamera);
                gameState.GetStateContents().cameraButtonVisible = true;
                gameState.ReportEvent(GameEvent.CAMERA_BUTTON_UPDATED);
            }
        }
        maxPlane.transform.localPosition = Vector3.zero;
        maxPlane.Reset();

        gameState.GetStateContents().homeButtonVisible = true;
        gameState.ReportEvent(GameEvent.HOME_BUTTON_UPDATED);
        gameState.GetStateContents().pauseButtonVisible = true;
        gameState.ReportEvent(GameEvent.PAUSE_BUTTON_UPDATED);
        gameState.GetStateContents().tvSimButtonVisible = true;
        gameState.ReportEvent(GameEvent.TV_SIM_BUTTON_UPDATED);

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
        riverSegments = new();
        newLevelTask = null;
        var stateContents = gameState.GetStateContents();
        if (stateContents.boss != null)
        {
            Destroy(stateContents.boss);
            stateContents.boss = null;
        }
        gameState.Reset();
        stateContents.latestLevelPrereq = new LevelPrerequisite
        {
            levelType = firstLevelType,
            riverLeftOfAirstrip = true,
            enemyHQsBombed = new List<bool> { false, false, false },
            boss = LevelHelper.GetBossType(firstLevelType) != BossType.NONE,
            missionComplete = false,
            firstLevel = true,
            enemyAircraft = LevelHelper.EnemyAircraft(firstLevelType),
            wind = LevelHelper.Wind(firstLevelType),
            nightTime = LevelHelper.NightTime(firstLevelType)
        };
        latestLevel = new LevelBuilder().Build(stateContents.latestLevelPrereq);
        sceneBuilder.Init();
        CreateLevel();
        PreventRelanding();
        stateContents.targetsHitMin = LevelHelper.GetTargetHitsMin(stateContents.latestLevelPrereq);
        var controlDocument = FindAnyObjectByType<ControlDocument>(FindObjectsInactive.Include);
        controlDocument.gameObject.SetActive(Globals.touchScreenDetected);
        gameState.ReportEvent(GameEvent.START);
        gameState.SetPause(false);
    }

    Light GetMainLight()
    {
        GameObject lightObject = GameObject.Find("Main Directional Light");
        if (lightObject == null)
        {
            return null;
        }

        return lightObject.GetComponent<Light>();
    }

    void Start()
    {
        //UserGuide.SetOpenState(!Settings.UserGuideHasBeenDisplayed());
        UserGuide.SetOpenState(false);
        Settings.Update();


        GameState.GetInstance().Subscribe(GameEvent.START, OnStartCallback);
        GameState.GetInstance().Subscribe(GameEvent.RESTART_REQUESTED, OnRestartRequestCallback);
        GameState.GetInstance().Subscribe(GameEvent.TARGET_HIT, OnTargetHitCallback);
        //GameState.GetInstance().Subscribe(GameEvent.DEBUG_ACTION1, OnDebugCallback1);
        //GameState.GetInstance().Subscribe(GameEvent.DEBUG_ACTION2, OnDebugCallback2);
        GameState.GetInstance().Subscribe(GameEvent.DEBUG_ACTION3, OnDebugCallback3);
        GameState.GetInstance().SubscribeToBombLandedEvent(OnBombLandedCallback);
        GameState.GetInstance().Subscribe(GameEvent.CAMERA_CHANGE_REQUESTED, CycleCameras);

        // Make copies of materials to avoid changing the .mat files
        GameState.carBlinkMaterial = new Material(carTargetMaterial);
        GameState.boatBlinkMaterial = new Material(boatTargetMaterial);
        GameState.planeBlinkMaterial = new Material(planeTargetMaterial);
        GameState.targetBlinkMaterial = new Material(targetMaterial);
        targetBlinker = new TargetMaterialBlinker(new[] {
            GameState.planeBlinkMaterial,
            GameState.carBlinkMaterial,
            GameState.boatBlinkMaterial,
            GameState.targetBlinkMaterial});
        var mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        cameras.Add(mainCamera);
        StartNewGame();
        debugAcceleration = gameState.acceleration;
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
    // Todo: use z coord, not y coord
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
        var segment = riverSegments.FirstOrDefault(s =>  position.z < s.maxZ);
        if (segment == null || segment.minZ >= position.z)
        {
            return false;
        }
        
        // interpolate river edges x
        var zdiff = position.z - segment.minZ;
        var xdiff = zdiff * (segment.ulcX - segment.llcX) / (segment.maxZ - segment.minZ);

        return position.x > segment.llcX + xdiff && position.x < segment.lrcX + xdiff;
    }

    void PreventRelanding()
    {
        landingStripEndZ = landingStripStartZ;
    }

    void SetEnemyPlaneCooldown()
    {
        enemyPlaneCooldown = UnityEngine.Random.Range(enemyPlaneIntervalSecMin, enemyPlaneIntervalSecMax);
    }

    void SetWindCooldown()
    {
        windCooldown = UnityEngine.Random.Range(windIntervalSecMin, windIntervalSecMax);
    }

    void SpawnBossShadow(BossShadowVariant variant)
    {
        Debug.Log("Time to spawn boss shadow");
        BossShadowCaster bossShadowCaster = Instantiate(bossShadowCasterPrefab);
        bossShadowCaster.Init(refobject, variant);
    }

    void SpawnEnemyPlane()
    {   
        if (!GameState.GetInstance().GetStateContents().latestLevelPrereq.enemyAircraft)
        {
            return;
        }

        var startPos = refobject.transform.position;

        bool oncoming = UnityEngine.Random.Range(0f, 1.0f) < enemyPlaneOncomingProbability;
        startPos.x += UnityEngine.Random.Range(-gameState.maxHorizPosition / 3, gameState.maxHorizPosition / 3);
        startPos.z += oncoming ? activationDistance : -deactivationDistance;
        startPos.y = UnityEngine.Random.Range(gameState.minSafeAltitude, gameState.maxAltitude * 0.8f);
        
        EnemyPlane3d enemyPlane = Instantiate(enemyPlanePrefab, startPos, Quaternion.identity);
        enemyPlane.refObject = refobject.transform;
        var minSpeed = oncoming ? enemyPlaneOncomingSpeedMin : enemyPlaneSpeedMin * gameState.maxSpeed;
        var maxSpeed = oncoming ? enemyPlaneOncomingSpeedMax : enemyPlaneSpeedMax * gameState.maxSpeed;

        enemyPlane.SetSpeed(UnityEngine.Random.Range(minSpeed, maxSpeed));
        if (UnityEngine.Random.Range(0f, 1.0f) < vipProbability && 
            LevelHelper.PossibleVipTargets(gameState.GetStateContents().latestLevelPrereq.levelType))
        {
            enemyPlane.SetVip();
        }

        Debug.Log("Time to spawn enemy plane. oncoming: " + oncoming + "vip:" + enemyPlane.IsVip());
    }

    bool ShallCreateNewBoss(LevelType newLevelType, LevelType oldLevelType)
    {
        return LevelHelper.GetBossType(newLevelType) != BossType.NONE &&
           oldLevelType != newLevelType;
    }

    bool IsMissionComplete(LevelType newLevelType, bool bossDefeated, bool reachedTargetLimit)
    {
        return (LevelHelper.GetBossType(newLevelType) != BossType.NONE && bossDefeated) ||
            reachedTargetLimit;
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
            else if (latestLevelType == LevelType.ROAD) 
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

        return new LevelPrerequisite
        {
            levelType = newLevelType,
            riverLeftOfAirstrip = latestLevel.riverEndsLeftOfAirstrip,
            enemyHQsBombed = enemyHQsBombed,
            boss = ShallCreateNewBoss(newLevelType, latestLevelType),
            missionComplete = IsMissionComplete(newLevelType, gameState.GetStateContents().bossDefeated, reachedTargetLimit),
            firstLevel = false,
            enemyAircraft = LevelHelper.EnemyAircraft(newLevelType),
            wind = LevelHelper.Wind(newLevelType),
            nightTime = LevelHelper.NightTime(newLevelType)
        };
    }

    bool AllEnemyHQsBombed()
    {
        var stateContents = gameState.GetStateContents();
        if (stateContents.latestLevelPrereq.levelType == LevelType.DAM)
        {
            // include dams in enemy HQs
            return stateContents.targetsHit >= stateContents.targetsHitMin;
        }

        var enemyHQs = gameState.GetStateContents().enemyHQs;
        return enemyHQs != null && enemyHQs.Count > 0 && enemyHQs.All(hq => hq.IsBombed());
    }

    // Update is called once per frame
    void Update()
    {
        gameState = GameState.GetInstance();
        GameStateContents stateContents = gameState.GetStateContents();

        if (refobject.transform.position.z > (lastLevelStartZ + latestLevel.gridHeight * cellLength - prepTimeForNextLevelLength))
        {
            if (!asyncLevelBuild)
            {
                Debug.Log("Time to build new level (sync) ***************");
                stateContents.latestLevelPrereq = GetNewLevelPrereq();
                lastLevelStartZ += latestLevel.gridHeight * cellLength;
                latestLevel = new LevelBuilder().Build(stateContents.latestLevelPrereq);
                gameState.SetApproachingLanding(latestLevel.landingStrip);
                CreateLevel();
                gameState.SetTargetsHit(
                    GetTargetHitsAtStartOfLevel(stateContents.latestLevelPrereq),
                    LevelHelper.GetTargetHitsMin(stateContents.latestLevelPrereq));
            }
            else 
            {
                if (newLevelTask == null)
                {
                    Debug.Log("Time to build new level asynchronously ***************");
                    stateContents.latestLevelPrereq = GetNewLevelPrereq();
                    gameState.SetTargetsHit(
                        GetTargetHitsAtStartOfLevel(stateContents.latestLevelPrereq),
                        LevelHelper.GetTargetHitsMin(stateContents.latestLevelPrereq));
                    newLevelTask = new LevelBuilder().BuildAsync(stateContents.latestLevelPrereq);
                    framesToBuildLevelDbg = 0;
                }
                else 
                {
                    ++framesToBuildLevelDbg;
                    if (newLevelTask.IsCompleted)
                    {
                        Debug.Log($"New level built in {framesToBuildLevelDbg} frames ***************");
                        lastLevelStartZ += latestLevel.gridHeight * cellLength;
                        latestLevel = newLevelTask.Result;
                        gameState.SetApproachingLanding(latestLevel.landingStrip);
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
            activeCollection.objectRefs = activeCollection.objectRefs.ToArray();
            pendingActivation.RemoveAt(0);
            activeObjects.Add(activeCollection);
            break;
        }

        while (activeObjects.Count > 0 && refobject.transform.position.z - deactivationDistance > activeObjects.First().zCoord)
        {
            //Debug.Log($"Time to release game objects at {refobject.transform.position.z} {activeObjects.First().zCoord}");

            var collection = activeObjects.First();
            foreach (var objRef in collection.objectRefs)
            {
                objRef.Release();
            }

            activeObjects.RemoveAt(0);
        }

        while (roadNearEdgesZ.Count > 0 && refobject.transform.position.z - deactivationDistance > roadNearEdgesZ.First())
        {
            roadNearEdgesZ.RemoveAt(0);
        }

        while (riverSegments.Count > 0 && refobject.transform.position.z - deactivationDistance > riverSegments.First().maxZ)
        {
            riverSegments.RemoveAt(0);
        } 

        var distanceDiff = refobject.transform.position.z - lastLevelStartZ;

        if (distanceDiff > 0f)
        {
            gameState.SetApproachingLanding(false);
        }

        // Update game state
        stateContents.floorAltitude = gameState.minAltitude +
             (IsOverRiver(maxPlane.transform.position) ? gameState.riverAltitude : 0f);

        if (stateContents.gameStatus == GameStatus.KILLED_BY_FLACK ||
            stateContents.gameStatus == GameStatus.COLLIDED)
        {
            if (maxPlane.GetAltitude() <= stateContents.floorAltitude)
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
                    if (IsOverLandingStrip(maxPlane.transform.position))
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
                    if (stateContents.latestLevelPrereq.wind)
                    {
                        stateContents.windDirection = GameStateContents.windDirections[UnityEngine.Random.Range(0, GameStateContents.windDirections.Length)];
                        gameState.SetWind(UnityEngine.Random.Range(0f, 1f) < windProbability);    
                    }
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
                var latestLevelType = gameState.GetStateContents().latestLevelPrereq.levelType;
                var bossDefeated = LevelHelper.GetBossType(latestLevelType) != BossType.NONE &&
                    gameState.GetStateContents().bossDefeated;
                gameState.SetStatus(bossDefeated || AllEnemyHQsBombed() ? GameStatus.FINISHED : GameStatus.REFUELLING);
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
            gameState.UpdateRestartTimer(Time.deltaTime);
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

        targetBlinker.Update(Time.deltaTime);

        // Update refobject position
        Vector3 levelVelocity = new(0, 0, stateContents.speed);
        Vector3 delta = levelVelocity * Time.deltaTime;
        refobject.transform.position += delta;
        gameState.playerPosition = maxPlane.gameObject.transform.position;
    }

    private void CycleCameras()
    {
        if (cameras.Count == 0)
        {
            return;
        }

        cameraIndex = (cameraIndex + 1) % cameras.Count;
        Debug.Log($"Switching to camera {cameraIndex} of {cameras.Count}");
        foreach (var camera in cameras)
        {
            camera.enabled = false;
        }
        cameras[cameraIndex].enabled = true;
        gameState.ReportEvent(GameEvent.CAMERA_CHANGED);
    }

    private void OnTargetHitCallback()
    {
        if (gameState.GetTargetsHit() != gameState.GetStateContents().targetsHitMin)
        {
            return;
        }

        switch (gameState.GetStateContents().latestLevelPrereq.levelType)
        {
            case LevelType.NORMAL:
                SpawnBossShadow(BossShadowVariant.BSH1);
                break;
            case LevelType.ROAD:
                SpawnBossShadow(BossShadowVariant.BSH2);
                break;
            case LevelType.CITY:
                SpawnBossShadow(BossShadowVariant.BSH3);
                break;
            case LevelType.BALLOONS:
            case LevelType.ROBOT_BOSS:
            case LevelType.RED_BARON_BOSS:
            case LevelType.INTRO:
            case LevelType.DAM:
                break;
            default:
                Debug.LogError($"Invalid level type {gameState.GetStateContents().latestLevelPrereq.levelType}");
                break;
        }
    }

    private void OnDebugCallback1()
    {
        SpawnBossShadow(BossShadowVariant.BSH1);
    }

    private void OnDebugCallback2()
    {
        SpawnBossShadow(BossShadowVariant.BSH2);
    }

    private void OnDebugCallback3()
    {
        CycleCameras();

        /*
        // Toggle speed 

        if (gameState.GetStateContents().speed == 0f)
        {
            gameState.SetSpeed(gameState.maxSpeed);
            gameState.acceleration = debugAcceleration;
        }
        else
        {
            gameState.SetSpeed(0f);
            gameState.acceleration = 0f;
        } */
    }

    private void OnStartCallback()
    {
        gameState.SetSpeed(0f);
        gameState.SetStatus(GameStatus.REFUELLING);
    }

    private void OnRestartRequestCallback()
    {
        if (!gameState.IsRestartAllowed())
        {
            Debug.Log("Too early to restart");
            return;
        }

        Debug.Log("Starting a new game");
        StartNewGame();
    }

    private void OnBombLandedCallback(BombLandedEventArgs args) =>
        OnBombLandedCallbackInternal(args.bomb, args.hitObject);
    private void OnBombLandedCallbackInternal(GameObject bomb, GameObject hitObject) 
    {
        if (hitObject == null)
        {
            var prefab = bombCraterPrefab;
            
            var craterAltitude = gameState.craterAltitude;

            if (IsOverRiver(bomb.transform.position))
            {
                if (bomb.transform.position.y > gameState.riverAltitude)
                {
                    // bomb has not landed yet
                    return;
                }
                prefab = bombSplashPrefab;
                craterAltitude += gameState.riverAltitude;
            }
            
            if (IsOverRoad(bomb.transform.position))
            {
                prefab = mushroomCloudPrefab;
                gameState.AddScore(10);
            }
            Vector3 craterPosition = bomb.transform.position;
            craterPosition.y = craterAltitude;
            Instantiate(prefab, craterPosition, Quaternion.identity, GetLevel().transform);
            if (prefab != bombSplashPrefab)
            {
                gameState.ReportEvent(GameEvent.SMALL_DETONATION);
                gameState.ReportEvent(GameEvent.SMALL_BANG);
            }
        }
        else
        {
            Instantiate(mushroomCloudPrefab, hitObject.transform.position, Quaternion.identity, GetLevel().transform);
            gameState.ReportEvent(GameEvent.SMALL_DETONATION);
            gameState.ReportEvent(GameEvent.MEDIUM_BANG);
            var managedObject = InterfaceHelper.GetInterface<ManagedObject>(hitObject);
            if (managedObject != null)
            {
                managedObject.Release();
            }
            else
            {
                Destroy(hitObject);
            }
        }
    
        if (bomb != null)
        {
            Destroy(bomb.gameObject);
        }
    }
}
