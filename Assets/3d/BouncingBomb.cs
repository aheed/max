using UnityEngine;

public class BouncingBomb : MonoBehaviour
{
    public float speedZ = 3.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var rb = GetComponent<Rigidbody>();
        rb.linearVelocity = new Vector3(0, 0, speedZ);
        //rb.transform.localPosition += new Vector3(0, -0.2f, 0);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"BouncingBomb collided with {collision.gameObject.name}");
    }
}
