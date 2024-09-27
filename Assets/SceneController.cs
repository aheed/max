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

public class SceneController : MonoBehaviour
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

    //// Game status
    int level = -1;
    GameStatus gameStatus = GameStatus.ACCELERATING;
    float prepTimeForNextLevelQuotient = 0.98f;
    float lastLevelLowerEdgeX = 0f;
    int currentLevelIndex = 0;
    static int nofLevels = 2;
    GameObject[] levels = new GameObject[nofLevels];
    LevelContents latestLevel;
    MaxControl maxPlane;
    float landingStripBottomY;
    float landingStripTopY;
    float landingStripWidth;
    public float maxSpeed = 2.0f;
    public float acceleration = 0.4f;
    float speed = 0f;
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
        lastLevelLowerEdgeX = llcx;
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
    public void PopulateScene(LevelContents levelContents)
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
        //var startY = 0f;
        //var y = startY;
        //var maxY = y + riverSectionHeight;
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

        // Single cell items: Flack guns, trees, tanks
        for (var xtmp = 0; xtmp < LevelContents.gridWidth; xtmp++)
        {
            for (var ytmp = 0; ytmp < LevelContents.gridHeight; ytmp++)
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
                }

            }
        }    
    }

    void Start()
    {
        //var refobject = GetComponent<refobj>();
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

        latestLevel = LevelBuilder.Build(true);
        var levelLowerLeftCornerX = 0f;
        var newRefObjPos = new Vector3(levelLowerLeftCornerX + levelWidth / 2, 0f, 0f);
        refobject.transform.position = newRefObjPos;
        RotateLevels();
        PopulateScene(latestLevel);
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
        if (refobject.transform.position.x > (lastLevelLowerEdgeX + levelHeight * prepTimeForNextLevelQuotient))
        {
            Debug.Log("Time to add new level ***************");
            latestLevel = LevelBuilder.Build(latestLevel.riverEndsLeftOfAirstrip);
            RotateLevels();
            PopulateScene(latestLevel);
        }

        // Update game state
        if (gameStatus == GameStatus.FLYING)
        {
            //Debug.Log($"Alt: {maxPlane.GetAltitude()} ({MaxControl.landingAltitude})");
            if (maxPlane.GetAltitude() <= MaxControl.landingAltitude) 
            {
                //Debug.Log("Low");
                if (IsOverLandingStrip(maxPlane.GetPosition()))
                {
                    Debug.Log(">>>>>>>>> Landing <<<<<<<<<");
                    PreventRelanding();
                    gameStatus = GameStatus.DECELERATING;
                }
            }
        }

        if (gameStatus == GameStatus.ACCELERATING)
        {
            speed += acceleration * maxSpeed * Time.deltaTime;
            if (speed > maxSpeed)
            {
                speed = maxSpeed;
                gameStatus = GameStatus.FLYING;
            }
        }

        if (gameStatus == GameStatus.DECELERATING)
        {
            speed -= acceleration * maxSpeed * Time.deltaTime;
            if (speed < 0f)
            {
                speed = 0f;
                gameStatus = GameStatus.REFUELLING;
            }
        }

        if (gameStatus == GameStatus.REFUELLING)
        {
            // Todo: Refuelling and repair
            
            gameStatus = GameStatus.ACCELERATING;
        }

        // Update refobject position
        Vector2 levelVelocity = new(speed, speed);
        Vector3 delta = levelVelocity * Time.deltaTime;
        refobject.transform.position += delta;
    }
}
