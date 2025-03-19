using UnityEngine;

public class SearchLight : ManagedObject
{
    public float maxTimeSec = 3f;
    public float minTimeSec = 0.4f;
    public float maxAngularSpeed = 10f;
    public float maxTiltAngle = 120f;
    public float minTiltAngle = 60f;
    float angularSpeedX;
    float angularSpeedY;
    float timeToChangeX;
    float timeToChangeY;
    float tiltAngle;
    float spinAngle;
    GameObject inner;

    void Start()
    {
        tiltAngle = (maxTiltAngle - minTiltAngle) / 2;
        inner = transform.GetChild(0).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        timeToChangeX -= Time.deltaTime;
        timeToChangeY -= Time.deltaTime;
        if(timeToChangeX <= 0)
        {
            timeToChangeX = Random.Range(minTimeSec, maxTimeSec);
            angularSpeedX = Random.Range(-maxAngularSpeed, maxAngularSpeed);
        }
        if(timeToChangeY <= 0)
        {
            timeToChangeY = Random.Range(minTimeSec, maxTimeSec);
            angularSpeedY = Random.Range(-maxAngularSpeed, maxAngularSpeed);
        }

        tiltAngle += angularSpeedX * Time.deltaTime;
        if (tiltAngle > maxTiltAngle || tiltAngle < minTiltAngle)
        {
            angularSpeedX = -angularSpeedX;
            tiltAngle = Mathf.Clamp(tiltAngle, minTiltAngle, maxTiltAngle);
        }

        spinAngle += angularSpeedY * Time.deltaTime;

        inner.transform.rotation = Quaternion.Euler(tiltAngle, spinAngle, 0);
    }
}
