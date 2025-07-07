using System;
using System.Globalization;
using Unity.VisualScripting;
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
    private bool demolished = false;
    static readonly int points = 10;


    void RestartShotClock()
    {
        timeToShoot = UnityEngine.Random.Range(0f, 2 * avgTimeToShootSeconds);
    }

    void Shoot()
    {
        if(!FlakProjectile3d.IsWithinRange(
            transform.position,
            GameState.GetInstance().playerPosition,
            new Vector3(0, 0, GameState.GetInstance().maxSpeed)))
        {
            //Debug.Log("Flakgun3d: Player out of range");
            return;
        }

        var gameObject = Instantiate(flackProjectilePrefab, transform.position, Quaternion.identity);
        var flakProjectile = InterfaceHelper.GetInterface<FlakProjectile3d>(gameObject);

        flakProjectile.Initialize(
            transform.position,
            GameState.GetInstance().playerPosition,
            new Vector3(0, 0, GameState.GetInstance().maxSpeed));
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.name.StartsWith("Bomb"))
        {
            Demolish();
            GameState.GetInstance().BombLanded(col.gameObject, gameObject);
            GameState.GetInstance().AddScore(points);
        }
        else if (col.name.StartsWith("bullet", true, CultureInfo.InvariantCulture))
        {
            Demolish();
            var gameState = GameState.GetInstance();
            gameState.ReportEvent(GameEvent.SMALL_DETONATION);
            gameState.ReportEvent(GameEvent.SMALL_BANG);
            GameState.GetInstance().AddScore(points);
        }
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

    GameObject GetHealthyModel()
    {
        return transform.GetChild(0).gameObject;
    }

    GameObject GetDemolishedModel()
    {
        return transform.GetChild(1).gameObject;
    }

    void Demolish()
    {
        if (demolished)
        {
            return;
        }

        demolished = true;
        alive = false;

        GetHealthyModel().SetActive(false);
        GetDemolishedModel().SetActive(true);

        var collider = gameObject.GetComponent<BoxCollider>();
        if (collider != null)
        {
            collider.enabled = false;
        }
    }


    // Overrides
    public override void Deactivate()
    {
        alive = false;
    }

    public override void Reactivate()
    {
        if (demolished)
        {
            demolished = false;
            GetHealthyModel().SetActive(true);
            GetDemolishedModel().SetActive(false);
            var collider = gameObject.GetComponent<BoxCollider>();
            if (collider != null)
            {
                collider.enabled = true;
            }
        }

        alive = true;
        RestartShotClock();
    }
}
