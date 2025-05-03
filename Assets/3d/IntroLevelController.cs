using UnityEngine;
using System;

enum IntroLevelControllerStage 
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

class Callback
{
    public GameEvent gameEvent;
    public Action action;
}

public class IntroLevelController : ILevelController
{
    IntroLevelControllerStage stage = IntroLevelControllerStage.PRE_START;
    Callback[] callbacks;

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
        foreach (var cb in callbacks)
        {
            gameState.Unsubscribe(cb.gameEvent, cb.action);
        }
    }

    public void StartLevel()
    {
        Debug.Log("IntroLevelController.StartLevel");
        callbacks = new Callback[] 
        {
            new Callback { gameEvent = GameEvent.DEBUG_ACTION2, action = OnDebugAction2 },
            new Callback { gameEvent = GameEvent.ALT_CHANGED, action = OnAltitudeChanged },
            new Callback { gameEvent = GameEvent.TARGET_HIT, action = OnTargetHitCallback }
        };
        RegisterCallbacks();
    }

    void OnDebugAction2()
    {
        // spawn enemy plane

    }

    void OnAltitudeChanged()
    {
        Debug.Log("IntroLevelController.OnAltitudeChanged");
        var gameState = GameState.GetInstance();
        if (stage == IntroLevelControllerStage.TAKE_OFF)
        {
            var altitude = gameState.GetStateContents().altitude;
            if (altitude > gameState.minSafeAltitude)
            {
                stage = IntroLevelControllerStage.FIRE_DEMO;
                Debug.Log("------> Fire your machine gun");
            }
        }
        else if (stage == IntroLevelControllerStage.ENEMY_SITTING_DUCK &&
            gameState.AnyEnemyPlaneAtCollisionAltitude())
        {
           stage = IntroLevelControllerStage.ENEMY_RIGHT_ALTITUDE;
           Debug.Log("------> Blue dashboard indicates presence of an enemy plane at your altitude");
        }
    }

    void OnTargetHitCallback()
    {
        Debug.Log("IntroLevelController.OnTargetHitCallback");
        stage = IntroLevelControllerStage.LANDING;
        Debug.Log("------> Victory! Now land the plane");
    }
}
