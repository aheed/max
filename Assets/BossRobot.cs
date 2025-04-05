using System.Globalization;
using UnityEngine;

public class BossRobot : MonoBehaviour
{
    enum BossRobotStage
    {
        APPROACHING,
        FIGHTING,
        DEFEATED
    }

    public BossMissile missilePrefab;
    public GameObject targetObject;
    public float launchIntervalSec = 2.0f;
    public float missileStartOffsetZ = 0.3f;
    public float fightDistance = 2.0f;
    public float approachDistance = 8.0f;
    public float approachSpeed = 1f;

    GameObject refObject;
    BossRobotStage stage = BossRobotStage.APPROACHING;
    float launchCooldown = 0.0f;
    BossMissile[] standbyMissiles = new BossMissile[2];
    int nextMissileIndex = 0;

    

    Transform GetLauncherTransform(int missileIndex)
    {
        // Logic to get the position of the missile launcher
        // For example, return the position of a child object named "MissileLauncher"
        Transform missileLauncher = transform.GetChild(2 + missileIndex);
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
        var missile = standbyMissiles[nextMissileIndex];
        if (missile != null && missile.ReadyToLaunch())
        {
            missile.Launch();
            standbyMissiles[nextMissileIndex] = null;
            nextMissileIndex = (nextMissileIndex + 1) % standbyMissiles.Length;
            Debug.Log("Missile launched!");
        }

        var missileTransform = GetLauncherTransform(nextMissileIndex);
        var missileStartPosition = missileTransform.position + new Vector3(0f, 0f, missileStartOffsetZ);

        var newMissile = Instantiate(missilePrefab, missileStartPosition, Quaternion.identity, missileTransform);
        newMissile.targetObject = targetObject;
        standbyMissiles[nextMissileIndex] = newMissile;

        Debug.Log($"Missile loaded! {nextMissileIndex}");
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        refObject = transform.parent.gameObject;
        var tmpPosition = refObject.transform.position;
        tmpPosition.z += approachDistance;
        transform.position = tmpPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (stage == BossRobotStage.APPROACHING)
        {
            if (refObject != null)
            {
                float distance = Vector3.Distance(transform.position, refObject.transform.position);
                
                // Move towards the target
                Vector3 direction = (refObject.transform.position - transform.position).normalized;
                transform.position += direction * approachSpeed * Time.deltaTime;

                if (distance < fightDistance)
                {
                    stage = BossRobotStage.FIGHTING;
                    Debug.Log("Boss Robot is now fighting!");
                }                
            }
            return;
        }

        launchCooldown -= Time.deltaTime;
        if (launchCooldown <= 0.0f)
        {
            LaunchMissile();
            launchCooldown = launchIntervalSec;
        }
    }

    void OnTriggerEnter(Collider col)
    {
        // Temporary implementation to destroy the robot when hit by a bullet
        // This should be replaced with registering missiles hit while in laucher

        //Debug.Log($"jjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjj Boss Hit by {col.name}");
        if (!col.name.StartsWith("bullet", true, CultureInfo.InvariantCulture))
        {
            return;
        }

        Destroy(col.gameObject);
        var gameState = GameState.GetInstance();
        gameState.GetStateContents().bossDefeated = true;
        gameState.ReportEvent(GameEvent.BIG_DETONATION);
        gameState.ReportEvent(GameEvent.BIG_BANG);
        gameState.TargetHit();
        Destroy(gameObject);
    }
}
