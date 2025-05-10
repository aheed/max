using UnityEngine;
using System;
using UnityEngine.UIElements;

enum IntroControllerStage 
{
    PRE_START,
    ACCELERATING,
    TAKE_OFF,
    FIRE_DEMO,
    BOMB_DEMO,
    FULL_SCREEN,
    ENEMY_APPROACHING,
    ENEMY_SITTING_DUCK,
    ENEMY_RIGHT_ALTITUDE,
    BOMB_BUILDING,
    LANDING,
    LANDING_APPROACH,
    FINISHED,
    CRASHED
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
    IntroLevelEnemyPlaneNavigator targetPlaneNavigator;
    ControlDocument controlDocument;
    DialogDocument dialogDocument;
    bool fullScreen;


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

    void ResetHints()
    {
        controlDocument.SetFireHintVisible(false);
        controlDocument.SetUpSwipeHintVisible(false);
        controlDocument.SetDownSwipeHintVisible(false);
        controlDocument.SetFullScreenTapHintVisible(false);
        dialogDocument.ShowDialog();
        dialogDocument.HideOkButton();
    }

    void Start()
    {
        Debug.Log("IntroController.Start");
        controlDocument = FindAnyObjectByType<ControlDocument>(FindObjectsInactive.Include);
        controlDocument.gameObject.SetActive(true);
        dialogDocument = FindAnyObjectByType<DialogDocument>();
        dialogDocument.SetOkButtonCallback(OnOkButtonClicked);
        var buttonBarDocument = FindAnyObjectByType<ButtonBarDocument>();
        var fullScreenTapHintElem = buttonBarDocument.GetFullScreenTapHintElem();
        controlDocument.SetFullScreenTapHintElement(fullScreenTapHintElem);
        fullScreen = Screen.fullScreen;
        
        ResetHints();
        callbacks = new CallbackSpec[] 
        {
            new() { gameEvent = GameEvent.GAME_STATUS_CHANGED, action = OnGameStatusChangedCallback },
            new() { gameEvent = GameEvent.DEBUG_ACTION2, action = OnDebugAction2 },
            new() { gameEvent = GameEvent.ALT_CHANGED, action = OnAltitudeChangedCallback },
            new() { gameEvent = GameEvent.TARGET_HIT, action = OnTargetHitCallback },
            new() { gameEvent = GameEvent.BULLET_FIRED, action = OnBulletFiredCallback },
            new() { gameEvent = GameEvent.BOMB_DROPPED, action = OnBombDroppedCallback },
            new() { gameEvent = GameEvent.LANDING_CHANGED, action = OnLandingChangedCallback }
        };
        RegisterCallbacks();
    }

    void Update()
    {
        if (Screen.fullScreen != fullScreen)
        {
            fullScreen = Screen.fullScreen;
            if (stage == IntroControllerStage.FULL_SCREEN)
            {
                AdvanceStage();
            }
        }

        if (stage == IntroControllerStage.ENEMY_APPROACHING && 
            targetPlaneNavigator?.stage == EnemyPlaneNavigatorStage.SITTING_DUCK)
        {
            AdvanceStage();
        }
    }

    void SpawnTargetPlane()
    {
        var targetPlanePosition = transform.parent.position;
        targetPlanePosition.y = GameState.GetInstance().maxAltitude / 3;
        targetPlane = Instantiate(targetPlanePrefab, targetPlanePosition, Quaternion.identity);
        //targetPlane = Instantiate(targetPlanePrefab);
        targetPlane.refObject = transform.parent;
        targetPlaneNavigator = new IntroLevelEnemyPlaneNavigator(targetPlane);
        targetPlane.SetNavigator(targetPlaneNavigator);
        targetPlane.SetVip();
        targetPlane.SetDestructible(false);
        stage = IntroControllerStage.ENEMY_APPROACHING;
    }

    void DisplayText(string text)
    {
        Debug.Log($"------------------> {text}"); //TEMP!!
        dialogDocument.SetDialogText(text);
    }

