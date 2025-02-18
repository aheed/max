using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlackProjectile : MonoBehaviour
{
    public GameObject flackExplosionPrefab;
    public float speedMax = 5.0f;
    public float speedMin = 1.0f;
    public float lifeSpanSec = 2.0f;
    float timeToLiveSec;
    Vector2 speed;

    // Start is called before the first frame update
    void Start()
    {
        timeToLiveSec = lifeSpanSec;
        speed = new Vector2(Random.Range(-speedMax, speedMax), Random.Range(speedMin, speedMax));
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 progress = speed * Time.deltaTime;
        transform.position += progress;

        timeToLiveSec -= Time.deltaTime;
        if (timeToLiveSec < 0f)
        {
            // Place the explosion at same altitude as the plane
            // for correct 3D sound distance.
            Vector3 explosionPos = new Vector3(
                transform.position.x,
                transform.position.y,
                GameState.GetInstance().GetStateContents().altitude);
            Instantiate(flackExplosionPrefab, explosionPos, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
