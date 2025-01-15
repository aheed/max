using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class SceneBuilder : MonoBehaviour
{
    public GameObject riverSectionPrefab;
    public GameObject roadPrefab;
    public GameObject landingStripPrefab;
    public ExpHouse housePrefab;
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
    public Car carPrefab;
    public GameObject airstripEndPrefab;
    public GameObject hangarPrefab;
    public EnemyHQ enemyHqPrefab;
    public GameObject bigHousePrefab;
    public Material riverMaterial;
    public Material riverBankMaterial;
    public Material roadMaterial;
    public Material landingStripMaterial;

    Mesh CreateQuadMesh(Vector3[] verts)
    {
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

    //public List<GameObjectCollection> PopulateScene(LevelContents levelContents) => new(); //TEMP!!!

    
    // Create game objects
    // llcx, llcy: Lower Left Corner of the level
    public SceneOutput PopulateScene(LevelContents levelContents, SceneInput sceneInput)
    {
        float cellWidth = sceneInput.levelWidth / LevelContents.gridWidth;
        float cellHeight = sceneInput.levelHeight / LevelContents.gridHeight;
        var midX = LevelContents.gridWidth / 2;

        SceneOutput ret = new();
        GameObjectCollection[] gameObjects = new GameObjectCollection[LevelContents.gridHeight];
        for (var ztmp = 0; ztmp < LevelContents.gridHeight; ztmp++)
        {
            gameObjects[ztmp] = new GameObjectCollection {
                zCoord = ztmp * cellHeight, // level relative coordinate
                gameObjects = new List<GameObject>()
            };
        }

        // Landing Strip
        {
            var lsWidth = LevelBuilder.landingStripWidth * cellWidth;
            var lsHeight = LevelBuilder.landingStripHeight * cellHeight;

            var lsGameObject = Instantiate(landingStripPrefab, sceneInput.levelTransform);
            
            var lsLocalTransform = new Vector3((LevelContents.gridWidth / 2) * cellWidth - (lsWidth / 2), 0.21f, 0f);
            lsGameObject.transform.localPosition = lsLocalTransform;
            
            ret.landingStripStartZ = lsGameObject.transform.position.z; 
            ret.landingStripEndZ = ret.landingStripStartZ + lsHeight;
            ret.landingStripWidth = lsWidth;

            var lsllcX = 0f;
            var lslrcX = lsWidth;
            var lsulcX = 0f;
            var lsurcX = lsWidth;
            var lsllcZ = 0f;
            var lslrcZ = 0f;
            var lsulcZ = lsHeight;
            var lsurcZ = lsHeight;
            var lsMeshY = 0f;

            var lsMeshFilter = lsGameObject.AddComponent<MeshFilter>();
            var lsMeshRenderer = lsGameObject.AddComponent<MeshRenderer>();
            lsMeshRenderer.material = landingStripMaterial;

            var lsVerts = new Vector3[]
            {
                new Vector3(lsllcX, lsMeshY, lsllcZ),
                new Vector3(lslrcX, lsMeshY, lslrcZ),
                new Vector3(lsulcX, lsMeshY, lsulcZ),
                new Vector3(lsurcX, lsMeshY, lsurcZ)
            };

            var lsMesh = CreateQuadMesh(lsVerts);
            lsMeshFilter.mesh = lsMesh;
        }

        /*

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
                ret[road].gameObjects = ret[road].gameObjects.Concat((new GameObject[] {null}).Select(_ => 
                    {
                        Car car = Instantiate(carPrefab, lvlTransform);
                        var carLocalTransform = new Vector3(roadLeftEdgeX + carOffsetX, lowerEdgeY + (roadHeight / 2), -0.24f);
                        car.transform.localPosition = carLocalTransform;
                        if (levelContents.vipTargets && UnityEngine.Random.Range(0f, 1.0f) < vipProbability)
                        {
                            car.SetVip();
                        }
                        return car.gameObject;
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
                GameObject selectedPrefab = null;
                switch (levelContents.cells[xtmp, ytmp] & CellContent.LAND_MASK)
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
                        break;

                    case CellContent.BOAT2:
                        selectedPrefab = boat2Prefab;
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

                List<GameObject> ret = new();
                if (selectedPrefab != null)
                {
                    var itemGameObject = Instantiate(selectedPrefab, lvlTransform);
                    var itemLocalTransform = new Vector3(xtmp * cellWidth + ytmp * cellHeight * neutralSlope, ytmp * cellHeight, -0.24f);
                    itemGameObject.transform.localPosition = itemLocalTransform;
                    
                    if (levelContents.vipTargets)
                    {
                        var possibleVip = InterfaceHelper.GetInterface<IVip>(itemGameObject);
                        if (possibleVip != null && UnityEngine.Random.Range(0f, 1.0f) < vipProbability)
                        {
                            possibleVip.SetVip();
                        }
                    }

                    ret.Add(itemGameObject);
                }

                if ((levelContents.cells[xtmp, ytmp] & CellContent.AIR_MASK) == CellContent.BALLOON)
                {
                    var balloonShadowGameObject = Instantiate(balloonShadowPrefab, lvlTransform);
                    var itemLocalTransform = new Vector3(xtmp * cellWidth + ytmp * cellHeight * neutralSlope, ytmp * cellHeight, -0.24f);
                    balloonShadowGameObject.transform.localPosition = itemLocalTransform;

                    var balloonGameObject = Instantiate(balloonPrefab, balloonParent.transform);
                    balloonGameObject.transform.position = balloonShadowGameObject.transform.position;
                    Balloon balloon = InterfaceHelper.GetInterface<Balloon>(balloonGameObject);
                    balloon.SetShadow(balloonShadowGameObject);
                    ret.Add(balloonShadowGameObject);
                }

                return ret;
            });

            ret[ytmp].gameObjects = ret[ytmp].gameObjects.Concat(gameObjectsAtY);
        }

        */
        ret.gameObjects = gameObjects.ToList();
        return ret;
    }
    
}