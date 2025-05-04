using UnityEngine;
using System;

enum IntroControllerStage 
{
    PRE_START,
    ACCELERATING,
    TAKE_OFF,
    FIRE_DEMO,
    BOMB_DEMO,
    ENEMY_APPROACHING,
    ENEMY_SITTING_DUCK,
    ENEMY_RIGHT_ALTITUDE,
    BOMB_BUILDING,
    LANDING,
    CRASHED,
    FINISHED
}

class CallbackSpec
{
    public GameEvent gameEvent;
    public Action action;
}

public class IntroController : MonoBehaviour
{
    public EnemyPlane3d targetPlanePrefab;

    IntroControllerStage stage = IntroControllerStage.PRE_START;
    CallbackSpec[] callbacks;
    EnemyPlane3d targetPlane;

    void RegisterCallbacks()
    {
        var gameState = GameState.GetInstance();
        foreach (var cb in callbacks)
        {
            gameState.Subscribe(cb.gameEvent, cb.action);
        }
    }

    void UnregisterCallbacks()
    {
        var gameState = GameState.GetInstance();
        if (gameState == null)
        {
            return;
        }

        foreach (var cb in callbacks)
        {
            gameState.Unsubscribe(cb.gameEvent, cb.action);
        }
    }

    void Start()
    {
        Debug.Log("IntroController.StartLevel");
        callbacks = new CallbackSpec[] 
        {
            new() { gameEvent = GameEvent.DEBUG_ACTION2, action = OnDebugAction2 },
            new() { gameEvent = GameEvent.ALT_CHANGED, action = OnAltitudeChanged },
            new() { gameEvent = GameEvent.TARGET_HIT, action = OnTargetHitCallback }
        };
        RegisterCallbacks();
    }

    void SpawnTargetPlane()
    {
        var targetPlanePosition = transform.parent.position;
        targetPlanePosition.y = GameState.GetInstance().maxAltitude / 3;
        targetPlane = Instantiate(targetPlanePrefab, targetPlanePosition, Quaternion.identity);
        //targetPlane = Instantiate(targetPlanePrefab);
        targetPlane.refObject = transform.parent;
        targetPlane.SetNavigator(new IntroLevelEnemyPlaneNavigator(targetPlane));
        targetPlane.SetVip();
    }

    void OnDebugAction2()
    {
        // spawn enemy plane
        Debug.Log("IntroController.OnDebugAction2");
        SpawnTargetPlane();
    }

    void OnAltitudeChanged()
    {
        //Debug.Log("IntroController.OnAltitudeChanged");
        var gameState = GameState.GetInstance();
        if (stage == IntroControllerStage.TAKE_OFF)
        {
            var altitude = gameState.GetStateContents().altitude;
            if (altitude > gameState.minSafeAltitude)
            {
                stage = IntroControllerStage.FIRE_DEMO;
                Debug.Log("------> Fire your machine gun");
            }
        }
        else if (stage == IntroControllerStage.ENEMY_SITTING_DUCK &&
            gameState.AnyEnemyPlaneAtCollisionAltitude())
        {
           stage = IntroControllerStage.ENEMY_RIGHT_ALTITUDE;
           Debug.Log("------> Blue dashboard indicates presence of an enemy plane at your altitude");
        }
    }

    void OnTargetHitCallback()
    {
        Debug.Log("IntroLevelController.OnTargetHitCallback");
        GameState.GetInstance().ReportBossDefeated();
        stage = IntroControllerStage.LANDING;
        Debug.Log("------> Victory! Now land the plane");
    }

    void OnDestroy()
    {
        UnregisterCallbacks();
    }
}
