using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlackGun : MonoBehaviour
{
    public GameObject flackProjectilePrefab;
    public float avgTimeToShootSeconds = 5.0f;
    float timeToShoot = -1.0f;

    void RestartShotClock()
    {
        timeToShoot = Random.Range(0f, 2 * avgTimeToShootSeconds);
    }

    void Shoot()
    {
        Instantiate(flackProjectilePrefab, transform.position, Quaternion.identity);
    }

    // Start is called before the first frame update
    void Start()
    {
        RestartShotClock();
    }

    // Update is called once per frame
    void Update()
    {
        timeToShoot -= Time.deltaTime;
        if (timeToShoot < 0f)
        {
            Shoot();
            RestartShotClock();
        }
    }
}
