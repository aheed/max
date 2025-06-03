using System;
using UnityEngine;

public class BombBouncing : MonoBehaviour
{
    public GameObject bombSplashPrefab;
    public float speedZ = 3.0f;
    public float endAltitude = -3f; // The altitude at which the bomb will destroy itself
    public float minSpeed = 0.1f; // Destroy the bomb if its speed is below this value
    public float maxImpactSpeed = 2.0f; // If the impact speed is greater than this, destroy the bomb
    float endAltitudeAbsolute;
    Rigidbody rb;
    bool stoppedBouncing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = new Vector3(0, 0, speedZ);
        endAltitudeAbsolute = GameState.GetInstance().riverAltitude + endAltitude;
    }

    void Update()
    {
        if (transform.position.y < endAltitudeAbsolute)
        {
            Debug.Log($"BombBouncing destroyed at {transform.position}");
            Destroy(gameObject);
        }
        else if (!stoppedBouncing && ShouldStopBouncing())
        {
            Debug.Log($"Stop bouncing 1 {rb.linearVelocity.y}");
            GetComponentInChildren<MeshCollider>().enabled = false;
            stoppedBouncing = true;
            Splash();
        }
    }

    void Splash()
    {
        Vector3 splashPosition = transform.position;
        splashPosition.y = GameState.GetInstance().riverAltitude + 0.01f; // Ensure the splash is above the river altitude
        Instantiate(bombSplashPrefab, splashPosition, Quaternion.identity, transform.parent);
    }

    bool ShouldStopBouncing()
    {
        return rb.linearVelocity.magnitude < minSpeed;
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"BombBouncing collided with {collision.gameObject.name}");

        if (collision.gameObject.name.StartsWith("DamWater") ||
            collision.gameObject.name.StartsWith("riversection"))
        {
            Debug.Log($"BombBouncing collided at {transform.position} at velocity {rb.linearVelocity}");
            Splash();
            if (Math.Abs(rb.linearVelocity.y) > maxImpactSpeed)
            {
                // Too hard impact, destroy the bomb
                Debug.Log($"BombBouncing destroyed at {transform.position} due to hard impact");
                GameState.GetInstance().ReportEvent(GameEvent.SMALL_BANG);
                GameState.GetInstance().ReportEvent(GameEvent.SMALL_DETONATION);
                Destroy(gameObject);
            }
        }
    }
}
