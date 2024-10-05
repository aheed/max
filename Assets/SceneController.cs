using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public enum GameStatus
{
    ACCELERATING = 0,
    DECELERATING,
    FLYING,
    COLLIDED,
    OUT_OF_FUEL,
    KILLED_BY_FLACK,
    REFUELLING,
    LOADING_BOMBS,
    REPAIRING,
    DEAD,
    FINISHED  // Rule Britannia!
}

public class GameObjectCollection
{
    public float yCoord;
    public List<GameObject> gameObjects;
}

public class SceneController : MonoBehaviour, IGameStateObserver
{
    public MaxControl maxPlanePrefab;
    public EnemyPlane enemyPlanePrefab;
    public ShadowControl shadowControlPrefab;
    public GameObject groundPrefab;
    public GameObject riverSectionPrefab;
    public GameObject roadPrefab;
    public GameObject landingStripPrefab;
    public GameObject housePrefab;
    public GameObject flackGunPrefab;
    public GameObject tankPrefab;
    public GameObject tree1Prefab;
    public GameObject tree2Prefab;
    public GameObject levelPrefab;
    public GameObject bombSplashPrefab;
    public GameObject bombCraterPrefab;
    public GameObject mushroomCloudPrefab;
    public refobj refobject;
    public float width = 1;
    public float height = 1;
    public float riverSectionHeight = 20f;
    public float maxSegmentHeight = 3.0f;
    public float minSegmentHeight = 0.5f;
    public float minDistanceRiverAirstrip = 5.0f;
    public float maxDistanceRiverToAdjust = 2.0f;
    public float approachQuotient = 0.2f;
    public Material riverMaterial;
    public Material roadMaterial;
    public Material groundMaterial;
    public Material landingStripMaterial;
    public static readonly float[] riverSlopes = new float[] {0.5f, 0.5f, 1.0f, 2.0f, 2.0f};
    public static readonly int neutralRiverSlopeIndex = 2;
    public float levelWidth = 8f;
    public float levelHeight = 80f;
    public float activationDistance = 20f;
    public float deactivationDistance = 20f;
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
    public float enemyPlaneIntervalSecMax = 15f;
    public float enemyPlaneIntervalSecMin = 5f;

