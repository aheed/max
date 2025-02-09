using UnityEngine;

public class FlakProjectile3d : MonoBehaviour
{
    public GameObject flackExplosionPrefab;
    public static readonly float lifeSpanSec = 2.0f;
    public float inaccuracy = 0.1f;
    public float forwardAimOffset = 0.1f;
    float timeToLiveSec = lifeSpanSec;
    Vector3 velocity;

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
        
        timeToLiveSec -= Time.deltaTime;
        if (timeToLiveSec < 0f)
        {
            Instantiate(flackExplosionPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
