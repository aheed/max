using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

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
    float riverLowerLeftCornerX = 0f;
    static readonly float[] riverSlopes = new float[] {0.5f, 0.5f, 1.0f, 2.0f, 2.0f};
    static readonly int neutralRiverSlopeIndex = 2;

    //
    public float levelWidth = 8f;
    public float levelHeight = 80f;

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
    public void PopulateScene(LevelContents levelContents, float llcx, float llcy)
    {
        float cellWidth = levelWidth / LevelContents.gridWidth;
        float cellHeight = levelHeight / LevelContents.gridHeight;
        float neutralSlope = riverSlopes[neutralRiverSlopeIndex];
        var midX = LevelContents.gridWidth / 2;

        // Ground
        var grPos = new Vector3(llcx, llcy, -0.2f);
        var grGameObject = Instantiate(groundPrefab, grPos, Quaternion.identity);
        
        var grUpperCornerOffsetX = levelHeight * neutralSlope;

        var grMeshFilter = grGameObject.AddComponent<MeshFilter>();
        var grMeshRenderer = grGameObject.AddComponent<MeshRenderer>();
        grMeshRenderer.material = groundMaterial;

        var grVerts = new List<Vector2>
        {
            new Vector2(0f, 0f),
            new Vector2(levelWidth, 0),
            new Vector2(grUpperCornerOffsetX, levelHeight),
            new Vector2(levelWidth + grUpperCornerOffsetX, levelHeight)
        };

        var grMesh = CreateQuadMesh(grVerts);
        grMeshFilter.mesh = grMesh;

        // Landing Strip
        var lsWidth = LevelBuilder.landingStripWidth * cellWidth;
        var lsHeight = LevelBuilder.landingStripHeight * cellHeight;

        var startPos = new Vector3(llcx + (LevelContents.gridWidth / 2) * cellWidth - (lsWidth / 2), llcy, -0.2f);
        var lsGameObject = Instantiate(landingStripPrefab, startPos, Quaternion.identity);
        
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
        startPos = new Vector3(llcx + levelContents.riverLowerLeftCornerX * cellWidth, llcy, -0.2f);
        var rsGameObject = Instantiate(riverSectionPrefab, startPos, Quaternion.identity);

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
            var roadPos = new Vector3(llcx + road * cellHeight * neutralSlope, llcy + road * cellHeight, -0.2f);
            var roadGameObject = Instantiate(roadPrefab, roadPos, Quaternion.identity);

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
            var housePos = new Vector3(llcx + house.x * cellWidth + house.y * cellHeight * neutralSlope, llcy + house.y * cellHeight, -0.2f);
            Instantiate(housePrefab, housePos, Quaternion.identity);
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
                    var itemPos = new Vector3(llcx + xtmp * cellWidth + ytmp * cellHeight * neutralSlope, llcy + ytmp * cellHeight, -0.2f);
                    Instantiate(selectedPrefab, itemPos, Quaternion.identity);
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
        MaxControl maxPlane = Instantiate(maxPlanePrefab, startPos, Quaternion.identity, refobject.transform);
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

        //CreateRiverSection();
        var level = LevelBuilder.Build(true);
        var levelLowerLeftCornerX = refobject.transform.position.x - levelWidth / 2;
        PopulateScene(level, levelLowerLeftCornerX, refobject.transform.position.y);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
