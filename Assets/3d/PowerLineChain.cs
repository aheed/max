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
    static GameObject InnerTemplate;
    GameObject Inner;

    public static void SetLevelWidth(float width)
    {
        levelWidth = width;
        InnerTemplate = null;
    }

    void Start()
    {
        if (InnerTemplate == null)
        {
            InnerTemplate = new GameObject("PowerLineChainInner");

            var lineOffsetX = (powerLineSegmentLength / 2) - (levelWidth / 2);
            var postOffsetX = -(levelWidth / 2);

            for (float x = 0; x < levelWidth; x += powerLineSegmentLength)
            {
                var powerX = x + lineOffsetX;

                var powerLineGameObject1 = Instantiate(
                    powerLinePrefab,
                    InnerTemplate.transform);
                powerLineGameObject1.transform.localPosition = new Vector3(powerX, powerLineAltitude, -powerLineDistanceZ);

                var powerLineGameObject2 = Instantiate(
                    powerLinePrefab,
                    InnerTemplate.transform);
                powerLineGameObject2.transform.localPosition = new Vector3(powerX, powerLineAltitude, powerLineDistanceZ);

                var powerPostGameObject = Instantiate(
                    powerPostPrefab,
                    InnerTemplate.transform);
                powerPostGameObject.transform.localPosition = new Vector3(x + postOffsetX, powerLineAltitude - (powerPostHeight / 2), 0);
            }
        }

        // A clone will suffice since all power lines are the same
        Inner = Instantiate(InnerTemplate, transform);
    }
}
