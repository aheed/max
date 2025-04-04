using UnityEngine;

public enum MissileStage
{
    PRELAUNCH,
    EXITINGLAUNCHER,
    FLIGHT
}

public class BossMissile : MonoBehaviour
{
    public GameObject targetObject;
    public float speedStage1 = 0.05f;
    public float speedStage2 = 0.5f;
    public float speedStage3 = 1.0f;
    public float zDistanceMaxStage1 = 0.1f;
    public float zDistanceMaxStage2 = 0.5f;
    public float zDistanceMaxStage3 = 1.5f;
    MissileStage stage = MissileStage.PRELAUNCH;
    Vector3 startPosition;
    float zDistanceTravelled;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //startPosition = transform.position;
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
            //transform.position += new Vector3(0f, 0f, -speedStage1 * Time.deltaTime);
            //transform.localPosition = new Vector3(0f, 0f, -zDistanceTravelled);
            //transform.position = startPosition + new Vector3(0f, 0f, -zDistanceTravelled);
            transform.localPosition = startPosition + new Vector3(0f, 0f, -zDistanceTravelled);
            
            if (zDistanceTravelled > zDistanceMaxStage1)
            {
                stage = MissileStage.EXITINGLAUNCHER;
                Debug.Log("Missile exiting launcher");
            }
        }
        else if (stage == MissileStage.EXITINGLAUNCHER)
        {
            zDistanceTravelled += speedStage2 * Time.deltaTime;
            //transform.localPosition = new Vector3(0f, 0f, -zDistanceTravelled);
            //transform.position = startPosition + new Vector3(0f, 0f, -zDistanceTravelled);
            transform.localPosition = startPosition + new Vector3(0f, 0f, -zDistanceTravelled);
            
            if (zDistanceTravelled > zDistanceMaxStage2)
            {
                stage = MissileStage.FLIGHT;
                Debug.Log("Missile taking flight");
            }
        }
        else if (stage == MissileStage.FLIGHT)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetObject.transform.position, speedStage3 * Time.deltaTime);
            if ((targetObject.transform.position.z - transform.position.z) > zDistanceMaxStage3)
            {
                Destroy(gameObject);
                Debug.Log("Missile out of range!");
            }
        }
    }
}
