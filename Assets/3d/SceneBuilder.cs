using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SceneBuilder : MonoBehaviour
{
    public float carProbability = 0.5f;
    public float carOffsetX = -5f;
    public GameObject riverSectionPrefab;
    public GameObject roadPrefab;
    public GameObject landingStripPrefab;
    public GameObject enemyLandingStripPrefab;
    public GameObject housePrefab;
    public GameObject flackGunPrefab;
    public GameObject tankPrefab;
    public GameObject tree1Prefab;
    public GameObject tree2Prefab;    
    public GameObject boat1Prefab;
    public GameObject boat2Prefab;
    public GameObject vehicle1Prefab;
    public GameObject vehicle2Prefab;
    public GameObject enemyHangarPrefab;
    public GameObject parkedPlanePrefab;
    public GameObject balloonPrefab;
    public GameObject balloonShadowPrefab;
    public bridge bridgePrefab;
    public GameObject carPrefab;
    public GameObject airstripEndPrefab;
    public GameObject hangarPrefab;
    public EnemyHQ enemyHqPrefab;
    public GameObject bigHousePrefab;
    public Material riverMaterial;
    public Material groundMaterial;
    public Material riverBankMaterial;
    public Material roadMaterial;
    public Material landingStripMaterial;
    public int leftTrim = 2;
    public int rightTrim = 5;
    public float roadAltitude = 0.01f;
    public float carAltitude = 0.05f;
    float airstripAltitude = 0.001f;
    public float parllelRoadSideWidth = 0.1f;
    public float parallelRoadWidth = 0.9f;

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

    //public List<GameObjectCollection> PopulateScene(LevelContents levelContents) => new(); //TEMP!!!

    
    // Create game objects    
    public SceneOutput PopulateScene(LevelContents levelContents, SceneInput sceneInput)
    {
        GameState gameState = FindAnyObjectByType<GameState>();

        float cellWidth = sceneInput.levelWidth / LevelContents.gridWidth;
        float cellHeight = sceneInput.levelHeight / LevelContents.gridHeight;
        var midX = LevelContents.gridWidth / 2;

        SceneOutput ret = new();
        GameObjectCollection[] gameObjectCollections = new GameObjectCollection[LevelContents.gridHeight];
        for (var ztmp = 0; ztmp < LevelContents.gridHeight; ztmp++)
        {
            gameObjectCollections[ztmp] = new GameObjectCollection {
                zCoord = ztmp * cellHeight, // level relative coordinate
                gameObjects = new List<GameObject>()
            };
        }        

        // Landing Strip
        {
            var lsWidth = LevelBuilder.landingStripWidth * cellWidth;
            var lsHeight = LevelBuilder.landingStripHeight * cellHeight;

            var lsGameObject = Instantiate(landingStripPrefab, sceneInput.levelTransform);

            // scale
            var lsQuadTransform = lsGameObject.transform.GetChild(0);
            //var lsMeshFilter = lsGameObject.GetComponentInChildren<MeshFilter>();
            var lsMeshFilter = lsQuadTransform.gameObject.GetComponent<MeshFilter>();
            var meshSize =  lsMeshFilter.mesh.bounds.size;
            var localScale = lsGameObject.transform.localScale;
            localScale.x = lsWidth / (meshSize.x * lsQuadTransform.localScale.x);
            localScale.z = lsHeight / (meshSize.y * lsQuadTransform.localScale.y); // mesh y corresponds to world z because of the mesh orientation
            lsGameObject.transform.localScale = localScale;
            
            // position
            var zOffset = lsHeight / 2;
            var lsLocalPosition = new Vector3((LevelContents.gridWidth / 2) * cellWidth, airstripAltitude, zOffset);
            lsGameObject.transform.localPosition = lsLocalPosition;

            ret.landingStripStartZ = lsGameObject.transform.position.z - zOffset;
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

            /*
            // parked planes
            var nofParkedPlanes = UnityEngine.Random.Range(1, 4);
            for (int i = 0; i < nofParkedPlanes; i++)
            {
                var parkedPlane = Instantiate(parkedPlanePrefab, lsGameObject.transform);
                var parkedPlaneY = lsllcY + (i+1) * lsHeight / (nofParkedPlanes+1);
                var parkedPlaneX = parkedPlaneY * neutralSlope + lsWidth / 2;
                var ppLocalTransform = new Vector3(parkedPlaneX, parkedPlaneY, -0.01f);
                parkedPlane.transform.localPosition = ppLocalTransform;
            }*/
        }

        /*
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
        }*/


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

        ret.riverVerts = riverVerts;
        ret.riverSegments = riverSegments;
        
        var upNormalsArray = UpNormals.ToArray();
        rsMeshFilter.mesh = CreateQuadMesh(ret.riverVerts.ToArray(), upNormalsArray);
        glMeshFilter.mesh = CreateQuadMesh(groundLeftOfRiverVerts.ToArray(), upNormalsArray);
        grMeshFilter.mesh = CreateQuadMesh(groundRightOfRiverVerts.ToArray(), upNormalsArray);
        lbMeshFilter.mesh = CreateQuadMesh(riverLeftBankVerts.ToArray(), riverLeftBankNormals.ToArray());
        rbMeshFilter.mesh = CreateQuadMesh(riverRightBankVerts.ToArray(), riverRightBankNormals.ToArray());

        // Add mesh colliders
        ret.riverSectionGameObject.AddComponent<MeshCollider>();
        groundLeftOfRiver.AddComponent<MeshCollider>();
        groundRightOfRiver.AddComponent<MeshCollider>();
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

            /*
            // Bridge
            var bridgeX = GetRiverLeftEdgeX(lowerEdgeZ, riverSectionGameObject.transform.localPosition.x, 0f) + riverWidth / 2;
            bridge bridge = Instantiate(bridgePrefab, lvlTransform);
            var bridgeLocalTransform = new Vector3(bridgeX, lowerEdgeZ + (roadHeight / 2), -0.23f);
            bridge.transform.localPosition = bridgeLocalTransform;
            if (levelContents.vipTargets && UnityEngine.Random.Range(0f, 1.0f) < vipProbability)
            {
                bridge.SetVip();
            }
            */

            // Car            
            if (UnityEngine.Random.Range(0f, 1.0f) < carProbability)
            {
                gameObjectCollections[road].gameObjects = gameObjectCollections[road].gameObjects.Concat((new GameObject[] {null}).Select(_ => 
                    {
                        GameObject car = Instantiate(carPrefab, sceneInput.levelTransform);
                        var carLocalTransform = new Vector3(carOffsetX, carAltitude,lowerEdgeZ + (sceneInput.roadHeight / 2));
                        car.transform.localPosition = carLocalTransform;
                        /*if (levelContents.vipTargets && UnityEngine.Random.Range(0f, 1.0f) < sceneInput.vipProbability)
                        {
                            car.SetVip();
                        }*/
                        return car.gameObject;
                    })
                );
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

            house.SetSize(new Vector3(houseSpec.width, houseSpec.height, houseSpec.depth));

            if (levelContents.vipTargets && UnityEngine.Random.Range(0f, 1.0f) < sceneInput.vipProbability)
            {
                house.SetVip();
            }
        }
        

        // Small items: Flack guns, trees, tanks
        for (var ztmpOuter = 0; ztmpOuter < LevelContents.gridHeight; ztmpOuter++)
        {
            var ztmp = ztmpOuter; //capture for lazy evaluation
            var gameObjectsAtZ = Enumerable.Range(leftTrim, LevelContents.gridWidth - rightTrim - leftTrim).SelectMany(xtmp =>
            {
                GameObject selectedPrefab = null;
                var altitude = 0f;
                switch (levelContents.cells[xtmp, ztmp] & CellContent.LAND_MASK)
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

                    case CellContent.BOAT1:
                        selectedPrefab = boat1Prefab;
                        altitude = gameState.riverAltitude;
                        break;

                    case CellContent.BOAT2:
                        selectedPrefab = boat2Prefab;
                        altitude = gameState.riverAltitude;
                        break;

                    case CellContent.VEHICLE1:
                        selectedPrefab = vehicle1Prefab;
                        break;
                    
                    case CellContent.VEHICLE2:
                        selectedPrefab = vehicle2Prefab;
                        break;

                    case CellContent.ENEMY_HANGAR:
                        selectedPrefab = enemyHangarPrefab;
                        break;

                    case CellContent.HANGAR:
                        selectedPrefab = hangarPrefab;
                        break;
                }

                var itemLocalTransform = new Vector3(xtmp * cellWidth, altitude, ztmp * cellHeight);

                List<GameObject> retInner = new();
                if (selectedPrefab != null)
                {
                    var itemGameObject = Instantiate(selectedPrefab, sceneInput.levelTransform);
                    itemGameObject.transform.localPosition = itemLocalTransform;
                    
                    if (levelContents.vipTargets)
                    {
                        var possibleVip = InterfaceHelper.GetInterface<IVip>(itemGameObject);
                        if (possibleVip != null && UnityEngine.Random.Range(0f, 1.0f) < sceneInput.vipProbability)
                        {
                            possibleVip.SetVip();
                        }
                    }

                    retInner.Add(itemGameObject);
                }

                if ((levelContents.cells[xtmp, ztmp] & CellContent.AIR_MASK) == CellContent.BALLOON)
                {
                    var balloonShadowGameObject = Instantiate(balloonShadowPrefab, sceneInput.levelTransform);
                    balloonShadowGameObject.transform.localPosition = itemLocalTransform;

                    var balloonGameObject = Instantiate(balloonPrefab, sceneInput.balloonParentTransform);
                    balloonGameObject.transform.position = balloonShadowGameObject.transform.position;
                    Balloon balloon = InterfaceHelper.GetInterface<Balloon>(balloonGameObject);
                    balloon.SetShadow(balloonShadowGameObject);
                    retInner.Add(balloonShadowGameObject);
                }

                return retInner;
            });

            gameObjectCollections[ztmp].gameObjects = gameObjectCollections[ztmp].gameObjects.Concat(gameObjectsAtZ);
        }

        ret.gameObjects = gameObjectCollections.ToList();
        return ret;
    }
    
}