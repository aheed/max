using System;
using UnityEngine;

public class BombBouncing : MonoBehaviour
{
    public GameObject bombSplashPrefab;
    public float speedZ = 3.0f;
    public float endAltitude = -3f; // The altitude at which the bomb will destroy itself
    public float minYSpeed = 0.001f; // Sink, not bounce at this vertical speed
    public float minBounceSpeed = 0.1f;
    public float maxImpactSpeed = 2.0f; // If the impact speed is greater than this, destroy the bomb
    public float minBounceAltitude = 0.2f;
    float minBounceAltitudeAbsolute;
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
        minBounceAltitudeAbsolute = GameState.GetInstance().riverAltitude + minBounceAltitude;
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
        return hasBounced &&
            rb.linearVelocity.magnitude < minBounceSpeed &&
            transform.position.y < minBounceAltitudeAbsolute;
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"BombBouncing collided with {collision.gameObject.name}");
        hasBounced = true;

        if (collision.gameObject.name.StartsWith("DamWater"))
        {
            Debug.Log($"BombBouncing collided with {collision.gameObject.name} at velocity {rb.linearVelocity}");
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
