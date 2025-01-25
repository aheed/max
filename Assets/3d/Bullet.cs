using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 8.0f;
    public float range = 10.0f;
    Vector3 velocity;
    Vector3 startPosition;

    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position;
        velocity = new Vector3(0, 0, speed);
        //Debug.Log($"Bullet spawned at {transform.position}");
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 position = transform.position;
        position += velocity * Time.deltaTime;
        transform.position = position;
        if (transform.position.z > (startPosition.z + range))
        {
            //Debug.Log($"3D Bullet out of sight at {transform.position}");
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision col)
    {
        //Debug.Log($"3D Hit!!!!!!!!!!!!!!! Bullet at altitude {transform.position.z} collided with {col.gameObject.name}");
        if (col.gameObject.name.StartsWith("Player") || 
            col.gameObject.name.StartsWith("flack_expl") ||
            col.gameObject.name.StartsWith("bomb") ||
            col.gameObject.name.StartsWith("balloon"))
        {
            return;
        }

        Debug.Log($"3D Hit!!!!!!!!!!!!!!! Bullet at altitude {transform.position.z} collided with {col.gameObject.name} HIT!!!");
        Destroy(gameObject);
    }
}
