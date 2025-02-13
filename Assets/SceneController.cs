using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

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

public class SceneController : MonoBehaviour
{
    public MaxControl maxPlanePrefab;
    public EnemyPlane enemyPlanePrefab;
    public ShadowControl shadowControlPrefab;
    public GameObject visibleAreaMarkerPrefab;
    public GameObject riverSectionPrefab;
    public GameObject roadPrefab;
    public GameObject landingStripPrefab;
    public ExpHouse housePrefab;
    public ManagedObject3 flackGunPrefab;
    public ManagedObject3 tankPrefab;
    public ManagedObject3 tree1Prefab;
    public ManagedObject3 tree2Prefab;
    public GameObject levelPrefab;
    public GameObject bombSplashPrefab;
    public GameObject bombCraterPrefab;
    public GameObject mushroomCloudPrefab;
    public ManagedObject3 boat1Prefab;
    public ManagedObject3 boat2Prefab;
    public ManagedObject3 vehicle1Prefab;
    public ManagedObject3 vehicle2Prefab;
    public ManagedObject3 enemyHangarPrefab;
    public GameObject parkedPlanePrefab;
    public GameObject balloonPrefab;
    public GameObject balloonShadowPrefab;
    public bridge bridgePrefab;
    public ManagedObject3 carPrefab;
    public GameObject airstripEndPrefab;
    public GameObject hangarPrefab;
    public EnemyHQ enemyHqPrefab;
    public GameObject bigHousePrefab;
    public GameObject balloonParentPrefab;
    public refobj refobject;
    public float width = 1;
    public float height = 1;
    public float riverSectionHeight = 20f;
    public float maxSegmentHeight = 3.0f;
    public float minSegmentHeight = 0.5f;
    public float minDistanceRiverAirstrip = 5.0f;
    public float maxDistanceRiverToAdjust = 2.0f;
    public float roadHeight = 0.4f;
    public Material riverMaterial;
    public Material riverBankMaterial;
    public Material roadMaterial;
    public Material landingStripMaterial;
    public Material visibleAreaMarkerMaterial;
    public static readonly float[] riverSlopes = new float[] {0.5f, 0.5f, 1.0f, 2.0f, 2.0f};
    public static readonly int neutralRiverSlopeIndex = 2;
    float levelWidth;
    public float levelHeight = 80f;
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
    float lastLevelLowerEdgeY = 0f;
    int currentLevelIndex = 0;
    static int nofLevels = 2;
    GameObject[] levels;
    LevelContents latestLevel;
    Task<LevelContents> newLevelTask;
    int framesToBuildLevelDbg;
    MaxControl maxPlane;
    float landingStripBottomY;
    float landingStripTopY;
    float landingStripWidth;
    GameState gameState;
    List<GameObjectCollection3> pendingActivation = new();
    List<GameObjectCollection3> activeObjects = new();
    float restartCoolDownSeconds = 0f;
    float bombLoadCooldownSec = 0f;
    float repairCooldownSec = 0f;
    float enemyPlaneCooldown = 0f;
    float windCooldown = 0f;
    GameObject riverSectionGameObject;
    List<Vector2> riverVerts;
    List<float> roadLowerEdgesY;
    public static readonly Color[] houseColors = new Color[] { Color.yellow, new Color(0.65f, 0.1f, 0f), new Color(0.65f, 0.57f, 0f)};
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
        var llcx = level * levelHeight * riverSlopes[neutralRiverSlopeIndex];
        var llcy = level * levelHeight;
        var newLevel = Instantiate(levelPrefab, new Vector3(llcx, llcy, 0f), Quaternion.identity);
        levels[currentLevelIndex] = newLevel;
        lastLevelLowerEdgeY = llcy;
        balloonParent = Instantiate(balloonParentPrefab, newLevel.transform);
        InterfaceHelper.GetInterface<BalloonManager>(balloonParent).SetRefTransform(refobject.transform);
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
    public List<GameObjectCollection3> PopulateScene(LevelContents levelContents)
    {
        GameStateContents stateContents = gameState.GetStateContents();
        float cellWidth = levelWidth / LevelContents.gridWidth;
        float cellHeight = levelHeight / LevelContents.gridHeight;
        float neutralSlope = riverSlopes[neutralRiverSlopeIndex];
        var midX = LevelContents.gridWidth / 2;

        var lvlTransform = GetLevel().transform;

        // Landing Strip
        {
            var lsWidth = LevelBuilder.landingStripWidth * cellWidth;
            var lsHeight = LevelBuilder.landingStripHeight * cellHeight;

            var lsTopEnd = Instantiate(airstripEndPrefab, lvlTransform);
            var lsBottomEnd = Instantiate(airstripEndPrefab, lvlTransform);
            var topSpriteR = lsTopEnd.gameObject.GetComponent<SpriteRenderer>();
            var endSpriteHeight = topSpriteR.bounds.size.y;

            var lsGameObject = Instantiate(landingStripPrefab, lvlTransform);
            
            var lsLocalTransform = new Vector3((LevelContents.gridWidth / 2) * cellWidth - (lsWidth / 2), 0f, -0.21f);
            lsGameObject.transform.localPosition = lsLocalTransform;

            lsLocalTransform.x += lsWidth / 2;
            lsLocalTransform.y += endSpriteHeight / 2;
            lsBottomEnd.transform.localPosition = lsLocalTransform;        
            
            landingStripBottomY = lsGameObject.transform.position.y;
            landingStripTopY = landingStripBottomY + lsHeight;
            landingStripWidth = lsWidth;
            
            var lsUpperCornerOffsetX = lsHeight * neutralSlope;

            lsLocalTransform.x += lsUpperCornerOffsetX - endSpriteHeight * neutralSlope;
            lsLocalTransform.y += lsHeight - endSpriteHeight;
            lsTopEnd.transform.localPosition = lsLocalTransform;

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
        }

        // Enemy Airstrips
        foreach(var enemyAirstrip in levelContents.enemyAirstrips)
        {            
            var lsWidth = LevelBuilder.landingStripWidth * cellWidth;
            var lsHeight = LevelBuilder.enemyAirstripHeight * cellHeight;

            var lsTopEnd = Instantiate(airstripEndPrefab, lvlTransform);
            var lsTopEnd2 = Instantiate(airstripEndPrefab, lvlTransform);
            var lsBottomEnd = Instantiate(airstripEndPrefab, lvlTransform);
            var lsBottomEnd2 = Instantiate(airstripEndPrefab, lvlTransform);
            var topSpriteR = lsTopEnd.gameObject.GetComponent<SpriteRenderer>();
            var endSpriteHeight = topSpriteR.bounds.size.y;

            var lsGameObject = Instantiate(landingStripPrefab, lvlTransform);

            var stripOffsetY = enemyAirstrip * cellHeight;            
            var stripOffsetX = stripOffsetY * neutralSlope;
            var lsLocalTransform = new Vector3(stripOffsetX + ((LevelContents.gridWidth / 2) - LevelBuilder.enemyAirstripXDistance) * cellWidth, stripOffsetY, -0.21f);
            lsGameObject.transform.localPosition = lsLocalTransform;

            lsLocalTransform.x += lsWidth / 2;
            lsLocalTransform.y += endSpriteHeight / 2;
            lsBottomEnd.transform.localPosition = lsLocalTransform;

            lsLocalTransform.x += 2 * endSpriteHeight * neutralSlope;
            lsLocalTransform.y += 2 * endSpriteHeight;
            lsBottomEnd2.transform.localPosition = lsLocalTransform;
            
            var lsUpperCornerOffsetX = lsHeight * neutralSlope;

            lsLocalTransform.x += lsUpperCornerOffsetX - 3 * endSpriteHeight * neutralSlope;
            lsLocalTransform.y += lsHeight - 3 * endSpriteHeight;
            lsTopEnd.transform.localPosition = lsLocalTransform;

            lsLocalTransform.x -= 2 * endSpriteHeight * neutralSlope;
            lsLocalTransform.y -= 2 * endSpriteHeight;
            lsTopEnd2.transform.localPosition = lsLocalTransform;

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

            var llc = new Vector2(lsllcX, lsllcY);
            var lrc = new Vector2(lslrcX, lslrcY);
            var ulc = new Vector2(lsulcX, lsulcY);
            var urc = new Vector2(lsurcX, lsurcY);
            var lsVerts = new List<Vector2> {llc, lrc, ulc, urc};

            var lsMesh = CreateQuadMesh(lsVerts);
            lsMeshFilter.mesh = lsMesh;

            PolygonCollider2D polygonCollider = lsGameObject.AddComponent<PolygonCollider2D>();
            polygonCollider.isTrigger = true;
            polygonCollider.points = new Vector2[] {llc, lrc, urc, ulc};

            // parked planes
            var nofParkedPlanes = UnityEngine.Random.Range(1, 4);
            for (int i = 0; i < nofParkedPlanes; i++)
            {
                var parkedPlane = Instantiate(parkedPlanePrefab, lsGameObject.transform);
                var parkedPlaneY = lsllcY + (i+1) * lsHeight / (nofParkedPlanes+1);
                var parkedPlaneX = parkedPlaneY * neutralSlope + lsWidth / 2;
                var ppLocalTransform = new Vector3(parkedPlaneX, parkedPlaneY, -0.01f);
                parkedPlane.transform.localPosition = ppLocalTransform;
            }
        }

        if (levelContents.city != null)
        {
            var cityWidth = LevelContents.gridWidth * cellWidth;
            var cityHeight = (levelContents.city.yEnd - levelContents.city.yStart) * cellHeight;

            var cityGameObject = Instantiate(landingStripPrefab, lvlTransform);

            var cityOffsetY = levelContents.city.yStart * cellHeight;
            var cityOffsetX = cityOffsetY * neutralSlope;
            var cityLocalTransform = new Vector3(cityOffsetX, cityOffsetY, -0.22f);
            cityGameObject.transform.localPosition = cityLocalTransform;

            var lsUpperCornerOffsetX = cityHeight * neutralSlope;

            var cityllcX = 0;
            var citylrcX = cityWidth;
            var cityulcX = lsUpperCornerOffsetX;
            var cityurcX = citylrcX + lsUpperCornerOffsetX;
            var cityllcY = 0;
            var citylrcY = 0;
            var cityulcY = cityHeight;
            var cityurcY = cityHeight;

            var cityMeshFilter = cityGameObject.AddComponent<MeshFilter>();
            var cityMeshRenderer = cityGameObject.AddComponent<MeshRenderer>();
            cityMeshRenderer.material = landingStripMaterial;

            var cityVerts = new List<Vector2>
            {
                new Vector2(cityllcX, cityllcY),
                new Vector2(citylrcX, citylrcY),
                new Vector2(cityulcX, cityulcY),
                new Vector2(cityurcX, cityurcY)
            };

            var cityMesh = CreateQuadMesh(cityVerts);
            cityMeshFilter.mesh = cityMesh;

            stateContents.enemyHQs = levelContents.city.enemyHQs.Select(hq =>
            {
                var hqInstance = Instantiate(enemyHqPrefab, lvlTransform);
                if (hq.bombed)
                {
                    hqInstance.SetBombed();
                }
                var targetOffsetY = hq.y * cellHeight;
                var targetOffsetX = targetOffsetY * neutralSlope;
                var targetLocalTransform = new Vector3(targetOffsetX + (LevelContents.gridWidth / 2) * cellWidth, targetOffsetY, -0.23f);
                hqInstance.transform.localPosition = targetLocalTransform;
                return hqInstance;
            }).ToList();

            // Big houses
            var sortedBigHouseList = levelContents.city.bigHouses
                .OrderBy(h => h.y)
                .ToList();
            var zSortOrder = -0.23f;
            var zSortOrderIncrement = 0.0001f;
            foreach (var bigHouse in sortedBigHouseList)
            {
                var bigHouseGameObject = Instantiate(bigHousePrefab, lvlTransform);
                var bigHouseOffsetY = bigHouse.y * cellHeight;
                var bigHouseOffsetX = bigHouseOffsetY * neutralSlope;
                var bigHouseXPosRel = bigHouse.x * cellWidth;
                var bigHouseLocalTransform = new Vector3(bigHouseOffsetX + bigHouseXPosRel, bigHouseOffsetY, zSortOrder);
                bigHouseGameObject.transform.localPosition = bigHouseLocalTransform;
                zSortOrder += zSortOrderIncrement;
            }
        }


        // River
        riverSectionGameObject = Instantiate(riverSectionPrefab, lvlTransform);
        var rsLocalTransform = new Vector3(levelContents.riverLowerLeftCornerX * cellWidth, 0f, -0.2f);
        riverSectionGameObject.transform.localPosition = rsLocalTransform;
        var riverLeftBank = new GameObject("riverbank");
        riverLeftBank.transform.parent = lvlTransform;
        var riverLeftBankLocalTransform = new Vector3(rsLocalTransform.x, rsLocalTransform.y, rsLocalTransform.z -0.01f);
        riverLeftBank.transform.localPosition = riverLeftBankLocalTransform;

        // River MeshRenderers
        var rsMeshFilter = riverSectionGameObject.AddComponent<MeshFilter>();
        var rsMeshRenderer = riverSectionGameObject.AddComponent<MeshRenderer>();
        rsMeshRenderer.material = riverMaterial;
        var bankMeshFilter = riverLeftBank.AddComponent<MeshFilter>();
        var bankMeshRenderer = riverLeftBank.AddComponent<MeshRenderer>();
        bankMeshRenderer.material = riverBankMaterial;

        // River Meshes
        var y = 0f;
        float riverLowerLeftCornerX = 0f;
        var riverWidth = LevelBuilder.riverWidth * cellWidth;        

        riverVerts = levelContents.riverSegments.SelectMany(segment => 
        {
            var segmentHeight = segment.height * cellHeight;
            var xOffset = segment.slope * segment.height * cellHeight + segmentHeight * neutralSlope;
            
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

        var riverBankVerts = riverVerts.Select((vert, index) => {
            var x = index % 2 == 0 ? vert.x : vert.x - riverWidth + riverBankWidth;
            return new Vector2(x, vert.y);
        }).ToList();
        
        var mesh = CreateQuadMesh(riverVerts);
        rsMeshFilter.mesh = mesh;
        var riverBankMesh = CreateQuadMesh(riverBankVerts);
        bankMeshFilter.mesh = riverBankMesh;


        // Parallel Road
        GameObject paraRoad = new GameObject("parallel road");
        GameObject paraRoadWide = new GameObject("parallel road wide");
        paraRoad.transform.parent = lvlTransform;
        paraRoadWide.transform.parent = lvlTransform;
        var prLocalTransform = new Vector3(levelContents.roadLowerLeftCornerX * cellWidth, 0f, -0.2f);
        paraRoadWide.transform.localPosition = prLocalTransform;
        prLocalTransform.z -= 0.01f;
        paraRoad.transform.localPosition = prLocalTransform;

        // MeshRenderer
        var prMeshFilter = paraRoad.AddComponent<MeshFilter>();
        var prMeshFilterWide = paraRoadWide.AddComponent<MeshFilter>();
        var prMeshRenderer = paraRoad.AddComponent<MeshRenderer>();
        var prMeshRendererWide = paraRoadWide.AddComponent<MeshRenderer>();
        prMeshRenderer.material = landingStripMaterial;
        prMeshRendererWide.material = riverBankMaterial;

        // Mesh
        y = 0f;
        float prLowerLeftCornerX = 0f;

        var paraRoadVerts = levelContents.roadSegments.SelectMany(segment => 
        {
            var segmentHeight = segment.height * cellHeight;
            var xOffset = segment.slope * segment.height * cellHeight + segmentHeight * neutralSlope;
            
            var ret = new List<Vector2>
            {
                new Vector2(prLowerLeftCornerX, y),
                new Vector2(prLowerLeftCornerX + parallelRoadWidth, y),
                new Vector2(prLowerLeftCornerX + xOffset, y + segmentHeight),
                new Vector2(prLowerLeftCornerX + parallelRoadWidth + xOffset, y + segmentHeight)
            };
            y += segmentHeight;
            prLowerLeftCornerX += xOffset;
            return ret;
        }).ToList();

        var paraRoadWideVerts = paraRoadVerts.Select((vert, index) => {
            var x = index % 2 == 0 ? vert.x - parllelRoadSideWidth : vert.x + parllelRoadSideWidth;
            return new Vector2(x, vert.y);
        }).ToList();

        var prMesh = CreateQuadMesh(paraRoadVerts);
        var prMeshWide = CreateQuadMesh(paraRoadWideVerts);
        prMeshFilter.mesh = prMesh;
        prMeshFilterWide.mesh = prMeshWide;

        GameObjectCollection3[] ret = new GameObjectCollection3[LevelContents.gridHeight];
        for (var ytmp = 0; ytmp < LevelContents.gridHeight; ytmp++)
        {
            ret[ytmp] = new GameObjectCollection3 {
                zCoord = ytmp * cellHeight, // level relative coordinate
                managedObjects = new List<Action>()
            };
        }

        // Object pools. Could be injected from outside or created earlier.
        var flakGunManagerFactory = new ObjectManagerFactory3(flackGunPrefab, lvlTransform, ObjectManagerFactory3.PoolType.Stack);
        var tankManagerFactory = new ObjectManagerFactory3(tankPrefab, lvlTransform, ObjectManagerFactory3.PoolType.Stack);
        var tree1ManagerFactory = new ObjectManagerFactory3(tree1Prefab, lvlTransform, ObjectManagerFactory3.PoolType.Stack);
        var tree2ManagerFactory = new ObjectManagerFactory3(tree2Prefab, lvlTransform, ObjectManagerFactory3.PoolType.Stack);
        var boat1ManagerFactory = new ObjectManagerFactory3(boat1Prefab, lvlTransform, ObjectManagerFactory3.PoolType.None);
        var boat2ManagerFactory = new ObjectManagerFactory3(boat2Prefab, lvlTransform, ObjectManagerFactory3.PoolType.None);
        var vehicle1ManagerFactory = new ObjectManagerFactory3(vehicle1Prefab, lvlTransform, ObjectManagerFactory3.PoolType.None);
        var vehicle2ManagerFactory = new ObjectManagerFactory3(vehicle2Prefab, lvlTransform, ObjectManagerFactory3.PoolType.None);
        var enemyHangarManagerFactory = new ObjectManagerFactory3(enemyHangarPrefab, lvlTransform, ObjectManagerFactory3.PoolType.None);
        //var hangarManagerFactory = new ObjectManagerFactory3(hangarPrefab, lvlTransform, ObjectManagerFactory3.PoolType.None);
        //var ballonShadowManagerFactory = new ObjectManagerFactory3(balloonShadowPrefab, lvlTransform, ObjectManagerFactory3.PoolType.Stack);
        var carManagerFactory = new ObjectManagerFactory3(carPrefab, lvlTransform, ObjectManagerFactory3.PoolType.None);
        
        // Roads
        foreach (var road in levelContents.roads)
        {
            var roadGameObject = Instantiate(roadPrefab, lvlTransform);
            var lowerEdgeY = road * cellHeight;
            
            var roadLeftEdgeX = road * cellHeight * neutralSlope;
            var roadLocalTransform = new Vector3(roadLeftEdgeX, lowerEdgeY, -0.21f);
            roadGameObject.transform.localPosition = roadLocalTransform;            
            roadLowerEdgesY.Add(roadGameObject.transform.position.y);

            var roadWidth = LevelContents.gridWidth * cellWidth;

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

            // Bridge
            var bridgeX = GetRiverLeftEdgeX(lowerEdgeY, riverSectionGameObject.transform.localPosition.x, 0f) + riverWidth / 2;
            bridge bridge = Instantiate(bridgePrefab, lvlTransform);
            var bridgeLocalTransform = new Vector3(bridgeX, lowerEdgeY + (roadHeight / 2), -0.23f);
            bridge.transform.localPosition = bridgeLocalTransform;
            if (levelContents.vipTargets && UnityEngine.Random.Range(0f, 1.0f) < vipProbability)
            {
                bridge.SetVip();
            }
            
            // Car            
            if (UnityEngine.Random.Range(0f, 1.0f) < carProbability)
            {
                ret[road].managedObjects = ret[road].managedObjects.Concat((new int[] {0}).Select(_ => 
                    {
                        //Car car = Instantiate(carPrefab, lvlTransform);
                        //var managedCar = new ManagedObject(carManagerFactory.Pool);
                        var managedCar = carManagerFactory.Pool.Get();
                        managedCar.releaseAction = managedCar.Deactivate;
                        var carLocalTransform = new Vector3(roadLeftEdgeX + carOffsetX, lowerEdgeY + (roadHeight / 2), -0.24f);
                        managedCar.transform.localPosition = carLocalTransform;
                        if (levelContents.vipTargets && UnityEngine.Random.Range(0f, 1.0f) < vipProbability)
                        {
                            InterfaceHelper.GetInterface<IVip>(managedCar.gameObject).SetVip();
                        }
                        return new Action(() => carManagerFactory.Pool.Release(managedCar));
                    })
                );
            }
        }

        // Houses
        foreach (var houseSpec in levelContents.houses)
        {
            ExpHouse house = Instantiate(housePrefab, lvlTransform);
            house.SetSize(houseSpec.width, houseSpec.height, houseSpec.depth);
            var colorIndex = UnityEngine.Random.Range(0, houseColors.Length);
            house.SetColor(houseColors[colorIndex]);
            var houseLocalTransform = new Vector3(houseSpec.position.x * cellWidth + houseSpec.position.y * cellHeight * neutralSlope, houseSpec.position.y * cellHeight, -0.2f);
            house.transform.localPosition = houseLocalTransform;

            if (levelContents.vipTargets && UnityEngine.Random.Range(0f, 1.0f) < vipProbability)
            {
                house.SetVip();
            }
        }        

        // Small items: Flack guns, trees, tanks
        for (var ytmpOuter = 0; ytmpOuter < LevelContents.gridHeight; ytmpOuter++)
        {
            var ytmp = ytmpOuter; //capture for lazy evaluation
            var gameObjectsAtY = Enumerable.Range(leftTrim, LevelContents.gridWidth - rightTrim - leftTrim).SelectMany(xtmp =>
            {
                ObjectManagerFactory3 selectedFactory3 = null;
                switch (levelContents.cells[xtmp, ytmp] & CellContent.LAND_MASK)
                {
                    case CellContent.FLACK_GUN:
                        selectedFactory3 = flakGunManagerFactory;
                        break;

                    case CellContent.TANK:
                        selectedFactory3 = tankManagerFactory;
                        break;

                    case CellContent.TREE1:
                        selectedFactory3 = tree1ManagerFactory;
                        break;

                    case CellContent.TREE2:
                        selectedFactory3 = tree2ManagerFactory;
                        break;

                    case CellContent.BOAT1:
                        selectedFactory3 = boat1ManagerFactory;
                        break;

                    case CellContent.BOAT2:
                        selectedFactory3 = boat2ManagerFactory;
                        break;

                    case CellContent.VEHICLE1:
                        selectedFactory3 = vehicle1ManagerFactory;
                        break;
                    
                    case CellContent.VEHICLE2:
                        selectedFactory3 = vehicle2ManagerFactory;
                        break;

                    case CellContent.ENEMY_HANGAR:
                        selectedFactory3 = enemyHangarManagerFactory;
                        break;

                    case CellContent.HANGAR:
                        //selectedFactory = hangarManagerFactory;
                        break;
                }

                List<Action> ret = new();
                var itemLocalTransform = new Vector3(xtmp * cellWidth + ytmp * cellHeight * neutralSlope, ytmp * cellHeight, -0.24f);
                if (selectedFactory3 != null)
                {
                    var managedObject = selectedFactory3.Pool.Get();
                    managedObject.releaseAction = managedObject.Deactivate;
                    managedObject.gameObject.transform.localPosition = itemLocalTransform;
                    
                    if (levelContents.vipTargets)
                    {
                        var possibleVip = InterfaceHelper.GetInterface<IVip>(managedObject.gameObject);
                        if (possibleVip != null && UnityEngine.Random.Range(0f, 1.0f) < vipProbability)
                        {
                            possibleVip.SetVip();
                        }
                    }

                    ret.Add(() => selectedFactory3.Pool.Release(managedObject));
                }

                /*if ((levelContents.cells[xtmp, ytmp] & CellContent.AIR_MASK) == CellContent.BALLOON)
                {
                    //var balloonShadowGameObject = Instantiate(balloonShadowPrefab, lvlTransform);
                    var managedBalloonShadow = new ManagedObject(ballonShadowManagerFactory.Pool);                    
                    managedBalloonShadow.GameObject.transform.localPosition = itemLocalTransform;

                    var balloonGameObject = Instantiate(balloonPrefab, balloonParent.transform);
                    balloonGameObject.transform.position = managedBalloonShadow.GameObject.transform.position;
                    Balloon balloon = InterfaceHelper.GetInterface<Balloon>(balloonGameObject);
                    balloon.SetShadow(managedBalloonShadow.GameObject);
                    ret.Add(managedBalloonShadow);
                }*/

                return ret;
            });

            ret[ytmp].managedObjects = ret[ytmp].managedObjects.Concat(gameObjectsAtY);
        }

        return ret.ToList();
    }

    void CreateLevel()
    {
        RotateLevels();
        var newGameObjects = PopulateScene(latestLevel)
        .Select(goc => new GameObjectCollection3 {zCoord = goc.zCoord + lastLevelLowerEdgeY, managedObjects = goc.managedObjects})
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
        levelWidth = (levelHeight * LevelContents.gridWidth) / LevelContents.gridHeight;
        level = -1;
        var levelLowerLeftCornerX = 0f;
        var refObjStartOffset = 0.8f;
        var newRefObjPos = new Vector3(levelLowerLeftCornerX + levelWidth / 2 + refObjStartOffset, refObjStartOffset, 0f);
        refobject.transform.position = newRefObjPos;
        gameState = GetGameState();

        if (maxPlane == null)
        {
            maxPlane = Instantiate(maxPlanePrefab, refobject.transform);
            maxPlane.refObject = refobject.transform;            
            AddPlaneShadow(maxPlane.transform);
            gameState.SetPlaneHeights(maxPlane.GetHeight(), Altitudes.enemyPlaneHeight);
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
        roadLowerEdgesY = new();
        newLevelTask = null;        
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
        stateContents.targetsHitMin = GetTargetHitsMin(stateContents.latestLevelPrereq);
        gameState.ReportEvent(GameEvent.START);
    }

    void Start()
    {   
        var camObject = GameObject.Find("Main Camera");
        tvSimDocumentObject = FindAnyObjectByType<TvSimDocument>(FindObjectsInactive.Include);
        maxCamera = InterfaceHelper.GetInterface<MaxCamera>(camObject);

        // visible area marker for debugging
        var vaGameObject = Instantiate(visibleAreaMarkerPrefab, camObject.transform);
        var localPosition = new Vector3(-visibleAreaMarkerWidth/2, -visibleAreaMarkerHeight/2, 1f);
        vaGameObject.transform.localPosition = localPosition;
        
        var vaMeshFilter = vaGameObject.AddComponent<MeshFilter>();
        var vaMeshRenderer = vaGameObject.AddComponent<MeshRenderer>();

        var vaColor = new Color(1f, 1f, 1f, 0.5f);
        vaMeshRenderer.material = visibleAreaMarkerMaterial;
        vaMeshRenderer.material.color = vaColor;

        var vaVerts = new List<Vector2>
        {
            new Vector2(0f, 0f),
            new Vector2(visibleAreaMarkerWidth, 0f),
            new Vector2(0f, visibleAreaMarkerHeight),
            new Vector2(visibleAreaMarkerWidth, visibleAreaMarkerHeight)
        };

        var vaMesh = CreateQuadMesh(vaVerts);
        vaMeshFilter.mesh = vaMesh;
        vaGameObject.SetActive(false);
        
        Settings.Update();

        StartNewGame();
    }

    bool IsOverRoad(Vector2 position)
    {
        foreach (var lowerEdgeY in roadLowerEdgesY)
        {
            if (position.y < (lowerEdgeY + roadHeight))
            {
                return position.y > lowerEdgeY;
            }
        }
        return false;
    }

    bool IsOverLandingStrip(Vector2 position)
    {
        var offsetX = (position.y - refobject.transform.position.y) * riverSlopes[neutralRiverSlopeIndex];
        return position.y > landingStripBottomY && 
            position.y < landingStripTopY &&            
            Math.Abs((refobject.transform.position.x + offsetX - position.x)) < landingStripWidth / 2;
    }

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


    bool IsOverRiver(Vector2 position)
    {
        var xOffset = riverSectionGameObject.transform.position.x;
        var yOffset = riverSectionGameObject.transform.position.y;

        var leftEdgeX = GetRiverLeftEdgeX(position.y, xOffset, yOffset);
        var rightEdgeX = leftEdgeX + LevelBuilder.riverWidth * (levelWidth / LevelContents.gridWidth);

        // compare to position x
        var overRiverSegment =
            position.x > leftEdgeX &&
            position.x < rightEdgeX;
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
            gameState = GameState.GetInstance();
            gameState.Subscribe(GameEvent.GAME_STATUS_CHANGED, OnGameStatusChanged);
            gameState.Subscribe(GameEvent.RESTART_REQUESTED, OnRestartRequested);
            gameState.Subscribe(GameEvent.START, OnGameStart);
            gameState.Subscribe(GameEvent.BIG_DETONATION, OnBigDetonation);
            gameState.Subscribe(GameEvent.VIEW_MODE_CHANGED, OnViewModeChanged);
            gameState.SubscribeToBombLandedEvent(OnBombLandedCallback);
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
        AddPlaneShadow(enemyPlane.transform);
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

        if (refobject.transform.position.y > (lastLevelLowerEdgeY + levelHeight * prepTimeForNextLevelQuotient))
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

        while (pendingActivation.Count > 0 && refobject.transform.position.y + activationDistance > pendingActivation.First().zCoord)
        {
            //Debug.Log($"Time to activate more game objects at {refobject.transform.position.y} {pendingActivation.First().yCoord}");
            var activeCollection = pendingActivation.First();
            // Instantiate game objects, never mind return value
            activeCollection.managedObjects = activeCollection.managedObjects.ToArray();
            pendingActivation.RemoveAt(0);
            activeObjects.Add(activeCollection);
            break;
        }

        while (activeObjects.Count > 0 && refobject.transform.position.y - deactivationDistance > activeObjects.First().zCoord)
        {
            //Debug.Log($"Time to destroy game objects at {refobject.transform.position.y} {activeObjects.First().yCoord}");

            var collection = activeObjects.First();
            foreach (var managedObject in collection.managedObjects)
            {
                managedObject();
            }

            activeObjects.RemoveAt(0);
        }

        while (roadLowerEdgesY.Count > 0 && refobject.transform.position.y - deactivationDistance > roadLowerEdgesY.First())
        {
            roadLowerEdgesY.RemoveAt(0);
        } 

        var distanceDiff = refobject.transform.position.y - lastLevelLowerEdgeY;
        gameState.SetApproachingLanding(
            (distanceDiff > levelHeight * (1-LevelBuilder.finalApproachQuotient)) ||
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
        Vector2 levelVelocity = new(stateContents.speed, stateContents.speed);
        Vector3 delta = levelVelocity * Time.deltaTime;
        refobject.transform.position += delta;
    }

    void OnGameStatusChanged()
    {
        var gameStatus = gameState.GetStateContents().gameStatus;
        Debug.Log($"New State: {gameStatus}");
        if(gameStatus == GameStatus.DEAD ||
           gameStatus == GameStatus.FINISHED)
        {
            gameState.SetSpeed(0f);
            restartCoolDownSeconds = minRestartWaitSeconds;
        }
    }

    void OnRestartRequested()
    {
        if (restartCoolDownSeconds > 0f)
        {
            //Debug.Log("Too early to restart");
            return;
        }

        Debug.Log("Starting a new game");

        StartNewGame();
    }

    void OnGameStart()
    {
        gameState.SetSpeed(0f);
        gameState.SetStatus(GameStatus.REFUELLING);
    }

    void OnBigDetonation()
    {
        if (maxCamera != null)
        {
            maxCamera.OnDetonation();
        }
    }

    void OnViewModeChanged()
    {
        if (maxCamera != null)
        {
            maxCamera.OnViewModeChanged();
            tvSimDocumentObject.OnViewModeChanged();
        }
    }

    void OnBombLandedCallback(BombLandedEventArgs args) =>
        OnBombLandedCallbackInternal(args.bomb, args.hitObject);

    void OnBombLandedCallbackInternal(GameObject bomb, GameObject hitObject) 
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
            var managedObject = InterfaceHelper.GetInterface<ManagedObject3>(hitObject);
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
            Destroy(bomb);
        }
    }
}
