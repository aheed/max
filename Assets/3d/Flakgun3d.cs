using System;
using UnityEngine;
using UnityEngine.Animations;

public class Flakgun3d : MonoBehaviour
{
    public GameObject flackProjectilePrefab;
    public float avgTimeToShootSeconds = 5.0f;
    public float projectileSpeedMax = 5.0f;
    public float projectileSpeedMin = 1.0f;
    float timeToShoot = -1.0f;
    private SpriteRenderer spriteR;
    private bool alive = true;


    void RestartShotClock()
    {
        timeToShoot = UnityEngine.Random.Range(0f, 2 * avgTimeToShootSeconds);
    }

    void Shoot()
    {
        var gameObject = Instantiate(flackProjectilePrefab, transform.position, Quaternion.identity);
        var flakProjectile = InterfaceHelper.GetInterface<FlakProjectile3d>(gameObject);
        /*var projectileVelocity = new Vector3(
            Random.Range(-projectileSpeedMax, projectileSpeedMax),
            Random.Range(projectileSpeedMin, projectileSpeedMax),
            Random.Range(-projectileSpeedMax, projectileSpeedMax));*/

        /*var a = GameState.GetInstance().playerPosition;        
        //var v1 = GameState.GetInstance().GetStateContents().speed;
        var v1 = GameState.GetInstance().maxSpeed;
        var v2 = projectileSpeedMax;        
        var c = transform.position;

        var q = v2 / v1;
        var d = (q * a - c) / (q - 1);

        var trajectory = d - c;
        var t = trajectory.magnitude / v2;
        var projectileVelocity = trajectory / t;

        flakProjectile.Initialize(c, projectileVelocity, t);*/

        var a = GameState.GetInstance().playerPosition;        
        var v1 = GameState.GetInstance().maxSpeed;
        var v2 = projectileSpeedMax;
        var c = transform.position;
        var q = v2 / v1;
        var zImpact = (q * a.z - c.z) / (q - 1);
        var zDistance = zImpact - c.z;
        var t = Math.Abs(zDistance) / v2;
        var t2 = Math.Abs(zImpact - a.z) / v1;
        var zSpeed = zDistance / t;
        var projectileVelocity = new Vector3(0, 0, zSpeed);
        //flakProjectile.Initialize(c, projectileVelocity, t, zImpact);

        ///
        var zImpact2 = a.z + v1 * t2;
        var zImpact3 = c.z + v2 * t;
        //Debug.Log($"zImpact: {zImpact}, zImpact2: {zImpact2}, zImpact3: {zImpact3} v:{projectileVelocity}");
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
    }*/

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

        //TEMP
        var a = GameState.GetInstance().playerPosition;        
        transform.GetChild(0).GetChild(0).LookAt(a);
    }
}
