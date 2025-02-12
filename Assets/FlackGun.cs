using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlackGun : ManagedObject3, IPositionObservable
{
    public GameObject flackProjectilePrefab;
    public Sprite normalSprite;
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
            Deactivate();
            var gameState = GameState.GetInstance();
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

    // Overrides

    public override void Deactivate()
    {
        if (!alive)
        {
            return;
        }
        alive = false;

        spriteR.sprite = shotSprite;
        if (gameObject.TryGetComponent<Collider2D>(out var collider))
        {
            collider.enabled = false;
        }
    }

    public override void Reactivate()
    {
        if (alive)
        {
            return;
        }
        alive = true;

        spriteR.sprite = normalSprite;
        if (gameObject.TryGetComponent<Collider2D>(out var collider))
        {
            collider.enabled = true;
        }
    }
}
