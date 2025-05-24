using System;
using UnityEngine;

public class BouncingBomb : MonoBehaviour
{
    public float speedZ = 3.0f;
    public float endAltitude = -1f; // The altitude at which the bomb will destroy itself
    public float minYSpeed = 0.001f; // Sink, not bounce at this vertical speed
    public float minBounceSpeed = 0.1f;
    Rigidbody rb;
    bool hasBounced = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = new Vector3(0, 0, speedZ);
        //rb.transform.localPosition += new Vector3(0, -0.2f, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y < endAltitude)
        {
            Debug.Log($"BouncingBomb destroyed at {transform.position}");
            Destroy(gameObject);
        }
        else if (ShouldStopBouncing())
        {
            Debug.Log($"Stop bouncing 1 {rb.linearVelocity.y} {minYSpeed}");
            GetComponentInChildren<MeshCollider>().enabled = false;
        }
    }

    bool ShouldStopBouncing()
    {
        //return hasBounced && Math.Abs(rb.linearVelocity.y) < minYSpeed;
        return hasBounced && rb.linearVelocity.magnitude < minBounceSpeed;
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"BouncingBomb collided with {collision.gameObject.name}");
        hasBounced = true;

        if (collision.gameObject.name.StartsWith("DamWater"))
        {
            Debug.Log($"BouncingBomb collided with {collision.gameObject.name} at velocity {rb.linearVelocity}");
            /*if (ShouldStopBouncing())
            {
                Debug.Log($"Stop bouncing 2 {rb.linearVelocity.y} {minYSpeed}");
                GetComponentInChildren<MeshCollider>().enabled = false;
            }*/
        }
    }
}
