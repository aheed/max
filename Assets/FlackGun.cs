using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlackGun : MonoBehaviour
{
    public GameObject flackProjectilePrefab;    
    public float avgTimeToShootSeconds = 5.0f;
    float timeToShoot = -1.0f;
    GameState gameState;

    void RestartShotClock()
    {
        timeToShoot = Random.Range(0f, 2 * avgTimeToShootSeconds);
    }

    void Shoot()
    {
        Instantiate(flackProjectilePrefab, transform.position, Quaternion.identity);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.name.StartsWith("bomb"))
        {
            var bomb = col.gameObject.GetComponent<Bomb>();
            gameState.BombLanded(bomb, gameObject);
        }
        else if (col.name.StartsWith("bullet"))
        {
            //todo
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        RestartShotClock();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameState == null)
        {
            gameState = FindObjectOfType<GameState>();
        }

        timeToShoot -= Time.deltaTime;
        if (timeToShoot < 0f)
        {
            Shoot();
            RestartShotClock();
        }
    }
}
