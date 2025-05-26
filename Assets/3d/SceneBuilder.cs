using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

public class SceneBuilder : MonoBehaviour
{
    public float carProbability = 0.5f;
    public float carOffsetX = -5f;
    public float damScale = 3.5f;
    public GameObject riverSectionPrefab;
    public GameObject roadPrefab;
    public FriendlyLandingStrip landingStripPrefab;
    public GameObject enemyLandingStripPrefab;
    public GameObject housePrefab;
    public ManagedObject flackGunPrefab;
    public ManagedObject tankPrefab;
    public ManagedObject tree1Prefab;
    public ManagedObject tree2Prefab;    
    public ManagedObject boat1Prefab;
    public ManagedObject boat2Prefab;
    public ManagedObject vehicle1Prefab;
    public ManagedObject vehicle2Prefab;
    public ManagedObject enemyHangarPrefab;
    public EnemyPlane3d parkedPlanePrefab;
    public GameObject balloonPrefab;
    public GameObject balloonShadowPrefab;
    public GameObject bridgePrefab;
    public ManagedObject carPrefab;
    public ManagedObject searchLightPrefab;
    public GameObject airstripEndPrefab;
    public ManagedObject hangarPrefab;
    public EnemyHQ3d enemyHqPrefab;
    public GameObject bigHousePrefab;
    public BossRobot robotBossPrefab;
    public GameObject redBaronBossPrefab;
    public GameObject introControllerPrefab;
    public Dam damPrefab;
    public GameObject powerLinePrefab;
    public GameObject powerPostPrefab;
    public Material riverMaterial;
    public Material groundMaterial;
    public Material riverBankMaterial;
    public Material roadMaterial;
    public Material landingStripMaterial;
    public int leftTrim = 2;
    public int rightTrim = 5;
    public float roadAltitude = 0.002f;
    public float powerLineAltitude = 2.5f;
    public float carAltitude = 0.05f;
    float airstripAltitude = 0.001f;
    public float parkedPlaneAltitude = 0.15f;
    public float parllelRoadSideWidth = 0.1f;
    public float parallelRoadWidth = 0.9f;
    public float bigHouseSizeX = 6f;
    public float bigHouseSizeY = 10f;
    public float bigHouseSizeZ = 5f;
    public float groundOverlap = 3f;

    private ObjectManager flakGunManager;
    private ObjectManager tankManager;
    private ObjectManager tree1Manager;
    private ObjectManager tree2Manager;
    private ObjectManager boat1Manager;
    private ObjectManager boat2Manager;
    private ObjectManager vehicle1Manager;
    private ObjectManager vehicle2Manager;
    private ObjectManager enemyHangarManager;
    private ObjectManager carManager;
    private ObjectManager searchLightManager;
    private ObjectManager damManager;
    private GameObject managedObjectsParent;

    public void Init()
    {
        if (managedObjectsParent != null)
        {
            Destroy(managedObjectsParent);
        }

        managedObjectsParent = new GameObject("managedObjects");
        flakGunManager = new ObjectManager(flackGunPrefab, managedObjectsParent.transform, ObjectManager.PoolType.Stack);
        tankManager = new ObjectManager(tankPrefab, managedObjectsParent.transform, ObjectManager.PoolType.Stack);
        tree1Manager = new ObjectManager(tree1Prefab, managedObjectsParent.transform, ObjectManager.PoolType.Stack);
        tree2Manager = new ObjectManager(tree2Prefab, managedObjectsParent.transform, ObjectManager.PoolType.Stack);
        boat1Manager = new ObjectManager(boat1Prefab, managedObjectsParent.transform, ObjectManager.PoolType.Stack);
        boat2Manager = new ObjectManager(boat2Prefab, managedObjectsParent.transform, ObjectManager.PoolType.Stack);
        vehicle1Manager = new ObjectManager(vehicle1Prefab, managedObjectsParent.transform, ObjectManager.PoolType.None);
        vehicle2Manager = new ObjectManager(vehicle2Prefab, managedObjectsParent.transform, ObjectManager.PoolType.None);
        enemyHangarManager = new ObjectManager(enemyHangarPrefab, managedObjectsParent.transform, ObjectManager.PoolType.None);
        carManager = new ObjectManager(carPrefab, managedObjectsParent.transform, ObjectManager.PoolType.None);
        searchLightManager = new ObjectManager(searchLightPrefab, managedObjectsParent.transform, ObjectManager.PoolType.None);
        damManager = new ObjectManager(damPrefab, managedObjectsParent.transform, ObjectManager.PoolType.None);
    }


