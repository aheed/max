using System.Globalization;
using System.Linq;
using UnityEngine;

public class BossRobot : MonoBehaviour
{
    enum BossRobotStage
    {
        APPROACHING,
        FIGHTING,
        DEFEATED,
        EXPLODED
    }

    class LauncherState
    {
        public GameObject missileLauncher;
        public BossMissile standbyMissile;
    }

    public BossMissile missilePrefab;
    public GameObject hitEffectPrefab;
    public GameObject targetObject;
    public float launchIntervalSec = 2.0f;
    public float missileStartOffsetZ = 0.3f;
    public float fightDistance = 2.0f;
    public float approachDistance = 8.0f;
    public float approachSpeed = 1f;
    public float moveSpeed = 1f;
    public float moveSpeedDefeated = 3.5f;
    public float maxOffsetZ = 0.8f;
    public float minOffsetZ = -0.4f;
    public float minOffsetX = -0.7f;
    public float maxOffsetX = 0.7f;
    public float minOffsetY = 0.12f;
    public float maxOffsetY = 1.2f;
    public float moveDelayMaxSec = 4.5f;
    public float moveDelayMinSec = 1.5f;
    public float moveDelayDefeated = 0.05f;
    public float defeatedLifeSpanSec = 1.5f;
    public float hitEffectLifeSpanSec = 0.5f;
    public float hitEffectOffsetZ = 0.26f;

    GameObject refObject;
    BossRobotStage stage = BossRobotStage.APPROACHING;
    float launchCooldown = 0.0f;
    float moveCooldown = 0.0f;
    LauncherState[] launchers = new LauncherState[2];
    int nextMissileIndex = 0;
    Vector3 destinationOffset;
    float currentMoveSpeed;
    float timeToLiveSec;

    Vector3 GetRandomOffset()
    {
        float offsetX = Random.Range(minOffsetX, maxOffsetX);
        float offsetY = Random.Range(minOffsetY, maxOffsetY);
        float offsetZ = Random.Range(minOffsetZ, maxOffsetZ) + fightDistance;
        return new Vector3(offsetX, offsetY, offsetZ);
    }

    void ResetMoveCooldown()
    {
        destinationOffset = GetRandomOffset();
        moveCooldown = stage == BossRobotStage.FIGHTING ?
            Random.Range(moveDelayMinSec, moveDelayMaxSec) :
            moveDelayDefeated;
    }

    Transform GetLauncherTransform(int missileIndex)
    {
        // Logic to get the position of the missile launcher
        // For example, return the position of a child object named "MissileLauncher"
        Transform missileLauncher = transform.GetChild(1 + missileIndex);
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

        if (launcherState.standbyMissile != null)
        {
            //Debug.LogWarning("Attempt to load missile in already loaded launcher!");
            return;
        }

        var launcherTransform = launcherState.missileLauncher.transform;
        var missileStartPosition = launcherTransform.position + new Vector3(0f, 0f, missileStartOffsetZ);

        var newMissile = Instantiate(missilePrefab, missileStartPosition, Quaternion.identity, launcherTransform);
        newMissile.targetObject = targetObject;
        newMissile.SetDestroyedInLauncherCallback(MissileDestroyedInLauncherCallback);
        launcherState.standbyMissile = newMissile;

        //Debug.Log($"Missile loaded! {nextMissileIndex}");
    }

    /*void LoadMissile(int launcherIndex)
    {
        var launcherState = launchers[launcherIndex];
        LoadMissile(launcherState);
    }*/

    void LaunchMissile()
    {
        if (GameState.GetInstance().IsGameOver())
        {
            return;
        }

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
            missile.Launch(refObject.transform);
            launcherState.standbyMissile = null;
            nextMissileIndex = GetNextLauncherIndex();
            //Debug.Log("Missile launched!");
        }

        LoadMissile(launcherState);
    }

    void MissileDestroyedInLauncherCallback(GameObject launcher)
    {
        //Debug.Log($"Missile destroyed in launcher! {launcher.name}");
        int i = 0;
        for (; i < launchers.Length; ++i)
        {
            var candidateLauncherState = launchers[i];
            if (candidateLauncherState?.missileLauncher == launcher)
            {
                candidateLauncherState.missileLauncher.transform.GetChild(0).gameObject.SetActive(false);
                if (candidateLauncherState.standbyMissile != null)
                {
                    candidateLauncherState.standbyMissile.Explode();
                }
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
        tmpPosition.y += minOffsetY;
        tmpPosition.z += approachDistance;
        transform.position = tmpPosition;

        for (int i = 0; i < launchers.Length; ++i)
        {
            launchers[i] = new LauncherState();
            launchers[i].missileLauncher = GetLauncherTransform(i).gameObject;
            launchers[i].standbyMissile = null;
        }

        currentMoveSpeed = moveSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (stage == BossRobotStage.APPROACHING)
        {
            if (refObject != null)
            {
                float distance = Vector3.Distance(transform.position, refObject.transform.position);
                
                Vector3 direction = new Vector3(0f, 0f, -1f);
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

        moveCooldown -= Time.deltaTime;
        if (moveCooldown <= 0.0f)
        {
            ResetMoveCooldown();
        }

        var destination = refObject.transform.position + destinationOffset;
        transform.position = Vector3.MoveTowards(transform.position, destination, currentMoveSpeed * Time.deltaTime);

        if (stage == BossRobotStage.DEFEATED)
        {
            timeToLiveSec -= Time.deltaTime;
            if (timeToLiveSec <= 0.0f)
            {
                GameState.GetInstance().ReportEvent(GameEvent.BIG_DETONATION);
                GameState.GetInstance().ReportEvent(GameEvent.BIG_BANG);
                transform.GetChild(0).gameObject.SetActive(false); // model
                transform.GetChild(3).gameObject.SetActive(true); // explosion effect
                stage = BossRobotStage.EXPLODED;
                Destroy(gameObject, 2.0f); // destroy after 2 seconds to allow explosion effect to play
            }
            return;
        }
        else if (stage == BossRobotStage.EXPLODED)
        {
            return;
        }

        launchCooldown -= Time.deltaTime;
        if (launchCooldown <= 0.0f)
        {
            LaunchMissile();
            launchCooldown = launchIntervalSec;
        }
    }

    void Defeat()
    {
        stage = BossRobotStage.DEFEATED;
        //Debug.Log("Boss Robot is defeated!");

        var gameState = GameState.GetInstance();
        gameState.ReportBossDefeated();        

        currentMoveSpeed = moveSpeedDefeated;
        ResetMoveCooldown();
        timeToLiveSec = defeatedLifeSpanSec;
    }


    void OnTriggerEnter(Collider col)
    {
        if (!col.name.StartsWith("bullet", true, CultureInfo.InvariantCulture))
        {
            return;
        }

        // Get position of surface of the robot at approximate point of collision (z not exact)
        var contactPoint = col.transform.position;
        contactPoint.z = transform.position.z - hitEffectOffsetZ;
        var hitEffect = Instantiate(hitEffectPrefab, contactPoint, Quaternion.identity, transform);
        Destroy(hitEffect, hitEffectLifeSpanSec);

        // TEMP
        // For debug: destroy a launcher on hit
        
        /* 
        var launcherState = launchers[nextMissileIndex];
        for (int i = 0; launcherState == null && i < launchers.Length; ++i)
        {
            nextMissileIndex = GetNextLauncherIndex();
            launcherState = launchers[nextMissileIndex];
        }

        if (launcherState == null)
        {
            return;
        }

        MissileDestroyedInLauncherCallback(launcherState.missileLauncher);
        */
    }
}
