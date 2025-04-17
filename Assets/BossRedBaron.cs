using UnityEngine;

public class BossRedBaron : MonoBehaviour
{
    public float moveDelayMaxSec = 4.5f;
    public float moveDelayMinSec = 1.5f;
    public float offsetMaxX = 2.5f;

    float moveCooldown = 0.0f;
    Vector3 targetLocalPosition;
    Vector3 startLocalPosition;
    KineticSystem kineticSystemX;
    PidController positionControllerX;
    PidController angleControllerX;

    void ResetMoveCooldown()
    {
        //destinationOffset = GetRandomOffset();
        targetLocalPosition = startLocalPosition + new Vector3(
            Random.Range(-offsetMaxX, offsetMaxX),
            0f, // TEMP
            0f  // TEMP
        );
        positionControllerX.SetTarget(targetLocalPosition.x);
        moveCooldown = Random.Range(moveDelayMinSec, moveDelayMaxSec);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startLocalPosition = transform.localPosition;

        kineticSystemX = new KineticSystem(0.5f, 1f, 10f);
        positionControllerX = new PidController(2.5f, 0f, 1.2f, (float)System.Math.PI / 4f);
        angleControllerX = new PidController(350f, 0f, 15f, 100f);

        ResetMoveCooldown();        
    }

    // Update is called once per frame
    void Update()
    {
        moveCooldown -= Time.deltaTime;
        if (moveCooldown <= 0.0f)
        {
            ResetMoveCooldown();
            
        }

        float currentPositionMeters = kineticSystemX.PositionMeters;
        float targetAngle = positionControllerX.Control(currentPositionMeters, Time.deltaTime);
        
        angleControllerX.SetTarget(targetAngle);
        float currentAngle = kineticSystemX.AngleRad;
        float torque = angleControllerX.Control(currentAngle, Time.deltaTime);

        kineticSystemX.SimulateByTorque(Time.deltaTime, torque);

        transform.localPosition = new Vector3(
            kineticSystemX.PositionMeters,
            transform.localPosition.y,
            transform.localPosition.z
        );

        transform.localRotation = Quaternion.Euler(
            0f,
            0f,
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
