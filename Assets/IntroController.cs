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
    FINISHED_OK,
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
        dialogDocument = FindAnyObjectByType<DialogDocument>();
        dialogDocument.SetOkButtonCallback(OnOkButtonClicked);
        var buttonBarDocument = FindAnyObjectByType<ButtonBarDocument>();
        var fullScreenTapHintElem = buttonBarDocument.GetFullScreenTapHintElem();
        controlDocument.SetFullScreenTapHintElement(fullScreenTapHintElem);
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
        GameStartUpdate();
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
        targetPlanePosition.z = -1f; // behind the player to avoid immediate collision
        targetPlane = Instantiate(targetPlanePrefab, targetPlanePosition, Quaternion.identity);
        targetPlane.refObject = transform.parent;
        targetPlaneNavigator = new IntroLevelEnemyPlaneNavigator(targetPlane);
        targetPlane.SetNavigator(targetPlaneNavigator);
        targetPlane.SetVip();
        targetPlane.SetDestructible(false);
        stage = IntroControllerStage.ENEMY_APPROACHING;
    }

    void DisplayText(string text)
    {
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
                if (Globals.touchScreenDetected)
                {
                    DisplayText("Swipe up to take off");
                    controlDocument.SetUpSwipeHintVisible(true);
                }
                else
                {
                    var pilot = Settings.GetPilotControl();
                    var displayText = $"Keyboard: {(pilot ? "Down" : "Up")} key to take off\n\nJoystick/gamepad support\n\nUp/Down configurable";
                    DisplayText(displayText);
                }
                break;
            case IntroControllerStage.FIRE_DEMO:
                controlDocument.SetFireHintVisible(Globals.touchScreenDetected);
                var fireInstruction = Globals.touchScreenDetected ? string.Empty : "\n\nKeyboard: Space bar";
                DisplayText($"Fire your machine gun {fireInstruction}");
                break;
            case IntroControllerStage.BOMB_DEMO:
                controlDocument.SetFireHintVisible(Globals.touchScreenDetected);
                controlDocument.SetDownSwipeHintVisible(Globals.touchScreenDetected);
                DisplayText("Drop a bomb by descending while firing");
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
                DisplayText("Blue dashboard indicates enemy plane at your altitude. Take him out!");
                break;
            case IntroControllerStage.BOMB_BUILDING:
                //todo: add a building
                //DisplayText("Bomb the building");
                ++stage; // skip this stage for now
                break;
            case IntroControllerStage.LANDING:
                DisplayText("Victory! Now land on the nearest airstrip");
                CheckLandingApproach();
                break;
            case IntroControllerStage.LANDING_APPROACH:
                DisplayText("\"L\" = Approaching airstrip\n\"P\" = Enemy plane alert\n\"W\" = Wind alert");
                break;
            case IntroControllerStage.FINISHED:
                DisplayText("Congratulations! You have completed your training mission. You are on your own now!");
                Settings.SetSelectedMission(Settings.GetSelectedMission() + 1);
                dialogDocument.ShowOkButton();
                break;
            case IntroControllerStage.FINISHED_OK:
                dialogDocument.HideDialog();
                break;
            case IntroControllerStage.CRASHED:
                DisplayText("Try again!\n\nFire button to restart");
                controlDocument.SetFireHintVisible(Globals.touchScreenDetected);
                break;
        }
    }

    public void OnGameStatusChangedCallback()
    {
        OnGameStatusChanged(GameState.GetInstance().GetStateContents().gameStatus);
    }
    
    void GameStartUpdate()
    {
        controlDocument.gameObject.SetActive(true);
        controlDocument.SetFireButtonVisible(Globals.touchScreenDetected);
        fullScreen = Screen.fullScreen;
        ResetHints();
    }

    public void OnGameStatusChanged(GameStatus gameStatus)
    {
        if (gameStatus == GameStatus.ACCELERATING ||
            gameStatus == GameStatus.FLYING ||
            gameStatus == GameStatus.FINISHED)
        {
            AdvanceStage();
        }
        else if (gameStatus == GameStatus.REPAIRING)
        {
            GameStartUpdate();
            stage = IntroControllerStage.PRE_START;
        }
        else if (gameStatus == GameStatus.DEAD)
        {
            stage = IntroControllerStage.CRASHED - 1;
            AdvanceStage();
        }
    }

    void OnDebugAction2()
    {
        Debug.Log("IntroController.OnDebugAction2");
        AdvanceStage();
    }

    void OnAltitudeChangedCallback()
    {
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
