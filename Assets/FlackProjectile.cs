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
            Instantiate(flackExplosionPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
