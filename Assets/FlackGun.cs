using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlackGun : MonoBehaviour, IPositionObservable
{
    public GameObject flackProjectilePrefab;
    public Sprite shotSprite;
    public float avgTimeToShootSeconds = 5.0f;
    float timeToShoot = -1.0f;
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
            var gameState = FindAnyObjectByType<GameState>();
            gameState.ReportEvent(GameEvent.SMALL_DETONATION);
            gameState.ReportEvent(GameEvent.SMALL_BANG);

            // Todo: report destroyed flack gun for scoring
        }

        //no collision
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.name.StartsWith("bomb"))
        {
            var bomb = col.gameObject.GetComponent<Bomb>();
            FindAnyObjectByType<GameState>().BombLanded(bomb, gameObject);
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
        timeToShoot -= Time.deltaTime;
        if (timeToShoot < 0f && alive)
        {
            Shoot();
            RestartShotClock();
        }
    }

    public Vector2 GetPosition() => transform.position;
    public float GetAltitude() => Altitudes.strafeMaxAltitude / 2;
    public float GetHeight() => Altitudes.strafeMaxAltitude;
}
