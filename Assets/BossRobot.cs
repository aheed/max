using UnityEngine;

public class BossRobot : MonoBehaviour
{
    public BossMissile missilePrefab;
    public GameObject targetObject;
    public float launchIntervalSec = 2.0f;

    float launchCooldown = 0.0f;

    Transform GetLauncherTransform()
    {
        // Logic to get the position of the missile launcher
        // For example, return the position of a child object named "MissileLauncher"
        Transform missileLauncher = transform.GetChild(2);
        if (missileLauncher != null)
        {
            return missileLauncher;
        }
        else
        {
            Debug.LogWarning("MissileLauncher not found!");
            return transform; // Fallback to the robot's position
        }
    }
    void LaunchMissile()
    {
        var missileTransform = GetLauncherTransform();
        var missileStartPosition = transform.position + missileTransform.localPosition;
        var missileStartRotation = missileTransform.rotation;

        var missile = Instantiate(missilePrefab, missileStartPosition, missileStartRotation);
        missile.targetObject = targetObject;

        Debug.Log("Missile launched!");
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        launchCooldown -= Time.deltaTime;
        if (launchCooldown <= 0.0f)
        {
            LaunchMissile();
            launchCooldown = launchIntervalSec;
        }
    }
}
