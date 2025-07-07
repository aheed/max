using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlackGun : ManagedObject, IPositionObservable
{
    public GameObject flackProjectilePrefab;
    public Sprite normalSprite;
    public Sprite shotSprite;
    public float avgTimeToShootSeconds = 5.0f;
    float timeToShoot = -1.0f;
    private SpriteRenderer spriteR;
    private bool alive = true;
    static readonly int points = 10;

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
            var gameState = GameState.GetInstance();
            gameState.ReportEvent(GameEvent.SMALL_DETONATION);
            gameState.ReportEvent(GameEvent.SMALL_BANG);
            gameState.AddScore(points);

            spriteR.sprite = shotSprite;
            if (gameObject.TryGetComponent<Collider2D>(out var collider))
            {
                collider.enabled = false;
            }
            alive = false;
        }

        //no collision
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.name.StartsWith("bomb"))
        {
            var bomb = col.gameObject.GetComponent<Bomb>();
            GameState.GetInstance().BombLanded(bomb, gameObject);
            GameState.GetInstance().AddScore(points);
            return;
        }

        HandleCollision(col);
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

    // Overrides

    public override void Deactivate()
    {
        alive = false;
    }

    public override void Reactivate()
    {
        alive = true;

        spriteR = gameObject.GetComponent<SpriteRenderer>();

        spriteR.sprite = normalSprite;
        if (gameObject.TryGetComponent<Collider2D>(out var collider))
        {
            collider.enabled = true;
        }

        RestartShotClock();
    }
}
