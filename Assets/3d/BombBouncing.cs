using System;
using UnityEngine;

public class BombBouncing : MonoBehaviour
{
    public GameObject bombSplashPrefab;
    public float speedZ = 3.0f;
    public float endAltitude = -1f; // The altitude at which the bomb will destroy itself
    public float minYSpeed = 0.001f; // Sink, not bounce at this vertical speed
    public float minBounceSpeed = 0.1f;
    Rigidbody rb;
    bool hasBounced = false;
    bool stoppedBouncing = false;
    float impactAltitude;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = new Vector3(0, 0, speedZ);
        //rb.transform.localPosition += new Vector3(0, -0.2f, 0);
        impactAltitude = 0f; 
    }

    void Update()
    {
        if (transform.position.y < endAltitude)
        {
            Debug.Log($"BombBouncing destroyed at {transform.position}");
            Destroy(gameObject);
        }
        else if (!hasBounced && transform.position.y < impactAltitude)
        {
            impactAltitude = endAltitude - 1f; // Don't get here again
            GameState.GetInstance().BombLanded(gameObject, null); // May not actually have landed
        }
        else if (!stoppedBouncing && ShouldStopBouncing())
        {
            Debug.Log($"Stop bouncing 1 {rb.linearVelocity.y} {minYSpeed}");
            GetComponentInChildren<MeshCollider>().enabled = false;
            stoppedBouncing = true;
            Splash();
        }
    }

    void Splash()
    {
        Instantiate(bombSplashPrefab, transform.position, Quaternion.identity, transform.parent);
    }

    bool ShouldStopBouncing()
    {
        return hasBounced && rb.linearVelocity.magnitude < minBounceSpeed;
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"BombBouncing collided with {collision.gameObject.name}");
        hasBounced = true;

        if (collision.gameObject.name.StartsWith("DamWater"))
        {
            Debug.Log($"BombBouncing collided with {collision.gameObject.name} at velocity {rb.linearVelocity}");
            Splash();
        }
    }
}
