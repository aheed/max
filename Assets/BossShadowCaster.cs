using System;
using UnityEngine;

public enum BossShadowVariant
{
    BSH1,
    BSH2,
    BSH3,
}

public class BossShadowCaster : MonoBehaviour
{
    GameObject refObject;
    public float altitude = 8f;

    // BSH1
    public float Bsh1SpeedX = 1.2f;
    public float Bsh1CrossDistanceZ = 1f;
    public float Bsh1Scale = 0.1f;

    // BSH2
    public float Bsh2SpeedZ = 0.8f;
    public float Bsh2Scale = 0.1f;
    public float Bsh2OffsetX = -2f;
    
    // BSH3
    public float maxDistanceZ = 10f;
    public float speedX = 0.1f;
    public float speedZ = 1.5f;
    public float startOffsetX = 5f;

    // State
    Vector3 velocity;

    public void Init(GameObject obj, BossShadowVariant variant)
    {
        refObject = obj;

        if (variant == BossShadowVariant.BSH1)
        {
            var refSpeed = GameState.GetInstance().maxSpeed;
            var startDistanceX = -Bsh1SpeedX * (maxDistanceZ - Bsh1CrossDistanceZ) / refSpeed;
            var startPosition = refObject.transform.position + new Vector3(startDistanceX, altitude, maxDistanceZ);
            transform.position = startPosition;
            transform.rotation = Quaternion.Euler(0, 90, 0);
            transform.localScale = new Vector3(Bsh1Scale, Bsh1Scale, Bsh1Scale);
            velocity = new Vector3(Bsh1SpeedX * refSpeed, 0, 0);
        }
        else if (variant == BossShadowVariant.BSH2)
        {
            var refSpeed = GameState.GetInstance().maxSpeed;
            var startPosition = refObject.transform.position + new Vector3(Bsh2OffsetX, altitude, maxDistanceZ);
            transform.position = startPosition;
            transform.rotation = Quaternion.Euler(0, 180, 0);
            transform.localScale = new Vector3(Bsh2Scale, Bsh2Scale, Bsh2Scale);
            velocity = new Vector3(0, 0, -Bsh2SpeedZ * refSpeed);
        }
        else if (variant == BossShadowVariant.BSH3)
        {
            var startPosition = refObject.transform.position + new Vector3(startOffsetX, altitude, -maxDistanceZ);
            transform.position = startPosition;
            var refSpeed = GameState.GetInstance().maxSpeed;
            velocity = new Vector3(speedX * refSpeed, 0, speedZ * refSpeed);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!refObject)
        {
            return;
        }

        transform.position += velocity * Time.deltaTime;

        if(Math.Abs(transform.position.z - refObject.transform.position.z) > maxDistanceZ)
        {
            Debug.Log("Destroying boss shadow sssssssssssssssssssssssssssssssssssssss");
            Destroy(gameObject);
            return;
        }        
    }
}