    void AdvanceStage()
    {
        stage += 1;
        ResetHints();
        switch (stage)
        {
            case IntroControllerStage.ACCELERATING:
                DisplayText("Welcome!");
                break;
            case IntroControllerStage.TAKE_OFF:
                DisplayText("Swipe up to take off");
                controlDocument.SetUpSwipeHintVisible(true);
                break;
            case IntroControllerStage.FIRE_DEMO:
                controlDocument.SetFireHintVisible(true);
                DisplayText("Tap to fire your machine gun");
                break;
            case IntroControllerStage.BOMB_DEMO:
                controlDocument.SetFireHintVisible(true);
                controlDocument.SetDownSwipeHintVisible(true);
                DisplayText("Drop a bomb by swiping down while firing");
                break;
            case IntroControllerStage.FULL_SCREEN:
                DisplayText("This game is best viewed in full screen mode and in landscape orientation");
                controlDocument.SetFullScreenTapHintVisible(true);
                dialogDocument.ShowOkButton();
                break;
            case IntroControllerStage.ENEMY_APPROACHING:
                SpawnTargetPlane();
                dialogDocument.HideDialog();
                break;
            case IntroControllerStage.ENEMY_SITTING_DUCK:
                targetPlane.SetDestructible(true);
                DisplayText("Shoot the enemy plane");
                break;
            case IntroControllerStage.ENEMY_RIGHT_ALTITUDE:
                DisplayText("Blue dashboard indicates presence of an enemy plane at your altitude. Take him out!");
                break;
            case IntroControllerStage.BOMB_BUILDING:
                //DisplayText("Bomb the building");
                ++stage;
                break;
            case IntroControllerStage.LANDING:
                DisplayText("Victory! Now land on the nearest airstrip");
                CheckLandingApproach();
                break;
            case IntroControllerStage.LANDING_APPROACH:
                DisplayText("\"L\" on the dashboard means you are approaching a friendly airstrip\n\n\"P\" = Enemy plane alert\n\"W\" = Wind alert");
                break;
            case IntroControllerStage.FINISHED:
                DisplayText("Congratulations! You have completed your training mission. You are on your own now!");
                break;
            case IntroControllerStage.CRASHED:
                DisplayText("Try again\n\nTap Fire button to restart");
                controlDocument.SetFireHintVisible(true);
                break;
        }
    }

    public void OnGameStatusChangedCallback()
    {
        OnGameStatusChanged(GameState.GetInstance().GetStateContents().gameStatus);
    }

    public void OnGameStatusChanged(GameStatus gameStatus)
    {
        if (gameStatus == GameStatus.ACCELERATING ||
            gameStatus == GameStatus.FLYING ||
            gameStatus == GameStatus.FINISHED)
        {
            AdvanceStage();
        }
        else if (gameStatus == GameStatus.REFUELLING)
        {
            stage = IntroControllerStage.PRE_START;
        }
        else if (gameStatus == GameStatus.DEAD)
        {
            stage = IntroControllerStage.CRASHED-1;
            AdvanceStage();
        }
    }

    void OnDebugAction2()
    {
        // spawn enemy plane
        Debug.Log("IntroController.OnDebugAction2");
        //SpawnTargetPlane();
        AdvanceStage();
    }

    void OnAltitudeChangedCallback()
    {
        //Debug.Log("IntroController.OnAltitudeChanged");
        var gameState = GameState.GetInstance();
        if (stage == IntroControllerStage.TAKE_OFF)
        {
            var altitude = gameState.GetStateContents().altitude;
            if (altitude > gameState.minSafeAltitude)
            {
                AdvanceStage();
            }
        }
        else if (stage == IntroControllerStage.ENEMY_SITTING_DUCK &&
            gameState.AnyEnemyPlaneAtCollisionAltitude())
        {
           AdvanceStage();
        }
    }

    void OnTargetHitCallback()
    {
        //Debug.Log("IntroLevelController.OnTargetHitCallback");
        GameState.GetInstance().ReportBossDefeated();
        stage = IntroControllerStage.LANDING-1;
        AdvanceStage();
    }

    void OnBulletFiredCallback()
    {
        if (stage == IntroControllerStage.FIRE_DEMO)
        {
            AdvanceStage();
        }
    }

    void OnBombDroppedCallback()
    {
        if (stage == IntroControllerStage.BOMB_DEMO)
        {
            AdvanceStage();
        }
    }

    void OnDestroy()
    {
        UnregisterCallbacks();
    }

    void OnOkButtonClicked()
    {
        Debug.Log("IntroController.OnOkButtonClicked");
        AdvanceStage();
    }

    void CheckLandingApproach()
    {
        var gameState = GameState.GetInstance();
        if (stage == IntroControllerStage.LANDING &&
            gameState.GetStateContents().approachingLanding)
        {
            AdvanceStage();
        }
    }

    void OnLandingChangedCallback()
    {
        CheckLandingApproach();
    }
}
