using System.Globalization;
using UnityEngine;

enum ManeuverType
{
    NONE = 0,
    ROLL_LEFT,
    ROLL_RIGHT,
    LOOP,
    NOF_MANEUVER_TYPES
}

public class BossRedBaron : MonoBehaviour
{
    public float moveDelayMaxSec = 4.5f;
    public float moveDelayMinSec = 1.5f;
    public float maneuverDelayMaxSec = 8.5f;
    public float maneuverDelayMinSec = 2.5f;
    public float rollDurationSec = 1.5f;
    public float loopDurationSec = 2.5f;
    public float loopOffsetY = 0.5f;
    public float loopOffsetZ = 0.5f;
    public float offsetMaxX = 2.5f;
    public float offsetMaxY = 1.5f;
    public float offsetMinY = -1.5f;
    public float yawFactor = 10f;
    public float healthBarOffsetY = 1f;
    public int maxHealth = 10;
    public Vector3 midPosition;

    HealthBar healthBar;
    int health;
    float maneuverDurationSec;
    float moveCooldownX = 0.0f;
    float moveCooldownY = 0.0f;
    float maneuverCooldown;
    float maneuverElapsedSec;
    float maneuverRateRadPerSec;
    float maneuverAngleRad;
    float maneuverRollRad;
    float maneuverPitchRad;
    float maneuverOffsetY;
    float maneuverOffsetZ;
    ManeuverType maneuverType = ManeuverType.NONE;
    KineticSystem kineticSystemX;
    KineticSystem kineticSystemY;
    PidController positionControllerX;
    PidController positionControllerY;
    PidController angleControllerX;
    PidController angleControllerY;
    static readonly int points = 10000;

    void ResetMoveCooldownX()
    {
        if (!IsManeuvering())
        {
            var targetX = Random.Range(-offsetMaxX, offsetMaxX);
            positionControllerX.SetTarget(targetX);
        }
        moveCooldownX = Random.Range(moveDelayMinSec, moveDelayMaxSec);
    }

    void ResetMoveCooldownY()
    {
        var targetY = Random.Range(offsetMinY, offsetMaxY);
        positionControllerY.SetTarget(targetY);
        moveCooldownY = Random.Range(moveDelayMinSec, moveDelayMaxSec);
    }

    void ResetManeuverCooldown()
    {
        maneuverCooldown = Random.Range(maneuverDelayMinSec, maneuverDelayMaxSec);
        maneuverElapsedSec = 0f;
        maneuverAngleRad = 0f;
        maneuverType = (ManeuverType)Random.Range(1, (int)ManeuverType.NOF_MANEUVER_TYPES);
        if (maneuverType == ManeuverType.ROLL_LEFT || maneuverType == ManeuverType.ROLL_RIGHT)
        {
            maneuverDurationSec = rollDurationSec;
        }
        else if (maneuverType == ManeuverType.LOOP)
        {
            maneuverDurationSec = loopDurationSec;
        }
        else
        {
            maneuverDurationSec = 0f;
        }
        maneuverRateRadPerSec = (float)System.Math.PI * 2 / maneuverDurationSec;
        //Debug.Log($"Starting maneuver    Maneuver type: {maneuverType}");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var startX = transform.localPosition.x - midPosition.x;
        kineticSystemX = new KineticSystem(0.5f, 1f, 10f, startX, 100f, -100f);
        var startY = transform.localPosition.y - midPosition.y;
        kineticSystemY = new KineticSystem(0.5f, 1f, 10f, startY, 100f, offsetMinY - 0.1f);
        positionControllerX = new PidController(2.5f, 0f, 1.2f, (float)System.Math.PI / 4f);
        positionControllerY = new PidController(0.5f, 0f, 0.5f, (float)System.Math.PI / 4f);
        angleControllerX = new PidController(350f, 0f, 15f, 100f);
        angleControllerY = new PidController(150f, 0f, 10f, 100f);

        ResetMoveCooldownX();
        ResetMoveCooldownY();
        ResetManeuverCooldown();
        maneuverType = ManeuverType.NONE;
        maneuverElapsedSec = 100f;
        maneuverRollRad = 0f;
        maneuverPitchRad = 0f;
        maneuverAngleRad = 0f;
        maneuverOffsetY = 0f;
        maneuverOffsetZ = 0f;

        health = maxHealth;
        healthBar = GetComponentInChildren<HealthBar>();
        if (healthBar == null)
        {
            Debug.LogError("HealthBar component not found!");
        }
        GameState.GetInstance().Subscribe(GameEvent.DEBUG_ACTION1, OnDebugCallback1);
    }

    bool IsManeuvering()
    {
        return maneuverType != ManeuverType.NONE;
    }

