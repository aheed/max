using System;
using UnityEngine;
using UnityEngine.Animations;

public class Flakgun3d : ManagedObject
{
    public GameObject flackProjectilePrefab;
    public float avgTimeToShootSeconds = 5.0f;
    public float projectileSpeedMax = 5.0f;
    public float projectileSpeedMin = 1.0f;
    float timeToShoot = -1.0f;
    private bool alive = true;


    void RestartShotClock()
    {
        timeToShoot = UnityEngine.Random.Range(0f, 2 * avgTimeToShootSeconds);
    }

    void Shoot()
    {
        var gameObject = Instantiate(flackProjectilePrefab, transform.position, Quaternion.identity);
        var flakProjectile = InterfaceHelper.GetInterface<FlakProjectile3d>(gameObject);

        flakProjectile.Initialize(
            transform.position,
            GameState.GetInstance().playerPosition,
            new Vector3(0, 0, GameState.GetInstance().maxSpeed));
    }

    /*void HandleCollision(Collider2D col)
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
            GameState.GetInstance().BombLanded(bomb, gameObject);
            return;
        }

        HandleCollision(col);
    }*/

    // Update is called once per frame
    void Update()
    {
        timeToShoot -= Time.deltaTime;
        if (timeToShoot < 0f && alive)
        {
            Shoot();
            RestartShotClock();
        }

        //TEMP
        var a = GameState.GetInstance().playerPosition;        
        transform.GetChild(0).GetChild(0).LookAt(a);
    }

    // Overrides
    public override void Deactivate()
    {
        alive = false;
    }

    public override void Reactivate()
    {
        alive = true;
        RestartShotClock();
    }
}
