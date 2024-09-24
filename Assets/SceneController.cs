using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneController : MonoBehaviour
{
    public MaxControl maxPlanePrefab;
    public EnemyPlane enemyPlanePrefab;
    public ShadowControl shadowControlPrefab;
    public GameObject riverSectionPrefab;
    public refobj refobject;
    public float width = 1;
    public float height = 1;
    public float riverSectionHeight = 20f;
    public float riverWidth = 4.0f;
    public float maxSegmentHeight = 3.0f;
    public float minSegmentHeight = 0.5f;
    public float minDistanceRiverAirstrip = 5.0f;
    public float maxDistanceRiverToAdjust = 2.0f;
    public float approachQuotient = 0.2f;
    public Material riverMaterial;
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

    // Create game objects
    // llcx, llcy: Lower Left Corner of the level
    public void PopulateScene(LevelContents levelContents, float llcx, float llcy)
    {
        float cellWidth = levelWidth / LevelContents.gridWidth;
        float cellHeight = levelHeight / LevelContents.gridHeight;

        // Landing Strip        

        // River
        var startPos = new Vector3(llcx + levelContents.riverLowerLeftCornerX * cellWidth, llcy, -0.2f);
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

            var xOffset = segment.slope * segment.height * cellWidth + segmentHeight * riverSlopes[neutralRiverSlopeIndex];

            //Debug.Log($"{riverLowerLeftCornerX} {riverWidth} {xOffset} {y} {segmentHeight}");
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
        // Bridges
        // Houses
        // Flack guns
        // Tanks
        // Trees
    }

    void CreateRiverSection()
    {
        // GameObject
        //var startPos = transform.position;
        var startPos = refobject.transform.position;
        startPos.z = -0.2f;
        var rsGameObject = Instantiate(riverSectionPrefab, startPos, Quaternion.identity);

        // MeshRenderer
        var rsMeshFilter = rsGameObject.AddComponent<MeshFilter>();
        var rsMeshRenderer = rsGameObject.AddComponent<MeshRenderer>();

        rsMeshRenderer.material = riverMaterial;

        // Mesh
        //var y = rsGameObject.transform.position.y;
        var startY = 0f;
        var y = startY;
        var maxY = y + riverSectionHeight;
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();
        int segments = 0;
        while (y < maxY)
        {
            var segmentHeight = Random.Range(minSegmentHeight, maxSegmentHeight);
            if (y + segmentHeight > maxY)
            {
                segmentHeight = maxY - y;
            }

            var midRiverX = riverLowerLeftCornerX + (riverWidth / 2);
            var refXatY = riverSlopes[neutralRiverSlopeIndex] * (y - startY);
            bool riverLeftOfAirstrip = midRiverX < refXatY;
            var minSlopeIndex = 1;
            var maxSlopeIndexExclusive = riverSlopes.Length - 1;
            bool approaching = maxY - y < riverSectionHeight * approachQuotient;
            bool takingOff = y - startY < riverSectionHeight * approachQuotient;
            int slopeIndexOffset = 0;
            if (approaching)
            {
                // Airstrip approaching. River must not bend toward next airstrip location.
                slopeIndexOffset =  riverLeftOfAirstrip ? -1 : 1;
            }
            if (takingOff)
            {
                // Leaving Airstrip. River must not bend away from next airstrip location.
                slopeIndexOffset =  riverLeftOfAirstrip ? 1 : -1;
            }
            minSlopeIndex += slopeIndexOffset;
            maxSlopeIndexExclusive += slopeIndexOffset;
            var slopeIndex = Random.Range(minSlopeIndex, maxSlopeIndexExclusive);

            var slopeX = riverSlopes[slopeIndex] * segmentHeight;

            Debug.Log($"riverLowerLeftCornerX riverWidth slopeX y segmentHeight: {riverLowerLeftCornerX} {riverWidth} {slopeX} {y} {segmentHeight} {approaching} {takingOff} {minSlopeIndex} {maxSlopeIndexExclusive} {riverLeftOfAirstrip}");
            vertices.Add(new Vector3(riverLowerLeftCornerX, y, 0));
            vertices.Add(new Vector3(riverLowerLeftCornerX + riverWidth, y, 0));
            vertices.Add(new Vector3(riverLowerLeftCornerX + slopeX, y + segmentHeight, 0));
            vertices.Add(new Vector3(riverLowerLeftCornerX + riverWidth + slopeX, y + segmentHeight, 0));

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
            riverLowerLeftCornerX += slopeX;
            segments += 1;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        mesh.uv = uvs.ToArray();
        
        rsMeshFilter.mesh = mesh;
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
        PopulateScene(level, refobject.transform.position.x, refobject.transform.position.y);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
