using System.Globalization;
using System.Linq;
using UnityEngine;

public class BossRobot : MonoBehaviour
{
    enum BossRobotStage
    {
        APPROACHING,
        FIGHTING,
        DEFEATED
    }

    class LauncherState
    {
        public GameObject missileLauncher;
        public BossMissile standbyMissile;
    }

    public BossMissile missilePrefab;
    public GameObject targetObject;
    public float launchIntervalSec = 2.0f;
    public float missileStartOffsetZ = 0.3f;
    public float fightDistance = 2.0f;
    public float approachDistance = 8.0f;
    public float approachSpeed = 1f;
    public float maxOffsetZ = 0.8f;
    public float minOffsetZ = -0.4f;
    public float minOffsetX = -0.7f;
    public float maxOffsetX = 0.7f;
    public float minOffsetY = 0.12f;
    public float maxOffsetY = 1.2f;
    public float moveDelayMaxSec = 4.5f;
    public float moveDelayMinSec = 1.5f;    

    GameObject refObject;
    BossRobotStage stage = BossRobotStage.APPROACHING;
    float launchCooldown = 0.0f;
    float moveCooldown = 0.0f;
    
    //BossMissile[] standbyMissiles = new BossMissile[2];
    LauncherState[] launchers = new LauncherState[2];
    int nextMissileIndex = 0;

    Vector3 GetRandomOffset()
    {
        float offsetX = Random.Range(minOffsetX, maxOffsetX);
        float offsetY = Random.Range(minOffsetY, maxOffsetY);
        float offsetZ = Random.Range(minOffsetZ, maxOffsetZ) + fightDistance;
        return new Vector3(offsetX, offsetY, offsetZ);
    }

    void ResetMoveCooldown()
    {
        moveCooldown = Random.Range(moveDelayMinSec, moveDelayMaxSec);
    }

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

    int GetNextLauncherIndex()
    {
        return (nextMissileIndex + 1) % launchers.Length;
    }

    void LoadMissile(LauncherState launcherState)
    {
        if (launcherState == null)
        {
            Debug.LogWarning("Attempt to load missile in missing launcher!");
            return;
        }

        var launcherTransform = launcherState.missileLauncher.transform;
        var missileStartPosition = launcherTransform.position + new Vector3(0f, 0f, missileStartOffsetZ);

        var newMissile = Instantiate(missilePrefab, missileStartPosition, Quaternion.identity, launcherTransform);
        newMissile.targetObject = targetObject;
        newMissile.SetDestroyedInLauncherCallback(MissileDestroyedInLauncherCallback);
        launcherState.standbyMissile = newMissile;

        Debug.Log($"Missile loaded! {nextMissileIndex}");
    }

    /*void LoadMissile(int launcherIndex)
    {
        var launcherState = launchers[launcherIndex];
        LoadMissile(launcherState);
    }*/

    void LaunchMissile()
    {
        var launcherState = launchers[nextMissileIndex];
        for (int i = 0; launcherState == null && i < launchers.Length; ++i)
        {
            nextMissileIndex = GetNextLauncherIndex();
            launcherState = launchers[nextMissileIndex];
        }

        if (launcherState == null)
        {
            Debug.LogWarning("Attempt to launch missile with no available launchers!");
            return;
        }

        var missile = launcherState.standbyMissile;
        if (missile != null && missile.ReadyToLaunch())
        {
            missile.Launch();
            launcherState.standbyMissile = null;
            nextMissileIndex = GetNextLauncherIndex();
            Debug.Log("Missile launched!");
        }

        LoadMissile(launcherState);
    }

    void MissileDestroyedInLauncherCallback(GameObject launcher)
    {
        Debug.Log($"Missile destroyed in launcher! {launcher.name}");
        int i = 0;
        for (; i < launchers.Length; ++i)
        {
            var candidateLauncherState = launchers[i];
            if (candidateLauncherState?.missileLauncher == launcher)
            {
                // todo: effects
                Destroy(candidateLauncherState.missileLauncher);
                launchers[i] = null;
                break;
            }
        }

        if (launchers.Count(l => l != null) == 0)
        {
            // All launchers are destroyed
            Defeat();    
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        refObject = transform.parent.gameObject;
        var tmpPosition = refObject.transform.position;
        tmpPosition.z += approachDistance;
        transform.position = tmpPosition;

        for (int i = 0; i < launchers.Length; ++i)
        {
            launchers[i] = new LauncherState();
            launchers[i].missileLauncher = GetLauncherTransform(i).gameObject;
            launchers[i].standbyMissile = null;
        }
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
                    foreach (var launcherState in launchers)
                    {
                        LoadMissile(launcherState);
                    }
                    ResetMoveCooldown();
                }            
            }
            return;
        }

        if (stage != BossRobotStage.FIGHTING)
        {
            return;
        }

        launchCooldown -= Time.deltaTime;
        if (launchCooldown <= 0.0f)
        {
            LaunchMissile();
            launchCooldown = launchIntervalSec;
        }

        moveCooldown -= Time.deltaTime;
        if (moveCooldown <= 0.0f)
        {
            var offset = GetRandomOffset();
            transform.position = refObject.transform.position + offset;
            ResetMoveCooldown();
        }
    }

    void Defeat()
    {
        stage = BossRobotStage.DEFEATED;
        Debug.Log("Boss Robot is defeated!");

        var gameState = GameState.GetInstance();
        gameState.ReportBossDefeated();
        gameState.ReportEvent(GameEvent.BIG_DETONATION);
        gameState.ReportEvent(GameEvent.BIG_BANG);

        // todo: add destruction animation
        // Destroy the boss robot after a delay to allow for animation
        // Destroy(gameObject, 2.0f);
        // For now, just destroy it immediately        
        Destroy(gameObject);
    }


    void OnTriggerEnter(Collider col)
    {
        //Debug.Log($"jjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjj Boss Hit by {col.name}");
        if (!col.name.StartsWith("bullet", true, CultureInfo.InvariantCulture))
        {
            return;
        }

        // todo: hit effect, sound and visuals
    }
}
