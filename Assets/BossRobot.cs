using UnityEngine;

public class BossRobot : MonoBehaviour
{
    public BossMissile missilePrefab;
    public GameObject targetObject;
    public float launchIntervalSec = 2.0f;
    public float missileStartOffsetZ = 0.3f;

    float launchCooldown = 0.0f;
    BossMissile[] standbyMissiles = new BossMissile[2];

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
        var missileIndex = 0;
        var missile = standbyMissiles[missileIndex];
        if (missile != null && missile.ReadyToLaunch())
        {
            missile.Launch();
            standbyMissiles[missileIndex] = null;
            Debug.Log("Missile launched!");
        }

        var missileTransform = GetLauncherTransform();
        var missileStartPosition = missileTransform.position + new Vector3(0f, 0f, missileStartOffsetZ);

        var newMissile = Instantiate(missilePrefab, missileStartPosition, Quaternion.identity, missileTransform);
        newMissile.targetObject = targetObject;
        standbyMissiles[missileIndex] = newMissile;

        Debug.Log("Missile loaded!");
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
