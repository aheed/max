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
    static readonly float[] riverSlopes = new float[] {0.5f, 0.5f, 1.0f, 2.0f, 2.0f};
    static readonly int neutralRiverSlopeIndex = 2;
    public float levelWidth = 8f;
    public float levelHeight = 80f;
    public float activationDistance = 20f;
    public float deactivationDistance = 20f;

    //// Game status
    int level = -1;
    float prepTimeForNextLevelQuotient = 0.90f;
    float lastLevelLowerEdgeY = 0f;
    int currentLevelIndex = 0;
    static int nofLevels = 2;
    GameObject[] levels = new GameObject[nofLevels];
    LevelContents latestLevel;
    MaxControl maxPlane;
    float landingStripBottomY;
    float landingStripTopY;
    float landingStripWidth;
    GameState gameState;
    List<GameObjectCollection> pendingActivation = new List<GameObjectCollection>();
    List<GameObjectCollection> activeObjects = new List<GameObjectCollection>();
    ////
    
    GameObject GetLevel() => levels[currentLevelIndex];

    void RotateLevels()
    {
        currentLevelIndex = (currentLevelIndex + 1) % 2; 
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
        var verts = coords.Select(v => new Vector3(v.x, v.y)).ToArray();
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
        var rsGameObject = Instantiate(riverSectionPrefab, lvlTransform);
        var rsLocalTransform = new Vector3(levelContents.riverLowerLeftCornerX * cellWidth, 0f, -0.2f);
        rsGameObject.transform.localPosition = rsLocalTransform;

        // MeshRenderer
        var rsMeshFilter = rsGameObject.AddComponent<MeshFilter>();
        var rsMeshRenderer = rsGameObject.AddComponent<MeshRenderer>();

        rsMeshRenderer.material = riverMaterial;

        // Mesh
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();
        int segments = 0;
        var y = 0f;
        float riverLowerLeftCornerX = 0f;
        foreach (var segment in levelContents.riverSegments)
        {
            var segmentHeight = segment.height * cellHeight;

            var xOffset = segment.slope * segment.height * cellWidth + segmentHeight * neutralSlope;

            //Debug.Log($"{riverLowerLeftCornerX} {riverWidth} {xOffset} {y} {segmentHeight}");
            var riverWidth = LevelBuilder.riverWidth * cellWidth;
            
            vertices.Add(new Vector3(riverLowerLeftCornerX, y, 0));
            vertices.Add(new Vector3(riverLowerLeftCornerX + riverWidth, y, 0));
            vertices.Add(new Vector3(riverLowerLeftCornerX + xOffset, y + segmentHeight, 0));
            vertices.Add(new Vector3(riverLowerLeftCornerX + riverWidth + xOffset, y + segmentHeight, 0));

            var triIndexOffset = segments * 4;
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

            y += segmentHeight;
            riverLowerLeftCornerX += xOffset;
            segments += 1;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        mesh.uv = uvs.ToArray();
        
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

    void Start()
    {
        var startPos = refobject.transform.position;
        /*startPos.x += 1.0f;
        startPos.y += 1.0f;
        startPos.z = 0.8f;*/
        maxPlane = Instantiate(maxPlanePrefab, startPos, Quaternion.identity, refobject.transform);
        maxPlane.refObject = refobject.transform;
        AddPlaneShadow(maxPlane.transform);

        startPos = transform.position;
        startPos.x += 2.0f;
        startPos.y += 2.0f;
        startPos.z = 0.8f;
        EnemyPlane enemyPlane = Instantiate(enemyPlanePrefab, startPos, Quaternion.identity);
        AddPlaneShadow(enemyPlane.transform);

        startPos = transform.position;
        startPos.x += 0.0f;
        startPos.y += 2.0f;
        startPos.z = 2.8f;
        EnemyPlane enemyPlane2 = Instantiate(enemyPlanePrefab, startPos, Quaternion.identity);
        AddPlaneShadow(enemyPlane2.transform);

        var levelLowerLeftCornerX = 0f;
        var newRefObjPos = new Vector3(levelLowerLeftCornerX + levelWidth / 2, 0f, 0f);
        refobject.transform.position = newRefObjPos;

        latestLevel = LevelBuilder.Build(true);        
        CreateLevel();
        PreventRelanding();
    }

    bool IsOverLandingStrip(Vector2 position)
    {
        return position.y > landingStripBottomY && 
            position.y < landingStripTopY &&
            Math.Abs(refobject.transform.position.y - maxPlane.transform.position.y) < landingStripWidth / 2;
    }

    void PreventRelanding()
    {
        landingStripTopY = landingStripBottomY;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameState == null)
        {
            gameState = FindObjectOfType<GameState>();
            gameState.RegisterObserver(this);
        }
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
        if (stateContents.gameStatus == GameStatus.FLYING)
        {
            //Debug.Log($"Alt: {maxPlane.GetAltitude()} ({MaxControl.landingAltitude})");
            if (maxPlane.GetAltitude() <= MaxControl.landingAltitude) 
            {
                //Debug.Log("Low");
                if (IsOverLandingStrip(maxPlane.GetPosition()))
                {
                    Debug.Log(">>>>>>>>> Landing <<<<<<<<<");
                    PreventRelanding();
                    stateContents.gameStatus = GameStatus.DECELERATING;
                }
            }
        }
        else if (stateContents.gameStatus == GameStatus.ACCELERATING)
        {
            stateContents.speed += gameState.acceleration * gameState.maxSpeed * Time.deltaTime;
            if (stateContents.speed > gameState.maxSpeed)
            {
                stateContents.speed = gameState.maxSpeed;
                stateContents.gameStatus = GameStatus.FLYING;
            }
        }
        else if (stateContents.gameStatus == GameStatus.DECELERATING)
        {
            stateContents.speed -= gameState.acceleration * gameState.maxSpeed * Time.deltaTime;
            if (stateContents.speed < 0f)
            {
                stateContents.speed = 0f;
                stateContents.gameStatus = GameStatus.REFUELLING;
            }
        }
        else if (stateContents.gameStatus == GameStatus.REFUELLING)
        {
            // Todo: Refuelling and repair
            
            stateContents.gameStatus = GameStatus.ACCELERATING;
        }

        // Update refobject position
        Vector2 levelVelocity = new(stateContents.speed, stateContents.speed);
        Vector3 delta = levelVelocity * Time.deltaTime;
        refobject.transform.position += delta;
    }

    public void OnGameStatusChanged(GameStatus gameStatus)
    {
        if(gameStatus == GameStatus.DEAD)
        {
            gameState.GetStateContents().speed = 0f;
        }
    }
}
