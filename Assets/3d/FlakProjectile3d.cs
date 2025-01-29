using UnityEngine;

public class FlakProjectile3d : MonoBehaviour
{
    public GameObject flackExplosionPrefab;
    public static readonly float lifeSpanSec = 2.0f;
    public float inaccuracy = 0.1f;
    public float forwardAimOffset = 0.1f;
    float timeToLiveSec = lifeSpanSec;
    Vector3 velocity;
    Vector3 startPosition;
    int updates = 0;
    float lSpan;
    float expectedImpactZ;

    /*
    // Start is called before the first frame update
    void Start()
    {
        timeToLiveSec = lifeSpanSec;
        velocity = new Vector2(Random.Range(-speedMax, speedMax), Random.Range(speedMin, speedMax));
    }*/

    public void Initialize(Vector3 position, Vector3 velocity, float ttl, float expectedImpactZ)
    {
        //timeToLiveSec = lifeSpanSec;
        timeToLiveSec = ttl;
        transform.position = position;
        startPosition = position;
        this.velocity = velocity;
        lSpan = ttl;
        this.expectedImpactZ = expectedImpactZ;
    }

    Vector3 Inaccurate(Vector3 v) => new Vector3(
            v.x * (1 + Random.Range(-inaccuracy, inaccuracy)),
            v.y * (1 + Random.Range(-inaccuracy, inaccuracy)),
            v.z * (1 + Random.Range(-inaccuracy, inaccuracy)));

    public void Initialize(Vector3 position, Vector3 targetPosition, Vector3 targetvelocity)
    {
        timeToLiveSec = lifeSpanSec;
        transform.position = position;
        var endPosition = targetPosition + targetvelocity * timeToLiveSec * (1 + forwardAimOffset);
        var trajectory = endPosition - position;
        velocity = Inaccurate(trajectory) / timeToLiveSec;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 progress = velocity * Time.deltaTime;
        transform.position += progress;
        updates++;
        
        timeToLiveSec -= Time.deltaTime;
        if (timeToLiveSec < 0f)
        {
            Debug.Log($"FlakProjectile3d.Update: {transform.position} start:{startPosition} progress: {progress} updates: {updates} lspan:{lSpan} Time.deltaTime:{Time.deltaTime} expectedImpactZ:{expectedImpactZ}");
            Instantiate(flackExplosionPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
