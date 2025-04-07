using System;
using System.Globalization;
using System.Xml;
using UnityEngine;

public enum MissileStage
{
    PRELAUNCH,
    STANDBY,
    EXITINGLAUNCHER,
    FLIGHT
}

public class BossMissile : MonoBehaviour
{
    public GameObject targetObject;
    public float speedStage1 = 0.05f;
    public float speedStage2 = 0.5f;
    public float speedStage3 = 1.0f;
    public float homingSpeed = 1.0f;
    public float zDistanceMaxStage1 = 0.1f;
    public float zDistanceMaxStage2 = 0.5f;
    public float zDistanceMaxStage3 = 1.5f;
    static readonly int maxHealth = 4;
    int health = maxHealth;
    MissileStage stage = MissileStage.PRELAUNCH;
    Vector3 startPosition;
    float zDistanceTravelled;
    Action<GameObject> DestroyedInLauncherCallback;
    Transform flightParentTransform;

    public void Launch(Transform flightParent)
    {
        if (stage != MissileStage.STANDBY)
        {
            Debug.LogWarning("Missile is not in standby state!");
            return;
        }
        flightParentTransform = flightParent;
        stage = MissileStage.EXITINGLAUNCHER;
        //Debug.Log("Missile launched!");
    }

    public bool ReadyToLaunch()
    {
        return stage == MissileStage.STANDBY;
    }

    public void SetDestroyedInLauncherCallback(Action<GameObject> callback)
    {
        DestroyedInLauncherCallback = callback;
    }

    public void Explode()
    {
        transform.GetChild(0).gameObject.SetActive(false); // model
        transform.GetChild(1).gameObject.SetActive(true); // explosion effect
        Destroy(gameObject, 2.0f); // destroy after 2 seconds to allow explosion effect to play
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPosition = transform.localPosition;
        zDistanceTravelled = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        //Temp
        if(targetObject == null)
        {
            Debug.LogWarning("Target object is not assigned!");
            Destroy(gameObject);
            return;
        }

        if (stage == MissileStage.PRELAUNCH)
        {
            zDistanceTravelled += speedStage1 * Time.deltaTime;
            transform.localPosition = startPosition + new Vector3(0f, 0f, -zDistanceTravelled);
            
            if (zDistanceTravelled > zDistanceMaxStage1)
            {
                stage = MissileStage.STANDBY;
                //Debug.Log("Missile in standby");
            }
        }
        else if (stage == MissileStage.EXITINGLAUNCHER)
        {
            zDistanceTravelled += speedStage2 * Time.deltaTime;
            transform.localPosition = startPosition + new Vector3(0f, 0f, -zDistanceTravelled);
            
            if (zDistanceTravelled > zDistanceMaxStage2)
            {
                stage = MissileStage.FLIGHT;
                transform.parent = flightParentTransform;
                //Debug.Log("Missile taking flight");
            }
        }
        else if (stage == MissileStage.FLIGHT)
        {
            var tmp = Vector2.MoveTowards(transform.position, targetObject.transform.position, homingSpeed * Time.deltaTime);
            transform.position = new Vector3(tmp.x, tmp.y, transform.position.z - speedStage3 * Time.deltaTime);
            if ((targetObject.transform.position.z - transform.position.z) > zDistanceMaxStage3)
            {
                Destroy(gameObject);
                //Debug.Log("Missile out of range!");
            }
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (!col.name.StartsWith("bullet", true, CultureInfo.InvariantCulture))
        {
            return;
        }

        // todo: hit effect, sound and visuals

        --health;
        if (health > 0)
        {
            return;
        }

        var gameState = GameState.GetInstance();
        gameState.ReportEvent(GameEvent.SMALL_DETONATION);
        gameState.ReportEvent(GameEvent.SMALL_BANG);

        Explode();
        
        if(stage != MissileStage.FLIGHT)
        {
            if (DestroyedInLauncherCallback != null)
            {
                DestroyedInLauncherCallback(transform.parent.gameObject);
            }
            else
            {
                Debug.LogWarning("Missile was destroyed in launcher, but no callback was set!");
            }
        }
    }
}
