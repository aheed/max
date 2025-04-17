using UnityEngine;

public class BossRedBaron : MonoBehaviour
{
    public float moveDelayMaxSec = 4.5f;
    public float moveDelayMinSec = 1.5f;
    public float offsetMaxX = 2.5f;
    public float offsetMaxY = 1.5f;
    public float yawFactor = 10f;

    float moveCooldownX = 0.0f;
    float moveCooldownY = 0.0f;
    Vector3 targetLocalPosition;
    Vector3 startLocalPosition;
    KineticSystem kineticSystemX;
    KineticSystem kineticSystemY;
    PidController positionControllerX;
    PidController positionControllerY;
    PidController angleControllerX;
    PidController angleControllerY;

    void ResetMoveCooldownX()
    {
        targetLocalPosition = startLocalPosition + new Vector3(
            Random.Range(-offsetMaxX, offsetMaxX),
            0f,
            0f
        );
        positionControllerX.SetTarget(targetLocalPosition.x);
        moveCooldownX = Random.Range(moveDelayMinSec, moveDelayMaxSec);
    }

    void ResetMoveCooldownY()
    {
        targetLocalPosition = startLocalPosition + new Vector3(
            0f,
            Random.Range(-offsetMaxY, offsetMaxY),
            0f
        );
        positionControllerY.SetTarget(targetLocalPosition.y);
        moveCooldownY = Random.Range(moveDelayMinSec, moveDelayMaxSec);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startLocalPosition = transform.localPosition;

        kineticSystemX = new KineticSystem(0.5f, 1f, 10f);
        kineticSystemY = new KineticSystem(0.5f, 1f, 10f);
        positionControllerX = new PidController(2.5f, 0f, 1.2f, (float)System.Math.PI / 4f);
        positionControllerY = new PidController(0.5f, 0f, 0.5f, (float)System.Math.PI / 4f);
        angleControllerX = new PidController(350f, 0f, 15f, 100f);
        angleControllerY = new PidController(150f, 0f, 10f, 100f);

        ResetMoveCooldownX();
        ResetMoveCooldownY();
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
            kineticSystemX.PositionMeters,
            kineticSystemY.PositionMeters,
            transform.localPosition.z
        );

        transform.localRotation = Quaternion.Euler(
            Mathf.Rad2Deg * -kineticSystemY.AngleRad,
            kineticSystemX.VelocityMetersPerSecond * yawFactor,
            Mathf.Rad2Deg * -kineticSystemX.AngleRad
        );

        /*transform.localPosition = Vector3.MoveTowards(
                transform.localPosition,
                targetLocalPosition,
                Time.deltaTime * 2.0f
            );
        */
    }
}