    //// Game status
    int level = -1;
    float prepTimeForNextLevelQuotient = 0.90f;
    float lastLevelLowerEdgeY = 0f;
    int currentLevelIndex = 0;
    static int nofLevels = 2;
    GameObject[] levels;
    LevelContents latestLevel;
    MaxControl maxPlane;
    float landingStripBottomY;
    float landingStripTopY;
    float landingStripWidth;
    GameState gameState;
    List<GameObjectCollection> pendingActivation = new List<GameObjectCollection>();
    List<GameObjectCollection> activeObjects = new List<GameObjectCollection>();
    float restartCoolDownSeconds = 0f;
    float bombLoadCooldownSec = 0f;
    float repairCooldownSec = 0f;
    float enemyPlaneCooldown = 0f;
    GameObject riverSectionGameObject;
    List<Vector2> riverVerts;
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
        var llcx = level * levelHeight * riverSlopes[neutralRiverSlopeIndex];
        var llcy = level * levelHeight;
        var newLevel = Instantiate(levelPrefab, new Vector3(llcx, llcy, 0f), Quaternion.identity);
        levels[currentLevelIndex] = newLevel;
        lastLevelLowerEdgeY = llcy;
    }


    void AddPlaneShadow(Transform parent)
    {        
        Instantiate(shadowControlPrefab, transform.position, Quaternion.identity, parent);
    }

    Mesh CreateQuadMesh(IEnumerable<Vector2> coords)
    {
        var verts = coords.Select(v => (Vector3)v).ToArray();
        if (verts.Length % 4 != 0)
        {
            throw new System.Exception("Length of param must a multiple of 4");
        }
        
        var triangles = new List<int>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();
        for (int i = 0; i < verts.Length / 4; i++)
        {
            var triIndexOffset = i * 4;
            triangles.Add(triIndexOffset + 0);
            triangles.Add(triIndexOffset + 2);
            triangles.Add(triIndexOffset + 1);
            triangles.Add(triIndexOffset + 2);
            triangles.Add(triIndexOffset + 3);
            triangles.Add(triIndexOffset + 1);

            normals.Add(-Vector3.forward);
            normals.Add(-Vector3.forward);
            normals.Add(-Vector3.forward);
            normals.Add(-Vector3.forward);

            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(1, 1));
        }

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        mesh.uv = uvs.ToArray();
        return mesh;
    }

    // Create game objects
    // llcx, llcy: Lower Left Corner of the level
    public List<GameObjectCollection> PopulateScene(LevelContents levelContents)
    {
        float cellWidth = levelWidth / LevelContents.gridWidth;
        float cellHeight = levelHeight / LevelContents.gridHeight;
        float neutralSlope = riverSlopes[neutralRiverSlopeIndex];
        var midX = LevelContents.gridWidth / 2;

        var lvlTransform = GetLevel().transform;

        // Ground
        var grGameObject = Instantiate(groundPrefab, lvlTransform);
        
        var grUpperCornerOffsetX = levelHeight * neutralSlope;

        var grMeshFilter = grGameObject.AddComponent<MeshFilter>();
        var grMeshRenderer = grGameObject.AddComponent<MeshRenderer>();
        grMeshRenderer.material = groundMaterial;

        var grVerts = new List<Vector2>
        {
            new Vector2(0f, 0f),
            new Vector2(levelWidth, 0f),
            new Vector2(grUpperCornerOffsetX, levelHeight),
            new Vector2(levelWidth + grUpperCornerOffsetX, levelHeight)
        };

        var grMesh = CreateQuadMesh(grVerts);
        grMeshFilter.mesh = grMesh;

        // Landing Strip
        var lsWidth = LevelBuilder.landingStripWidth * cellWidth;
        var lsHeight = LevelBuilder.landingStripHeight * cellHeight;

        var lsGameObject = Instantiate(landingStripPrefab, lvlTransform);
        var lsLocalTransform = new Vector3((LevelContents.gridWidth / 2) * cellWidth - (lsWidth / 2), 0f, -0.21f);
        lsGameObject.transform.localPosition = lsLocalTransform;
        landingStripBottomY = lsGameObject.transform.position.y;
        landingStripTopY = landingStripBottomY + lsHeight;
        landingStripWidth = lsWidth;
        
        var lsUpperCornerOffsetX = lsHeight * neutralSlope;

        var lsllcX = 0;
        var lslrcX = lsWidth;
        var lsulcX = lsUpperCornerOffsetX;
        var lsurcX = lslrcX + lsUpperCornerOffsetX;
        var lsllcY = 0;
        var lslrcY = 0;
        var lsulcY = lsHeight;
        var lsurcY = lsHeight;

        var lsMeshFilter = lsGameObject.AddComponent<MeshFilter>();
        var lsMeshRenderer = lsGameObject.AddComponent<MeshRenderer>();
        lsMeshRenderer.material = landingStripMaterial;

        var lsVerts = new List<Vector2>
        {
            new Vector2(lsllcX, lsllcY),
            new Vector2(lslrcX, lslrcY),
            new Vector2(lsulcX, lsulcY),
            new Vector2(lsurcX, lsurcY)
        };

        var lsMesh = CreateQuadMesh(lsVerts);
        lsMeshFilter.mesh = lsMesh;


        // River
        riverSectionGameObject = Instantiate(riverSectionPrefab, lvlTransform);
        var rsLocalTransform = new Vector3(levelContents.riverLowerLeftCornerX * cellWidth, 0f, -0.2f);
        riverSectionGameObject.transform.localPosition = rsLocalTransform;

        // MeshRenderer
        var rsMeshFilter = riverSectionGameObject.AddComponent<MeshFilter>();
        var rsMeshRenderer = riverSectionGameObject.AddComponent<MeshRenderer>();

        rsMeshRenderer.material = riverMaterial;

        // Mesh
        var y = 0f;
        float riverLowerLeftCornerX = 0f;

        riverVerts = levelContents.riverSegments.SelectMany(segment => 
        {
            var segmentHeight = segment.height * cellHeight;
            var xOffset = segment.slope * segment.height * cellWidth + segmentHeight * neutralSlope;
            var riverWidth = LevelBuilder.riverWidth * cellWidth;
            
            var ret = new List<Vector2>
            {
                new Vector2(riverLowerLeftCornerX, y),
                new Vector2(riverLowerLeftCornerX + riverWidth, y),
                new Vector2(riverLowerLeftCornerX + xOffset, y + segmentHeight),
                new Vector2(riverLowerLeftCornerX + riverWidth + xOffset, y + segmentHeight)
            };
            y += segmentHeight;
            riverLowerLeftCornerX += xOffset;
            return ret;
        }).ToList();
        
        var mesh = CreateQuadMesh(riverVerts);
        rsMeshFilter.mesh = mesh;

        
        
        // Roads
        foreach (var road in levelContents.roads)
        {
            var roadGameObject = Instantiate(roadPrefab, lvlTransform);
            var roadLocalTransform = new Vector3(road * cellHeight * neutralSlope, road * cellHeight, -0.2f);
            roadGameObject.transform.localPosition = roadLocalTransform;            

            var roadWidth = LevelContents.gridWidth * cellWidth;
            var roadHeight = LevelBuilder.roadHeight * cellHeight;

            var meshFilter = roadGameObject.AddComponent<MeshFilter>();
            var meshRenderer = roadGameObject.AddComponent<MeshRenderer>();
            meshRenderer.material = roadMaterial;

            var roadVerts = new List<Vector2>
            {
                new Vector2(0, 0),
                new Vector2(roadWidth, 0),
                new Vector2(0, roadHeight),
                new Vector2(roadWidth, roadHeight)
            };

            var roadMesh = CreateQuadMesh(roadVerts);
            meshFilter.mesh = roadMesh;
        }

        // Bridges

        // Houses
        foreach (var house in levelContents.houses)
        {
            var houseGameObject = Instantiate(housePrefab, lvlTransform);
            var houseLocalTransform = new Vector3(house.x * cellWidth + house.y * cellHeight * neutralSlope, house.y * cellHeight, -0.2f);
            houseGameObject.transform.localPosition = houseLocalTransform;
        }

        List<GameObjectCollection> ret = new();

        // Single cell items: Flack guns, trees, tanks
        for (var ytmp = 0; ytmp < LevelContents.gridHeight; ytmp++)
        {
            var gameObjects = new List<GameObject>();
            ret.Add(new GameObjectCollection {
                yCoord = ytmp * cellHeight, // level relative coordinate
                gameObjects = gameObjects
            });
            for (var xtmp = 0; xtmp < LevelContents.gridWidth; xtmp++)    
            {
                GameObject selectedPrefab = null;
                switch (levelContents.cells[xtmp, ytmp])
                {
                    case CellContent.FLACK_GUN:
                        selectedPrefab = flackGunPrefab;
                        break;

                    case CellContent.TANK:
                        selectedPrefab = tankPrefab;
                        break;

                    case CellContent.TREE1:
                        selectedPrefab = tree1Prefab;
                        break;

                    case CellContent.TREE2:
                        selectedPrefab = tree2Prefab;
                        break;
                }

                if (selectedPrefab != null)
                {
                    var itemGameObject = Instantiate(selectedPrefab, lvlTransform);
                    var itemLocalTransform = new Vector3(xtmp * cellWidth + ytmp * cellHeight * neutralSlope, ytmp * cellHeight, -0.2f);
                    itemGameObject.transform.localPosition = itemLocalTransform;
                    gameObjects.Add(itemGameObject);
                }

            }
        }

        return ret;
    }

    void CreateLevel()
    {
        RotateLevels();
        var newGameObjects = PopulateScene(latestLevel)
        .Select(goc => new GameObjectCollection {yCoord = goc.yCoord + lastLevelLowerEdgeY, gameObjects = goc.gameObjects})
        .ToList();
        foreach (var collection in newGameObjects)
        {
            foreach (var gameObject in collection.gameObjects)
            {
                var collider = gameObject.GetComponent<Collider2D>();
                if (collider != null)
                {
                    collider.enabled = false;
                }
                gameObject.SetActive(false);
            }
        }
        pendingActivation.AddRange(newGameObjects);
    }

    void StartNewGame()
    {
        level = -1;
        var levelLowerLeftCornerX = 0f;
        var newRefObjPos = new Vector3(levelLowerLeftCornerX + levelWidth / 2, 0f, 0f);
        refobject.transform.position = newRefObjPos;

        if (maxPlane == null)
        {
            maxPlane = Instantiate(maxPlanePrefab, refobject.transform);
            maxPlane.refObject = refobject.transform;            
            AddPlaneShadow(maxPlane.transform);
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

        latestLevel = LevelBuilder.Build(true);
        CreateLevel();
        PreventRelanding();
        gameState = GetGameState();
        gameState.Reset();
        gameState.ReportEvent(GameEvent.START);
    }

    void Start()
    {
        StartNewGame();
    }

    bool IsOverLandingStrip(Vector2 position)
    {
        var offsetX = (position.y - refobject.transform.position.y) * riverSlopes[neutralRiverSlopeIndex];
        return position.y > landingStripBottomY && 
            position.y < landingStripTopY &&            
            Math.Abs((refobject.transform.position.x + offsetX - position.x)) < landingStripWidth / 2;
    }

    bool IsOverRiver(Vector2 position)
    {
        var xOffset = riverSectionGameObject.transform.position.x;
        var yOffset = riverSectionGameObject.transform.position.y;

        // find segment
        var segmentIndex = 0;
        var maxSegmentIndex = (riverVerts.Count - 1) / 4;
        while (segmentIndex <= maxSegmentIndex)
        {
            if ((riverVerts[segmentIndex * 4].y + yOffset) < position.y &&
                (riverVerts[segmentIndex * 4 + 2].y + yOffset) >= position.y)
            {
                break;
            }
            segmentIndex++;
        }

        if (segmentIndex > maxSegmentIndex)
        {
            return false;
        }

        // interpolate river edges x
        var ydiff = position.y - (riverVerts[segmentIndex*4].y + yOffset);
        var xdiff = ydiff * ((riverVerts[segmentIndex*4 + 2].x + xOffset) - (riverVerts[segmentIndex*4].x + xOffset)) / ((riverVerts[segmentIndex*4 + 2].y + yOffset) - (riverVerts[segmentIndex*4].y + yOffset));

        // compare to position x
        var overRiverSegment =
            position.x > ((riverVerts[segmentIndex*4].x + xOffset) + xdiff) &&
            position.x < ((riverVerts[segmentIndex*4+1].x + xOffset) + xdiff);
        return overRiverSegment;
    }

    void PreventRelanding()
    {
        landingStripTopY = landingStripBottomY;
    }

    GameState GetGameState() 
    {
        if (gameState == null)
        {
            gameState = FindObjectOfType<GameState>();
            gameState.RegisterObserver(this);
        }
        return gameState;
    }

    void SetEnemyPlaneCooldown()
    {
        enemyPlaneCooldown = UnityEngine.Random.Range(enemyPlaneIntervalSecMin, enemyPlaneIntervalSecMax);
    }

    void SpawnEnemyPlane()
    {
        var startPos = refobject.transform.position;
        //startPos = transform.position;
        startPos.x += UnityEngine.Random.Range(-gameState.maxHorizPosition - 2 * gameState.maxAltitude, gameState.maxHorizPosition - 2 * gameState.maxAltitude);
        startPos.y += -gameState.maxAltitude;
        startPos.z = UnityEngine.Random.Range(gameState.minSafeAltitude, gameState.maxAltitude);
        EnemyPlane enemyPlane = Instantiate(enemyPlanePrefab, startPos, Quaternion.identity);
        enemyPlane.refObject = refobject.transform;
        enemyPlane.speed = 
            UnityEngine.Random.Range(
                enemyPlaneSpeedMin * gameState.maxSpeed,
                enemyPlaneSpeedMax * gameState.maxSpeed);
        AddPlaneShadow(enemyPlane.transform);
    }

    // Update is called once per frame
    void Update()
    {
        gameState = GetGameState();
        GameStateContents stateContents = gameState.GetStateContents();

        if (refobject.transform.position.y > (lastLevelLowerEdgeY + levelHeight * prepTimeForNextLevelQuotient))
        {
            Debug.Log("Time to add new level ***************");
            latestLevel = LevelBuilder.Build(latestLevel.riverEndsLeftOfAirstrip);
            CreateLevel();
        }

        while (pendingActivation.Count > 0 && refobject.transform.position.y + activationDistance > pendingActivation.First().yCoord)
        {
            //Debug.Log($"Time to activate more game objects at {refobject.transform.position.y} {pendingActivation.First().yCoord}");

            // Activate objects
            var collection = pendingActivation.First();
            foreach (var gameObject in collection.gameObjects)
            {
                var collider = gameObject.GetComponent<Collider2D>();
                if (collider != null)
                {
                    collider.enabled = true;
                }
                gameObject.SetActive(true);

            }

            // Move collection to the activeObjects collection
            pendingActivation.RemoveAt(0);
            activeObjects.Add(collection);
        }

        while (activeObjects.Count > 0 && refobject.transform.position.y - deactivationDistance > activeObjects.First().yCoord)
        {
            //Debug.Log($"Time to destroy game objects at {refobject.transform.position.y} {activeObjects.First().yCoord}");

            var collection = activeObjects.First();
            foreach (var gameObject in collection.gameObjects)
            {
                Destroy(gameObject);
            }

            activeObjects.RemoveAt(0);
        }

        // Update game state
        if (stateContents.gameStatus == GameStatus.KILLED_BY_FLACK ||
            stateContents.gameStatus == GameStatus.COLLIDED)
        {
            if (maxPlane.GetAltitude() <= MaxControl.minAltitude)
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

                //// TEMP !!!!!!!!!!!!!!!!!11
                if (IsOverLandingStrip(maxPlane.GetPosition()))
                {
                    gameState.SetAlert(GameState.landingAlert);
                }
                else
                {
                    gameState.SetAlert("-");
                }
                ////

                enemyPlaneCooldown -= Time.deltaTime;
                if (enemyPlaneCooldown <= 0)
                {
                    SpawnEnemyPlane();
                    SetEnemyPlaneCooldown();
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
                gameState.SetStatus(GameStatus.REFUELLING);
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
        Vector2 levelVelocity = new(stateContents.speed, stateContents.speed);
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
    }

    public void OnBombLanded(Bomb bomb, GameObject hitObject) 
    {
        if (hitObject == null)
        {
            var prefab = IsOverRiver(bomb.GetPosition()) ? bombSplashPrefab : bombCraterPrefab;
            Vector3 craterPosition = bomb.GetPosition();
            craterPosition.z = -0.22f;
            var c = Instantiate(prefab, craterPosition, Quaternion.identity, GetLevel().transform);
        }
        else 
        {
            Instantiate(mushroomCloudPrefab, hitObject.transform.position, Quaternion.identity, GetLevel().transform);
            Destroy(hitObject);
        }
        //var s = IsOverRiver(bomb.GetPosition()) ? "Splash!" : "Booom!";
        //Debug.Log($"Bomb on the scene at {bomb.GetPosition().x}, {bomb.GetPosition().y} ******* {s} {bomb.transform.position} {c.transform.position}");
        Destroy(bomb.gameObject);
    }
}
