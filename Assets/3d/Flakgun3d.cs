using UnityEngine;

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
        timeToShoot = Random.Range(0f, 2 * avgTimeToShootSeconds);
    }

    void Shoot()
    {
        var gameObject = Instantiate(flackProjectilePrefab, transform.position, Quaternion.identity);
        var flakProjectile = InterfaceHelper.GetInterface<FlakProjectile3d>(gameObject);
        var projectileVelocity = new Vector3(
            Random.Range(-projectileSpeedMax, projectileSpeedMax),
            Random.Range(projectileSpeedMin, projectileSpeedMax),
            Random.Range(-projectileSpeedMax, projectileSpeedMax));
        flakProjectile.Initialize(transform.position, projectileVelocity);
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
    }
}
