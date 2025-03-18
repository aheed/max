using UnityEngine;

public class BossShadowCaster : MonoBehaviour
{
    public float altitude = 8f;
    public float maxDistanceZ = 10f;
    public float speedX = 0.1f;
    public float speedZ = 1.5f;
    public float startOffsetX = 5f;
    public Vector3 velocity;
    GameObject refObject;

    public void Init(GameObject obj)
    {
        refObject = obj;
        var startPosition = refObject.transform.position + new Vector3(startOffsetX, altitude, -maxDistanceZ / 2);
        transform.position = startPosition;
        var refSpeed = GameState.GetInstance().maxSpeed;
        velocity = new Vector3(speedX * refSpeed, 0, speedZ * refSpeed);
    }

    // Update is called once per frame
    void Update()
    {
        if(!refObject)
        {
            return;
        }

        if(transform.position.z - refObject.transform.position.z > maxDistanceZ)
        {
            //transform.position = refObject.transform.position - new Vector3(0, 0, maxDistanceZ);
            Destroy(gameObject);
            return;
        }
        
        transform.position += velocity * Time.deltaTime;
    }
}