    Mesh CreateQuadMesh(Vector3[] verts, Vector3[] quadNormals)
    {
        if (verts.Length % 4 != 0)
        {
            throw new System.Exception("Length of param must a multiple of 4");
        }

        if (verts.Length != quadNormals.Length * 4 && verts.Length != 0)
        {
            throw new System.Exception("Length of quadNormals must be 1/4 of verts");
        }
        
        var triangles = new List<int>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();
        var quads = verts.Length / 4;
        for (int i = 0; i < quads; i++)
        {
            var triIndexOffset = i * 4;
            triangles.Add(triIndexOffset + 0);
            triangles.Add(triIndexOffset + 2);
            triangles.Add(triIndexOffset + 1);
            triangles.Add(triIndexOffset + 2);
            triangles.Add(triIndexOffset + 3);
            triangles.Add(triIndexOffset + 1);


            normals.Add(quadNormals[i]);
            normals.Add(quadNormals[i]);
            normals.Add(quadNormals[i]);
            normals.Add(quadNormals[i]);

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

    float GetRiverLeftEdgeX(float z, List<SceneRiverSegment> riverSegments)
    {
        var segment = riverSegments.FirstOrDefault(s =>  z < s.maxZ);
        if (segment == null || segment.minZ > z)
        {
            Debug.LogError($"No river segment found for z={z}");
            return 0;
        }
        
        // interpolate river edges x
        var zdiff = z - segment.minZ;
        var xdiff = zdiff * (segment.ulcX - segment.llcX) / (segment.maxZ - segment.minZ);

        return segment.llcX + xdiff;
    }
    
    // Create game objects    
    public SceneOutput PopulateScene(LevelContents levelContents, SceneInput sceneInput)
    {
        GameState gameState = GameState.GetInstance();

        float cellWidth = sceneInput.levelWidth / LevelContents.gridWidth;
        float cellHeight = sceneInput.levelHeight / levelContents.gridHeight;
        var midX = LevelContents.gridWidth / 2;

        SceneOutput ret = new();
        GameObjectCollection4[] gameObjectCollections = new GameObjectCollection4[levelContents.gridHeight];
        for (var ztmp = 0; ztmp < levelContents.gridHeight; ztmp++)
        {
            gameObjectCollections[ztmp] = new GameObjectCollection4 {
                zCoord = ztmp * cellHeight, // level relative coordinate
                objectRefs = new List<ManagedObjectReference>()
            };
        }        

        var parentPositionOffset = sceneInput.levelTransform.position - managedObjectsParent.transform.position;

        if (levelContents.landingStrip)
        {
            var lsWidth = LevelBuilder.landingStripWidth * cellWidth;
            var lsHeight = LevelBuilder.landingStripHeight * cellHeight;

            var friendlyStrip = Instantiate(landingStripPrefab, sceneInput.levelTransform);
            if (levelContents.airstripInfo != null)
            {
                friendlyStrip.airStripInfo = levelContents.airstripInfo;
            }

            // scale
            var lsQuadTransform = friendlyStrip.transform.GetChild(0);
            var lsMeshFilter = lsQuadTransform.gameObject.GetComponent<MeshFilter>();
            var meshSize =  lsMeshFilter.mesh.bounds.size;
            var localScale = friendlyStrip.transform.localScale;

            localScale.x = lsWidth / meshSize.x;
            localScale.z = lsHeight / (meshSize.y * lsQuadTransform.localScale.x); // mesh size y and mesh scale x correspond to world z because of the mesh orientation
            friendlyStrip.transform.localScale = localScale;
            
            // position
            var zOffset = lsHeight / 2;
            var lsLocalPosition = new Vector3((LevelContents.gridWidth / 2) * cellWidth, airstripAltitude, zOffset);
            friendlyStrip.transform.localPosition = lsLocalPosition;

            ret.landingStripStartZ = friendlyStrip.transform.position.z - zOffset;
            ret.landingStripEndZ = ret.landingStripStartZ + lsHeight;
            ret.landingStripWidth = lsWidth;
        }

        // Enemy Airstrips
        foreach(var enemyAirstrip in levelContents.enemyAirstrips)
        {            
            var lsWidth = LevelBuilder.landingStripWidth * cellWidth;
            var lsHeight = LevelBuilder.enemyAirstripHeight * cellHeight;

            var lsGameObject = Instantiate(enemyLandingStripPrefab, sceneInput.levelTransform);

            // scale
            var lsQuadTransform = lsGameObject.transform.GetChild(0);
            //var lsMeshFilter = lsGameObject.GetComponentInChildren<MeshFilter>();
            var lsMeshFilter = lsQuadTransform.gameObject.GetComponent<MeshFilter>();
            var meshSize =  lsMeshFilter.mesh.bounds.size;
            var localScale = lsGameObject.transform.localScale;
            localScale.x = lsWidth / (meshSize.x * lsQuadTransform.localScale.x);
            localScale.z = lsHeight / (meshSize.z * lsQuadTransform.localScale.z);
            lsGameObject.transform.localScale = localScale;

            // position
            var lsLocalPosition = new Vector3(
                ((LevelContents.gridWidth / 2) - LevelBuilder.enemyAirstripXDistance) * cellWidth + lsWidth / 2,
                airstripAltitude,
                enemyAirstrip * cellHeight + lsHeight / 2);
            lsGameObject.transform.localPosition = lsLocalPosition;         
            
            // parked planes
            var nofParkedPlanes = UnityEngine.Random.Range(1, 3);
            for (int i = 0; i < nofParkedPlanes; i++)
            {
                EnemyPlane3d parkedPlane = Instantiate(parkedPlanePrefab, lsGameObject.transform);
                parkedPlane.transform.Rotate(-25, 45, 0);
                var parkedPlaneZ = (i+1) * lsHeight / (nofParkedPlanes+1) - lsHeight / 2;
                var ppLocalPosition = new Vector3(0, parkedPlaneAltitude, parkedPlaneZ);
                parkedPlane.transform.localPosition = ppLocalPosition;
                parkedPlane.SetVip();
                parkedPlane.SetSpeed(0);
            }
        }
        
        if (levelContents.city != null)
        {
            var cityWidth = LevelContents.gridWidth * cellWidth;
            var cityHeight = (levelContents.city.yEnd - levelContents.city.yStart) * cellHeight;

            var cityGameObject = new GameObject("city");
            cityGameObject.transform.parent = sceneInput.levelTransform;

            var cityOffsetZ = levelContents.city.yStart * cellHeight;
            var cityLocalTransform = new Vector3(0, roadAltitude, cityOffsetZ);
            cityGameObject.transform.localPosition = cityLocalTransform;

            var cityllcX = 0;
            var citylrcX = cityWidth;
            var cityulcX = 0;
            var cityurcX = cityWidth;
            var cityllcZ = 0;
            var citylrcZ = 0;
            var cityulcZ = cityHeight;
            var cityurcZ = cityHeight;

            var cityMeshFilter = cityGameObject.AddComponent<MeshFilter>();
            var cityMeshRenderer = cityGameObject.AddComponent<MeshRenderer>();
            cityMeshRenderer.material = landingStripMaterial;

            var cityVerts = new Vector3[]
            {
                new Vector3(cityllcX, 0, cityllcZ),
                new Vector3(citylrcX, 0, citylrcZ),
                new Vector3(cityulcX, 0, cityulcZ),
                new Vector3(cityurcX, 0, cityurcZ)
            };

            var cityUpNormalsArray = new Vector3[]
            {
                Vector3.up,
            };

            var cityMesh = CreateQuadMesh(cityVerts, cityUpNormalsArray);
            cityMeshFilter.mesh = cityMesh;

            ret.enemyHQs = levelContents.city.enemyHQs.Select(hq =>
            {
                EnemyHQ3d hqInstance = Instantiate(enemyHqPrefab, sceneInput.levelTransform);
                if (hq.bombed)
                {
                    hqInstance.SetBombed();
                }
                var targetOffsetZ = hq.y * cellHeight;
                var targetLocalTransform = new Vector3((LevelContents.gridWidth / 2) * cellWidth, roadAltitude, targetOffsetZ);
                hqInstance.transform.localPosition = targetLocalTransform;
                return hqInstance as IEnemyHQ;
            }).ToList();

            // Big houses
            foreach (var bigHouse in levelContents.city.bigHouses)
            {
                var bigHouseGameObject = Instantiate(housePrefab, sceneInput.levelTransform);
                var house = InterfaceHelper.GetInterface<House4>(bigHouseGameObject);
                var bigHouseOffsetZ = bigHouse.y * cellHeight;
                var bigHouseXPosRel = bigHouse.x * cellWidth;
                var bigHouseLocalTransform = new Vector3(bigHouseXPosRel, 2 * roadAltitude, bigHouseOffsetZ);
                bigHouseGameObject.transform.localPosition = bigHouseLocalTransform;
                house.SetAppearance(new Vector3(bigHouseSizeX, bigHouseSizeY, bigHouseSizeZ), false);
            }
        }

        // River
        ret.riverSectionGameObject = new GameObject("riversection");
        ret.riverSectionGameObject.transform.parent = sceneInput.levelTransform;
        ret.riverSectionGameObject.transform.localPosition = new Vector3(0f, gameState.riverAltitude, 0f);

        var groundLeftOfRiver = new GameObject("ground left");
        groundLeftOfRiver.transform.parent = sceneInput.levelTransform;
        groundLeftOfRiver.transform.localPosition = new Vector3(0f, 0f, 0f);
        
        var groundRightOfRiver = new GameObject("ground right");
        groundRightOfRiver.transform.parent = sceneInput.levelTransform;
        groundRightOfRiver.transform.localPosition = new Vector3(0f, 0f, 0f);

        var riverLeftBank = new GameObject("riverbank");
        riverLeftBank.transform.parent = sceneInput.levelTransform;
        var riverLeftBankLocalTransform = new Vector3(0f, 0f, 0f);
        riverLeftBank.transform.localPosition = riverLeftBankLocalTransform;

        var riverRightBank = new GameObject("riverbank");
        riverRightBank.transform.parent = sceneInput.levelTransform;
        var riverRightBankLocalTransform = new Vector3(0f, 0f, 0f);
        riverRightBank.transform.localPosition = riverLeftBankLocalTransform;

        // River MeshRenderers
        var rsMeshFilter = ret.riverSectionGameObject.AddComponent<MeshFilter>();
        var rsMeshRenderer = ret.riverSectionGameObject.AddComponent<MeshRenderer>();
        rsMeshRenderer.material = riverMaterial;

        var glMeshFilter = groundLeftOfRiver.AddComponent<MeshFilter>();
        var glMeshRenderer = groundLeftOfRiver.AddComponent<MeshRenderer>();
        glMeshRenderer.material = groundMaterial;

        var grMeshFilter = groundRightOfRiver.AddComponent<MeshFilter>();
        var grMeshRenderer = groundRightOfRiver.AddComponent<MeshRenderer>();
        grMeshRenderer.material = groundMaterial;

        var lbMeshFilter = riverLeftBank.AddComponent<MeshFilter>();
        var lbMeshRenderer = riverLeftBank.AddComponent<MeshRenderer>();
        lbMeshRenderer.material = landingStripMaterial;

        var rbMeshFilter = riverRightBank.AddComponent<MeshFilter>();
        var rbMeshRenderer = riverRightBank.AddComponent<MeshRenderer>();
        rbMeshRenderer.material = landingStripMaterial;

        // River Meshes
        var z = 0f;        
        float riverLowerLeftCornerX = levelContents.riverLowerLeftCornerX * cellWidth;
        var riverWidth = LevelBuilder.riverWidth * cellWidth;

        List<Vector3> riverVerts = new();
        List<Vector3> groundLeftOfRiverVerts = new();
        List<Vector3> groundRightOfRiverVerts = new();
        List<Vector3> riverLeftBankVerts = new();
        List<Vector3> riverRightBankVerts = new();
        List<Vector3> UpNormals = new();
        List<Vector3> riverLeftBankNormals = new();
        List<Vector3> riverRightBankNormals = new();
        List<SceneRiverSegment> riverSegments = new();

        var riverZOffset = ret.riverSectionGameObject.transform.position.z;
        var riverXOffset = ret.riverSectionGameObject.transform.position.x;

        foreach (var segment in levelContents.riverSegments)
        {
            var segmentHeight = segment.height * cellHeight;
            var xOffset = segment.slope * segmentHeight;

            var riverUpperLeftCornerX = riverLowerLeftCornerX + xOffset;
            var riverLowerRightCornerX = riverLowerLeftCornerX + riverWidth;
            var riverUpperRightCornerX = riverUpperLeftCornerX + riverWidth;
            var upperZ = z + segmentHeight;
            
            riverVerts.Add(new Vector3(riverLowerLeftCornerX, 0f, z));
            riverVerts.Add(new Vector3(riverLowerRightCornerX, 0f, z));
            riverVerts.Add(new Vector3(riverUpperLeftCornerX, 0f, upperZ));
            riverVerts.Add(new Vector3(riverUpperRightCornerX, 0f, upperZ));
            riverSegments.Add(new SceneRiverSegment
            {
                minZ = z + riverZOffset,
                maxZ = upperZ + riverZOffset,
                llcX = riverLowerLeftCornerX + riverXOffset,
                lrcX = riverLowerRightCornerX + riverXOffset,
                ulcX = riverUpperLeftCornerX + riverXOffset,
                urcX = riverUpperRightCornerX + riverXOffset
            });

            groundLeftOfRiverVerts.Add(new Vector3(0f, 0f, z));
            groundLeftOfRiverVerts.Add(new Vector3(riverLowerLeftCornerX, 0f, z));
            groundLeftOfRiverVerts.Add(new Vector3(0f, 0f, upperZ));
            groundLeftOfRiverVerts.Add(new Vector3(riverUpperLeftCornerX, 0f, upperZ));

            groundRightOfRiverVerts.Add(new Vector3(riverLowerRightCornerX, 0f, z));
            groundRightOfRiverVerts.Add(new Vector3(sceneInput.levelWidth, 0f, z));
            groundRightOfRiverVerts.Add(new Vector3(riverUpperRightCornerX, 0f, upperZ));
            groundRightOfRiverVerts.Add(new Vector3(sceneInput.levelWidth, 0f, upperZ));

            riverLeftBankVerts.Add(new Vector3(riverLowerLeftCornerX, 0f, z));
            riverLeftBankVerts.Add(new Vector3(riverLowerLeftCornerX, gameState.riverAltitude, z));
            riverLeftBankVerts.Add(new Vector3(riverUpperLeftCornerX, 0f, upperZ));            
            riverLeftBankVerts.Add(new Vector3(riverUpperLeftCornerX, gameState.riverAltitude, upperZ));

            riverRightBankVerts.Add(new Vector3(riverUpperRightCornerX, 0f, upperZ));            
            riverRightBankVerts.Add(new Vector3(riverUpperRightCornerX, gameState.riverAltitude, upperZ));
            riverRightBankVerts.Add(new Vector3(riverLowerRightCornerX, 0f, z));
            riverRightBankVerts.Add(new Vector3(riverLowerRightCornerX, gameState.riverAltitude, z));

            UpNormals.Add(Vector3.up);
            var riverLeftBankNormal = new Vector3(segmentHeight, 0f, xOffset).normalized;
            riverLeftBankNormals.Add(riverLeftBankNormal);
            var riverRightBankNormal = -riverLeftBankNormal;
            riverRightBankNormals.Add(riverRightBankNormal);

            z += segmentHeight;
            riverLowerLeftCornerX += xOffset;
        }

        if (riverVerts.Count == 0)
        {
            // Add a ground mesh even if there is no river
            groundRightOfRiverVerts.Add(new Vector3(0f, 0f, 0f));
            groundRightOfRiverVerts.Add(new Vector3(sceneInput.levelWidth, 0f, 0f));
            groundRightOfRiverVerts.Add(new Vector3(0f, 0f, sceneInput.levelHeight));
            groundRightOfRiverVerts.Add(new Vector3(sceneInput.levelWidth, 0f, sceneInput.levelHeight));
            UpNormals.Add(Vector3.up);
        }

        var overlapVector = new Vector3(0f,
             -0.001f, // tiny altitude offset to avoid z-fighting
             groundOverlap);
        for(int i = 0; i < 2; i++)
        {
            if (groundLeftOfRiverVerts.Count > i)
            {
                groundLeftOfRiverVerts[i] -= overlapVector;
                groundLeftOfRiverVerts[groundLeftOfRiverVerts.Count - 1 - i] += overlapVector;
            }

            if (groundRightOfRiverVerts.Count > i)
            {
                groundRightOfRiverVerts[i] -= overlapVector;
                groundRightOfRiverVerts[groundRightOfRiverVerts.Count - 1 - i] += overlapVector;
            }
        }

        ret.riverVerts = riverVerts;
        ret.riverSegments = riverSegments;
        
        var upNormalsArray = UpNormals.ToArray();
        rsMeshFilter.mesh = CreateQuadMesh(ret.riverVerts.ToArray(), upNormalsArray);
        glMeshFilter.mesh = CreateQuadMesh(groundLeftOfRiverVerts.ToArray(), upNormalsArray);
        grMeshFilter.mesh = CreateQuadMesh(groundRightOfRiverVerts.ToArray(), upNormalsArray);
        lbMeshFilter.mesh = CreateQuadMesh(riverLeftBankVerts.ToArray(), riverLeftBankNormals.ToArray());
        rbMeshFilter.mesh = CreateQuadMesh(riverRightBankVerts.ToArray(), riverRightBankNormals.ToArray());

        // Add mesh colliders
        riverLeftBank.AddComponent<MeshCollider>();
        riverRightBank.AddComponent<MeshCollider>();

        // Parallel Road
        GameObject paraRoad = new GameObject("parallel road");
        GameObject paraRoadWide = new GameObject("parallel road wide");
        paraRoad.transform.parent = sceneInput.levelTransform;
        paraRoadWide.transform.parent = sceneInput.levelTransform;
        var prLocalTransform = new Vector3(levelContents.roadLowerLeftCornerX * cellWidth, airstripAltitude, 0f);
        paraRoadWide.transform.localPosition = prLocalTransform;
        prLocalTransform.y += airstripAltitude;
        paraRoad.transform.localPosition = prLocalTransform;

        // MeshRenderer
        var prMeshFilter = paraRoad.AddComponent<MeshFilter>();
        var prMeshFilterWide = paraRoadWide.AddComponent<MeshFilter>();
        var prMeshRenderer = paraRoad.AddComponent<MeshRenderer>();
        var prMeshRendererWide = paraRoadWide.AddComponent<MeshRenderer>();
        prMeshRenderer.material = landingStripMaterial;
        prMeshRendererWide.material = riverBankMaterial;

        // Mesh
        z = 0f;
        float prLowerLeftCornerX = 0f;

        List<Vector3> paraRoadSegmentNormals = new();

        var paraRoadVerts = levelContents.roadSegments.SelectMany(segment => 
        {
            var segmentHeight = segment.height * cellHeight;
            var xOffset = segment.slope * segment.height * cellHeight;
            
            var ret = new List<Vector3>
            {
                new Vector3(prLowerLeftCornerX, 0f, z),
                new Vector3(prLowerLeftCornerX + parallelRoadWidth, 0f, z),
                new Vector3(prLowerLeftCornerX + xOffset, 0f, z + segmentHeight),
                new Vector3(prLowerLeftCornerX + parallelRoadWidth + xOffset, 0f, z + segmentHeight)
            };
            paraRoadSegmentNormals.Add(Vector3.up);
            z += segmentHeight;
            prLowerLeftCornerX += xOffset;
            return ret;
        }).ToArray();

        var paraRoadWideVerts = paraRoadVerts.Select((vert, index) => {
            var x = index % 2 == 0 ? vert.x - parllelRoadSideWidth : vert.x + parllelRoadSideWidth;
            return new Vector3(x, vert.y, vert.z);
        }).ToArray();

        var paraRoadSegmentNormalsArray = paraRoadSegmentNormals.ToArray();
        var prMesh = CreateQuadMesh(paraRoadVerts, paraRoadSegmentNormalsArray);
        var prMeshWide = CreateQuadMesh(paraRoadWideVerts, paraRoadSegmentNormalsArray);
        prMeshFilter.mesh = prMesh;
        prMeshFilterWide.mesh = prMeshWide;
        
        
        // Roads
        ret.roadNearEdgesZ = new();
        foreach (var road in levelContents.roads)
        {
            if (levelContents.dams?.Count() > 0)
            {
                // Power lines instead of roads

                var powerLineSegmentLength = 5f; // check mesh bounds instead?
                var powerPostHeight = 2.5f; // check mesh bounds instead?
                for (float x = 0; x < sceneInput.levelWidth; x += powerLineSegmentLength)
                {
                    var powerLineGameObject = Instantiate(powerLinePrefab, sceneInput.levelTransform);
                    powerLineGameObject.transform.localPosition = new Vector3(x + (powerLineSegmentLength / 2), powerLineAltitude, road * cellHeight);
                    var powerPostGameObject = Instantiate(powerPostPrefab, sceneInput.levelTransform);
                    powerPostGameObject.transform.localPosition = new Vector3(x, powerLineAltitude - (powerPostHeight / 2), road * cellHeight);
                }
                continue;
            }
            
            //var roadGameObject = Instantiate(roadPrefab, lvlTransform);
            var roadGameObject = new GameObject("road");
            roadGameObject.transform.parent = sceneInput.levelTransform;
            var lowerEdgeZ = road * cellHeight;
            roadGameObject.transform.localPosition = new Vector3(0f, roadAltitude, lowerEdgeZ);
            
            ret.roadNearEdgesZ.Add(roadGameObject.transform.position.z);

            var roadWidth = LevelContents.gridWidth * cellWidth;

            var meshFilter = roadGameObject.AddComponent<MeshFilter>();
            var meshRenderer = roadGameObject.AddComponent<MeshRenderer>();
            meshRenderer.material = roadMaterial;

            var roadVerts = new Vector3[]
            {
                new Vector3(0, 0, 0),
                new Vector3(roadWidth, 0, 0),
                new Vector3(0, 0, sceneInput.roadHeight),
                new Vector3(roadWidth, 0, sceneInput.roadHeight)
            };

            var roadMesh = CreateQuadMesh(roadVerts, new Vector3[] {Vector3.up});
            meshFilter.mesh = roadMesh;

            
            // Bridge
            var bridgeZ = roadGameObject.transform.position.z + (sceneInput.roadHeight / 2);
            var bridgeX = GetRiverLeftEdgeX(bridgeZ, ret.riverSegments) + riverWidth / 2;
            var bridgePosition = new Vector3(bridgeX, roadAltitude, bridgeZ);

            var bridgeGameObject = Instantiate(bridgePrefab, bridgePosition, Quaternion.identity, sceneInput.levelTransform);            

            // scale
            var roadSectionQuadTransform = bridgeGameObject.transform.GetChild(0);
            //var lsMeshFilter = lsGameObject.GetComponentInChildren<MeshFilter>();
            var roadSectionMeshFilter = roadSectionQuadTransform.gameObject.GetComponent<MeshFilter>();
            var meshSize = roadSectionMeshFilter.mesh.bounds.size;
            var localScale = bridgeGameObject.transform.localScale;
            localScale.x = sceneInput.roadHeight / (meshSize.y * roadSectionQuadTransform.localScale.x); // mesh y is bridge width (along river) because of the mesh orientation
            localScale.y = localScale.x;
            localScale.z = localScale.x;
            bridgeGameObject.transform.localScale = localScale;

            if (levelContents.vipTargets)
            {
                var vip = InterfaceHelper.GetInterface<IVip>(bridgeGameObject);
                if (vip != null && UnityEngine.Random.Range(0f, 1.0f) < sceneInput.vipProbability)
                {
                    vip.SetVip();
                }
            }            

            // Car            
            if (UnityEngine.Random.Range(0f, 1.0f) < carProbability)
            {
                gameObjectCollections[road].objectRefs = gameObjectCollections[road].objectRefs.Concat((new GameObject[] {null}).Select(_ => 
                    {
                        var carRef = carManager.Get();
                        var carLocalTransform = new Vector3(carOffsetX, carAltitude,lowerEdgeZ + (sceneInput.roadHeight / 2));
                        carRef.managedObject.transform.localPosition = carLocalTransform + parentPositionOffset;
                        if (levelContents.vipTargets && UnityEngine.Random.Range(0f, 1.0f) < sceneInput.vipProbability)
                        {
                            InterfaceHelper.GetInterface<IVip>(carRef.managedObject.gameObject)?.SetVip();
                        }
                        return carRef;
                    })
                );
            }
        }

        foreach (var dam in levelContents.dams)
        {
            var damZ = sceneInput.levelTransform.position.z + dam * cellHeight;
            var damX = GetRiverLeftEdgeX(damZ, ret.riverSegments) + riverWidth / 2;
            var damPosition = new Vector3(damX,
                gameState.riverAltitude - 0.01f, // tiny altitude offset to avoid z-fighting
                damZ);

            var damGameObject = Instantiate(damPrefab, sceneInput.levelTransform);
            damGameObject.transform.position = damPosition;

            foreach (var moveableObject in damGameObject.GetMoveableObjects())
            {
                // Set x position to match river edges
                var moveableObjectZ = moveableObject.transform.position.z;
                var moveableObjectX = GetRiverLeftEdgeX(moveableObjectZ, ret.riverSegments) + riverWidth / 2;
                moveableObject.transform.position = new Vector3(moveableObjectX, moveableObject.transform.position.y, moveableObjectZ);

                // Rescale x to at least cover river width
                moveableObject.transform.localScale = new Vector3(
                    damScale * moveableObject.transform.localScale.x,
                    moveableObject.transform.localScale.y,
                    moveableObject.transform.localScale.z);
            }
        }
        
        // Houses
        foreach (var houseSpec in levelContents.houses)
        {
            var houseGameObject = Instantiate(housePrefab, sceneInput.levelTransform);
            var house = InterfaceHelper.GetInterface<House4>(houseGameObject);

            /*
            var colorIndex = UnityEngine.Random.Range(0, houseColors.Length);
            house.SetColor(houseColors[colorIndex]);*/
            var houseOffsetY = 0.0f; //TEMP
            var houseLocalPosition = new Vector3(houseSpec.position.x * cellWidth, houseOffsetY, houseSpec.position.y * cellHeight);
            houseGameObject.transform.localPosition = houseLocalPosition;

            house.SetAppearance(new Vector3(houseSpec.width, houseSpec.height, houseSpec.depth), true);

            if (levelContents.vipTargets && UnityEngine.Random.Range(0f, 1.0f) < sceneInput.vipProbability)
            {
                house.SetVip();
            }
        }

        // Boss
        if (levelContents.bossType == BossType.ROBOT)
        {
            var bossOffsetZ = LevelContents.bossY * cellHeight;
            var bossPosition = sceneInput.referenceObjectTransform.position + new Vector3(0f, 0f, bossOffsetZ);
            var bossGameObject = Instantiate(robotBossPrefab, bossPosition, Quaternion.identity, sceneInput.referenceObjectTransform);
            bossGameObject.targetObject = sceneInput.playerPlaneObject;
            ret.boss = bossGameObject.gameObject;
        }
        else if (levelContents.bossType == BossType.RED_BARON)
        {
            var bossPosition = sceneInput.referenceObjectTransform.position + new Vector3(-10f, 20f, 0f);
            var bossGameObject = Instantiate(redBaronBossPrefab, bossPosition, Quaternion.identity, sceneInput.referenceObjectTransform);
            ret.boss = bossGameObject;
        }
        else if (levelContents.bossType == BossType.INTRO_CONTROLLER)
        {
            var bossOffsetZ = LevelContents.bossY * cellHeight;
            var bossPosition = sceneInput.referenceObjectTransform.position + new Vector3(0f, 0f, bossOffsetZ);
            var bossGameObject = Instantiate(introControllerPrefab, bossPosition, Quaternion.identity, sceneInput.referenceObjectTransform);
            ret.boss = bossGameObject;
        }

        // Small items: Flack guns, trees, tanks
        for (var ztmpOuter = 0; ztmpOuter < levelContents.gridHeight; ztmpOuter++)
        {
            var ztmp = ztmpOuter; //capture for lazy evaluation
            var gameObjectsAtZ = Enumerable.Range(leftTrim, LevelContents.gridWidth - rightTrim - leftTrim).SelectMany(xtmp =>
            {
                ObjectManager selectedManager = null;
                var altitude = 2 * roadAltitude;
                switch (levelContents.cells[xtmp, ztmp] & CellContent.LAND_MASK)
                {
                    case CellContent.FLACK_GUN:
                        selectedManager = flakGunManager;
                        break;

                    case CellContent.TANK:
                        selectedManager = tankManager;
                        break;

                    case CellContent.TREE1:
                        selectedManager = tree1Manager;
                        break;

                    case CellContent.TREE2:
                        selectedManager = tree2Manager;
                        break;

                    case CellContent.BOAT1:
                        selectedManager = boat1Manager;
                        altitude = gameState.riverAltitude;
                        break;

                    case CellContent.BOAT2:
                        selectedManager = boat2Manager;
                        altitude = gameState.riverAltitude;
                        break;

                    case CellContent.VEHICLE1:
                        selectedManager = vehicle1Manager;
                        break;

                    case CellContent.VEHICLE2:
                        selectedManager = vehicle2Manager;
                        break;

                    case CellContent.ENEMY_HANGAR:
                        selectedManager = enemyHangarManager;
                        break;

                    case CellContent.SEARCH_LIGHT:
                        selectedManager = searchLightManager;
                        altitude = gameState.searchLightAltitude;
                        break;
                    case CellContent.DAM:
                        selectedManager = damManager;
                        //altitude = gameState.riverAltitude;
                        break;
                }

                var itemLocalTransform = new Vector3(xtmp * cellWidth, altitude, ztmp * cellHeight);

                List<ManagedObjectReference> retInner = new();
                if (selectedManager != null)
                {
                    var objectRef = selectedManager.Get();
                    objectRef.managedObject.transform.localPosition = itemLocalTransform + parentPositionOffset;
                    
                    if (levelContents.vipTargets)
                    {
                        var possibleVip = InterfaceHelper.GetInterface<IVip>(objectRef.managedObject.gameObject);
                        if (possibleVip != null && UnityEngine.Random.Range(0f, 1.0f) < sceneInput.vipProbability)
                        {
                            possibleVip.SetVip();
                        }
                    }

                    retInner.Add(objectRef);
                }

                /*if ((levelContents.cells[xtmp, ztmp] & CellContent.AIR_MASK) == CellContent.BALLOON)
                {
                    //var balloonShadowGameObject = Instantiate(balloonShadowPrefab, sceneInput.levelTransform);
                    var managedBalloonShadow = new ManagedObject(ballonShadowManagerFactory.Pool);
                    managedBalloonShadow.GameObject.transform.localPosition = itemLocalTransform;

                    var balloonGameObject = Instantiate(balloonPrefab, sceneInput.balloonParentTransform);
                    balloonGameObject.transform.position = managedBalloonShadow.GameObject.transform.position;
                    Balloon balloon = InterfaceHelper.GetInterface<Balloon>(balloonGameObject);
                    balloon.SetShadow(managedBalloonShadow.GameObject);
                    retInner.Add(managedBalloonShadow);
                }*/

                return retInner;
            });

            gameObjectCollections[ztmp].objectRefs = gameObjectCollections[ztmp].objectRefs.Concat(gameObjectsAtZ);
        }

        ret.gameObjects = gameObjectCollections.ToList();
        return ret;
    }
    
}