    // Update is called once per frame
    void Update()
    {
        moveCooldownX -= Time.deltaTime;
        if (moveCooldownX <= 0.0f)
        {
            ResetMoveCooldownX();
        }

        moveCooldownY -= Time.deltaTime;
        if (moveCooldownY <= 0.0f)
        {
            ResetMoveCooldownY();
        }

        maneuverCooldown -= Time.deltaTime;
        if (maneuverCooldown <= 0.0f)
        {            
            ResetManeuverCooldown();
        }

        if (maneuverElapsedSec < maneuverDurationSec)
        {
            maneuverElapsedSec += Time.deltaTime;
            maneuverAngleRad = maneuverRateRadPerSec * maneuverElapsedSec;

            if (maneuverElapsedSec > maneuverDurationSec)
            {
                maneuverAngleRad = 0f;
                maneuverRollRad = 0f;
                maneuverPitchRad = 0f;
                maneuverOffsetY = 0f;
                maneuverOffsetZ = 0f;
                maneuverType = ManeuverType.NONE;
                //Debug.Log("Maneuver completed");
            }
            else if (maneuverType == ManeuverType.ROLL_LEFT)
            {
                maneuverRollRad = -maneuverAngleRad;
                maneuverPitchRad = 0f;
            }
            else if (maneuverType == ManeuverType.ROLL_RIGHT)
            {
                maneuverRollRad = maneuverAngleRad;
                maneuverPitchRad = 0f;
            }
            else if (maneuverType == ManeuverType.LOOP)
            {
                maneuverRollRad = 0f;
                maneuverPitchRad = maneuverAngleRad;
                maneuverOffsetY = Mathf.Sin(maneuverAngleRad - ((float)System.Math.PI / 2)) * loopOffsetY + loopOffsetY;
                maneuverOffsetZ = -(Mathf.Sin(maneuverAngleRad - ((float)System.Math.PI / 2)) * loopOffsetZ + loopOffsetZ);
            }
        }

        float currentPositionMeters = kineticSystemX.PositionMeters;
        float targetAngle = positionControllerX.Control(currentPositionMeters, Time.deltaTime);

        angleControllerX.SetTarget(targetAngle);
        float currentAngle = kineticSystemX.AngleRad;
        float torque = angleControllerX.Control(currentAngle, Time.deltaTime);

        kineticSystemX.SimulateByTorque(Time.deltaTime, torque);

        float targetAngleY = positionControllerY.Control(kineticSystemY.PositionMeters, Time.deltaTime);
        angleControllerY.SetTarget(targetAngleY);
        float currentAngleY = kineticSystemY.AngleRad;
        float torqueY = angleControllerY.Control(currentAngleY, Time.deltaTime);
        kineticSystemY.SimulateByTorque(Time.deltaTime, torqueY);
        //kineticSystemY.SimulateByAngle(Time.deltaTime, targetAngleY);

        transform.localPosition = new Vector3(
            midPosition.x + kineticSystemX.PositionMeters,
            midPosition.y + kineticSystemY.PositionMeters + maneuverOffsetY,
            midPosition.z + maneuverOffsetZ
        );

        transform.localRotation = Quaternion.Euler(
            Mathf.Rad2Deg * -(kineticSystemY.AngleRad + maneuverPitchRad),
            kineticSystemX.VelocityMetersPerSecond * yawFactor,
            Mathf.Rad2Deg * -(kineticSystemX.AngleRad + maneuverRollRad)
        );

        /*transform.localPosition = Vector3.MoveTowards(
                transform.localPosition,
                targetLocalPosition,
                Time.deltaTime * 2.0f
            );
        */
    }

    void TakeHit()
    {
        --health;

        healthBar.SetHealth(health, maxHealth);

        if (health > 0)
        {
            return;
        }

        var gameState = GameState.GetInstance();
        gameState.ReportEvent(GameEvent.BIG_DETONATION);
        gameState.ReportEvent(GameEvent.BIG_BANG);
        gameState.ReportBossDefeated();
        gameState.AddScore(points);

        Explode();
    }

    void OnTriggerEnter(Collider col)
    {
        Debug.Log($"Red baron collided with                 {col.gameObject.name}");

        if (!col.name.StartsWith("bullet", true, CultureInfo.InvariantCulture))
        {
            return;
        }

        TakeHit();
    }

    private void OnDebugCallback1()
    {
        //Fake a hit
        TakeHit();
    }

    void Explode()
    {
        Debug.Log("Red baron exploded !!!!!!!!!!!! ****************");
        transform.GetChild(0).gameObject.SetActive(false); // model
        transform.GetChild(1).gameObject.SetActive(true); // explosion effect
        var Collider = GetComponent<Collider>();
        if (Collider != null)
        {
            Collider.enabled = false;
        }
        Destroy(gameObject, 2.0f); // destroy after 2 seconds to allow explosion effect to play
    }

    void OnDestroy()
    {
        var gameState = GameState.GetInstance();
        if (gameState == null)
        {
            return;
        }

        gameState.Unsubscribe(GameEvent.DEBUG_ACTION1, OnDebugCallback1);
    }
}
