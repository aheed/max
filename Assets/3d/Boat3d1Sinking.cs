using UnityEngine;

public class Boat3d1Sinking : MonoBehaviour
{
    public float sinkSpeed = 0.1f;
    public float sinkDepth = 1.0f;
    Vector3 startPosition;
    float depth = 0f;

    GameObject sinkingModel;

    void Start()
    {
        sinkingModel = transform.GetChild(0).gameObject;
        startPosition = sinkingModel.transform.position;
    }

    void Update()
    {
        depth += sinkSpeed * Time.deltaTime;
        
        sinkingModel.transform.position = startPosition - new Vector3(0, depth, 0);
        
        if (depth > sinkDepth)
        {
            Destroy(gameObject);
        }
    }
}
