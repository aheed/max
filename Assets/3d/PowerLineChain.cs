using UnityEngine;

public class PowerLineChain : ManagedObject
{
    public GameObject powerLinePrefab;
    public GameObject powerPostPrefab;
    public float powerLineSegmentLength = 5f; // check mesh bounds instead?
    public float powerPostHeight = 2.5f; // check mesh bounds instead?
    public float powerLineDistanceZ = 0.1f;
    public float powerLineAltitude = 2.5f;
    static float levelWidth;

    public static void SetLevelWidth(float width)
    {
        levelWidth = width;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {        
        for (float x = 0; x < levelWidth; x += powerLineSegmentLength)
        {
            var powerX = x + (powerLineSegmentLength / 2) - (levelWidth / 2);
            var powerZ = 0; // will be overridden by road z
            var powerLineGameObject = Instantiate(powerLinePrefab, transform);
            powerLineGameObject.transform.localPosition = new Vector3(powerX, powerLineAltitude, powerZ - powerLineDistanceZ);
            powerLineGameObject = Instantiate(powerLinePrefab, transform);
            powerLineGameObject.transform.localPosition = new Vector3(powerX, powerLineAltitude, powerZ + powerLineDistanceZ);
            var powerPostGameObject = Instantiate(powerPostPrefab, transform);
            powerPostGameObject.transform.localPosition = new Vector3(x - (levelWidth / 2), powerLineAltitude - (powerPostHeight / 2), powerZ);
        }        
    }
}
