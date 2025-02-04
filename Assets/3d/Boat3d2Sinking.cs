using UnityEngine;

public class Boat3d2Sinking : MonoBehaviour
{
    public float sinkSpeed = 0.01f;
    public float sinkDepth = 1.0f;
    Vector3 startPosition;
    float depth = 0f;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        depth += sinkSpeed * Time.deltaTime;
        
        transform.position = startPosition - new Vector3(0, depth, 0);
        
        if (depth > sinkDepth)
        {
            Destroy(gameObject);
        }
    }
}
