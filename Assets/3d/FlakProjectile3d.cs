using UnityEngine;

public class FlakProjectile3d : MonoBehaviour
{
    public GameObject flackExplosionPrefab;
    public static readonly float lifeSpanSec = 1.0f;
    float timeToLiveSec = lifeSpanSec;
    Vector3 velocity;

    /*
    // Start is called before the first frame update
    void Start()
    {
        timeToLiveSec = lifeSpanSec;
        velocity = new Vector2(Random.Range(-speedMax, speedMax), Random.Range(speedMin, speedMax));
    }*/

    public void Initialize(Vector3 position, Vector3 velocity)
    {
        timeToLiveSec = lifeSpanSec;
        transform.position = position;
        this.velocity = velocity;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 progress = velocity * Time.deltaTime;
        transform.position += progress;

        timeToLiveSec -= Time.deltaTime;
        if (timeToLiveSec < 0f)
        {
            Instantiate(flackExplosionPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
