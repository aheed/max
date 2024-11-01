using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlackGun : MonoBehaviour, IPositionObservable
{
    public GameObject flackProjectilePrefab;
    public Sprite shotSprite;
    public float avgTimeToShootSeconds = 5.0f;
    float timeToShoot = -1.0f;
    GameState gameState;
    private SpriteRenderer spriteR;
    private bool alive = true;

    void RestartShotClock()
    {
        timeToShoot = Random.Range(0f, 2 * avgTimeToShootSeconds);
    }

    void Shoot()
    {
        Instantiate(flackProjectilePrefab, transform.position, Quaternion.identity);
    }

    void HandleCollision(Collider2D col)
    {
        var collObjName = CollisionHelper.GetObjectWithOverlappingAltitude(this, col.gameObject);
        if (collObjName.StartsWith("bullet"))
        {
            spriteR.sprite = shotSprite;
            var collider = gameObject.GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = false;
            }
            alive = false;

            // Todo: report destroyed flack gun for scoring
        }

        //no collision
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.name.StartsWith("bomb"))
        {
            var bomb = col.gameObject.GetComponent<Bomb>();
            gameState.BombLanded(bomb, gameObject);
            return;
        }

        HandleCollision(col);
    }

    // Start is called before the first frame update
    void Start()
    {
        spriteR = gameObject.GetComponent<SpriteRenderer>();
        RestartShotClock();        
    }

    // Update is called once per frame
    void Update()
    {
        if (gameState == null)
        {
            gameState = FindObjectOfType<GameState>();
        }

        if (!alive)
        {
            return;
        }

        timeToShoot -= Time.deltaTime;
        if (timeToShoot < 0f)
        {
            Shoot();
            RestartShotClock();
        }
    }

    public Vector2 GetPosition() => transform.position;
    public float GetAltitude() => 0.1f;
    public float GetHeight() => 0.41f;
}
