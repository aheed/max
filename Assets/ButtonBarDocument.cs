using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class ButtonBarDocument : MonoBehaviour
{
    public Texture2D crtTexture;
    public Texture2D flatscreenTexture;
    public Texture2D fullScreenTexture;
    public Texture2D exitFullScreenTexture;
    public Texture2D volumeMuteTexture;
    public Texture2D volumeUpTexture;
    public Texture2D normalControlTexture;
    public Texture2D pilotControlTexture;
    public Texture2D normalDotsTexture;
    public Texture2D invertedDotsTexture;
    public Texture2D pauseTexture;
    public Texture2D playTexture;
    VisualElement buttonBarUIElem;
    VisualElement tvElem;
    VisualElement fullScreenElem;
    VisualElement CameraSwapElem;
    VisualElement dotsElem;
    VisualElement muteElem;
    VisualElement pilotElem;
    VisualElement helpElem;
    VisualElement homeElem;
    VisualElement pauseElem;
    VisualElement expandedGroupElem;
    VisualElement fullScreenTapHintElem;
    VisualElement spacerButtonLeftElem;
    VisualElement spacerButtonRightElem;
    Label debugElem;
    GameState gameState;
    bool fullScreen = false; //expected state, could change any time
    bool rightSideExpanded = false;
    bool debugDisplayed = false;
    int quickTapCount = 0;

    public VisualElement GetFullScreenTapHintElem()
    {
        return fullScreenTapHintElem;
    }

    // Start is called before the first frame update
    void Start()
    {
        gameState = GameState.GetInstance();
        var uiDocument = GetComponent<UIDocument>();

        buttonBarUIElem = uiDocument.rootVisualElement.Q<VisualElement>("ButtonBarUI");
        buttonBarUIElem.RegisterCallback<ClickEvent>(OnBackgroundClicked);

        tvElem = uiDocument.rootVisualElement.Q<VisualElement>("TvButton");
        tvElem.RegisterCallback<ClickEvent>(OnTVClicked);

        fullScreenElem = uiDocument.rootVisualElement.Q<VisualElement>("FullScreenButton");
        fullScreenElem.RegisterCallback<PointerDownEvent>(OnFullScreenClicked);

        CameraSwapElem = uiDocument.rootVisualElement.Q<VisualElement>("CameraButton");
        CameraSwapElem.RegisterCallback<ClickEvent>(OnCameraSwapClicked);

        fullScreenTapHintElem = uiDocument.rootVisualElement.Q<VisualElement>("FullScreenTapHint");

        muteElem = uiDocument.rootVisualElement.Q<VisualElement>("MuteButton");
        muteElem.RegisterCallback<ClickEvent>(OnMuteClicked);

        pilotElem = uiDocument.rootVisualElement.Q<VisualElement>("PilotButton");
        pilotElem.RegisterCallback<ClickEvent>(OnPilotClicked);

        helpElem = uiDocument.rootVisualElement.Q<VisualElement>("HelpButton");
        helpElem?.RegisterCallback<ClickEvent>(OnHelpClicked);

        homeElem = uiDocument.rootVisualElement.Q<VisualElement>("HomeButton");
        homeElem.RegisterCallback<ClickEvent>(OnHomeClicked);

        dotsElem = uiDocument.rootVisualElement.Q<VisualElement>("DotsButton");
        dotsElem.RegisterCallback<ClickEvent>(OnDotsClicked);

        pauseElem = uiDocument.rootVisualElement.Q<VisualElement>("PauseButton");
        pauseElem.RegisterCallback<ClickEvent>(OnPauseClicked);

        expandedGroupElem = uiDocument.rootVisualElement.Q<VisualElement>("ExpandedGroup");

        debugElem = uiDocument.rootVisualElement.Q<Label>("DebugLabel");

        spacerButtonLeftElem = uiDocument.rootVisualElement.Q<VisualElement>("SpacerButtonLeft");
        spacerButtonRightElem = uiDocument.rootVisualElement.Q<VisualElement>("SpacerButtonRight");

        UpdateAll();

        GameState.GetInstance().Subscribe(GameEvent.DEBUG_ACTION1, OnDebugCallback1);
        GameState.GetInstance().Subscribe(GameEvent.CAMERA_BUTTON_UPDATED, UpdateCameraButton);
        GameState.GetInstance().Subscribe(GameEvent.HOME_BUTTON_UPDATED, UpdateHomeButton);
        GameState.GetInstance().Subscribe(GameEvent.PAUSE_BUTTON_UPDATED, UpdatePauseButton);
        GameState.GetInstance().Subscribe(GameEvent.TV_SIM_BUTTON_UPDATED, UpdateTvSimButton);
        GameState.GetInstance().Subscribe(GameEvent.SPACER_BUTTONS_UPDATED, UpdateSpacerButtons);

        StartCoroutine(QuickTapCoroutine());
    }

    void UpdateAll()
    {
        UpdateTvSimButton();
        UpdateFullScreenButton();
        UpdateMuteButton();
        UpdatePilotButton();
        UpdateRightSideExpanded();
        UpdateCameraButton();
        UpdateHomeButton();
        UpdatePauseButton();
        UpdateSpacerButtons();
    }

    void UpdateSpacerButtons()
    {
        if (spacerButtonLeftElem == null || spacerButtonRightElem == null)
            return;

        var stateContents = gameState.GetStateContents();
        spacerButtonLeftElem.style.display = stateContents.spacerButtonsVisible ? DisplayStyle.Flex : DisplayStyle.None;
        spacerButtonRightElem.style.display = stateContents.spacerButtonsVisible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    void UpdatePauseButton()
    {
        if (pauseElem == null)
            return;

        pauseElem.style.display = gameState.GetStateContents().pauseButtonVisible ? DisplayStyle.Flex : DisplayStyle.None;

        var newTexture = gameState.IsPaused() ? playTexture : pauseTexture;
        pauseElem.style.backgroundImage = new StyleBackground(newTexture);
    }

    void UpdateHomeButton()
    {
        if (homeElem == null)
            return;

        homeElem.style.display = gameState.GetStateContents().homeButtonVisible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    void UpdateTvSimButton()
    {
        if (tvElem == null)
            return;

        tvElem.style.display = gameState.GetStateContents().tvSimButtonVisible ? DisplayStyle.Flex : DisplayStyle.None;

        var newTexture = gameState.viewMode == ViewMode.NORMAL ? crtTexture : flatscreenTexture;
        tvElem.style.backgroundImage = new StyleBackground(newTexture);
    }

    void UpdateFullScreenButton()
    {
        var newTexture = Screen.fullScreen ? exitFullScreenTexture : fullScreenTexture;
        fullScreenElem.style.backgroundImage = new StyleBackground(newTexture);
    }

    void CheckFullScreenButton()
    {
        if (fullScreen != Screen.fullScreen)
        {
            UpdateFullScreenButton();
            fullScreen = Screen.fullScreen;
        }
    }

    void UpdateMuteButton()
    {
        var newTexture = Settings.GetMute() ? volumeMuteTexture : volumeUpTexture;
        muteElem.style.backgroundImage = new StyleBackground(newTexture);
    }

    void UpdatePilotButton()
    {
        var newTexture = Settings.GetPilotControl() ? pilotControlTexture : normalControlTexture;
        pilotElem.style.backgroundImage = new StyleBackground(newTexture);
    }

    void UpdateCameraButton()
    {
        if (CameraSwapElem == null)
            return;

        CameraSwapElem.style.display = GameState.GetInstance().GetStateContents().cameraButtonVisible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    void Update()
    {
        CheckFullScreenButton();
    }

    void OnTVClicked(ClickEvent evt)
    {
        Debug.Log("TV sim toggle clicked");
        if (evt.target != tvElem)
            return;

        gameState.SetViewMode(gameState.viewMode == ViewMode.NORMAL ? ViewMode.TV_SIM : ViewMode.NORMAL);
        UpdateTvSimButton();
    }

    void OnFullScreenClicked(PointerDownEvent evt)
    {
        Debug.Log("Fullscreen toggle clicked");
        // Only perform this action at the target, not in a parent
        if (evt.target != fullScreenElem)
            return;

        Screen.fullScreen = !Screen.fullScreen;
        //UpdateFullScreenButton();
    }

    void OnCameraSwapClicked(ClickEvent evt)
    {
        Debug.Log("Camera swap clicked");
        if (evt.target != CameraSwapElem)
            return;

        gameState.ReportEvent(GameEvent.CAMERA_CHANGE_REQUESTED);
    }

    void OnMuteClicked(ClickEvent evt)
    {
        Debug.Log("Mute clicked");
        if (evt.target != muteElem)
            return;

        Settings.SetMute(!Settings.GetMute());
        quickTapCount++;
        if (quickTapCount >= 5)
        {
            quickTapCount = 0;
            gameState.SetDebugInfoVisible(!gameState.GetStateContents().debugInfoVisible);
        }

        //AudioListener.pause = !AudioListener.pause;
        UpdateMuteButton();
    }

    void OnPilotClicked(ClickEvent evt)
    {
        Debug.Log("Pilot clicked");
        if (evt.target != pilotElem)
            return;

        Settings.SetPilotControl(!Settings.GetPilotControl());

        UpdatePilotButton();
    }

    void OnHelpClicked(ClickEvent evt)
    {
        Debug.Log("Help clicked");
        if (evt.target != helpElem)
            return;

        FindAnyObjectByType<UserGuide>(FindObjectsInactive.Include).gameObject.SetActive(true);
    }

    void OnHomeClicked(ClickEvent evt)
    {
        Debug.Log("Home clicked");
        if (evt.target != homeElem)
            return;

        LevelSelection.startLevelOverride = false;
        SceneManager.LoadScene("mainMenuScene");
    }

    void UpdateRightSideExpanded()
    {
        var expansionDisplayStyle = rightSideExpanded ? DisplayStyle.Flex : DisplayStyle.None;
        expandedGroupElem.style.display = expansionDisplayStyle;

        var newTexture = rightSideExpanded ? invertedDotsTexture : normalDotsTexture;
        dotsElem.style.backgroundImage = new StyleBackground(newTexture);
    }

    void OnPauseClicked(ClickEvent evt)
    {
        Debug.Log("Pause clicked");
        if (evt.target != pauseElem)
            return;

        gameState.SetPause(!gameState.IsPaused());
    }

    void OnDotsClicked(ClickEvent evt)
    {
        Debug.Log("Dots clicked");
        if (evt.target != dotsElem)
            return;

        rightSideExpanded = !rightSideExpanded;
        UpdateRightSideExpanded();
    }

    void OnBackgroundClicked(ClickEvent evt)
    {
        Debug.Log("Background clicked");
        if (evt.target != buttonBarUIElem)
            return;

        rightSideExpanded = false;
        UpdateRightSideExpanded();
    }

    private void OnDebugCallback1()
    {
        debugDisplayed = !debugDisplayed;
        var controllersString = "";

        if (debugDisplayed)
        {
            controllersString = "Gamepads:\n";
            foreach (var controller in UnityEngine.InputSystem.Gamepad.all)
            {
                controllersString += $"{controller.displayName} - {controller.name}\n";
                controllersString += $"  Left Stick: {controller.leftStick.ReadValue()}\n";
                controllersString += $"  Right Stick: {controller.rightStick.ReadValue()}\n";
                controllersString += $"  Current Left Trigger: {controller.leftTrigger.ReadValue()}\n";
                controllersString += $"  Current Right Trigger: {controller.rightTrigger.ReadValue()}\n";
            }
            var currentGamepad = UnityEngine.InputSystem.Gamepad.current;
            controllersString += $"\ncurrent: {currentGamepad?.displayName} - {currentGamepad?.name}\n";

            controllersString += "\n\nJoysticks:\n";
            foreach (var controller in UnityEngine.InputSystem.Joystick.all)
            {
                controllersString += $"{controller.displayName} - {controller.name}\n";
                controllersString += $"  Stick: {controller.stick.ReadValue()}\n";
                controllersString += $"  Current Trigger: {controller.trigger.ReadValue()}\n";
            }
            var currentJoystick = UnityEngine.InputSystem.Joystick.current;
            controllersString += $"\n\ncurrent: {currentJoystick?.displayName} - {currentJoystick?.name}\n";

            controllersString += "\n\nDevices:\n";
            foreach (var device in UnityEngine.InputSystem.InputSystem.devices)
            {
                //controllersString += $"{device.displayName}";
                controllersString += $"{device.description}  ";
                controllersString += $"  type: {device.description.deviceClass}\n";
            }

            // Use Input Manager to get the current input devices
            controllersString += "\n\nUnityEngine.Input (Legacy):\n";
            controllersString += $"  Joystick Names: {string.Join(", ", Input.GetJoystickNames())}\n";

        }

        debugElem.text = controllersString;
    }

    IEnumerator QuickTapCoroutine() {
        CustomYieldInstruction waitShort = new WaitForSecondsRealtime(1);
        while (true) {            
            yield return waitShort;
            quickTapCount = 0;
        }
    }
}
