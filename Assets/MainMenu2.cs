using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class Mission
{
    public static readonly string TwoDSceneName = "2DScene";
    public static readonly string ThreeDSceneName = "3DScene";
    public string Title { get; set; }
    public string Description { get; set; }
    public LevelType LevelType { get; set; }
    public string SceneName { get; set; }
}

public class MainMenu2 : MonoBehaviour
{
    Button scrollLeftButton;
    Button scrollRightButton;
    Button playButton;
    Label titleLabel;
    Label descriptionLabel;
    ScrollView missionsScrollView;
    VisualElement missionsElement;
    int selectedMissionIndex = 0;
    static readonly int initialUpdates = 3; // Number of initial updates to ensure UI is ready
    static readonly string selectedClassName = "selected-mission-button";
    int updatesToWait = initialUpdates;

    Mission[] missions = new Mission[]
    {
        new Mission { Title = "Intro", Description = "Let's get you airborne!", LevelType = LevelType.INTRO, SceneName = Mission.ThreeDSceneName },
        new Mission { Title = "2D Classic", Description = "Triple stage mission. 1983 looks better than ever.", LevelType = LevelType.NORMAL, SceneName = Mission.TwoDSceneName },
        new Mission { Title = "2D Balloons", Description = "How many red balloons go by? You guessed it.", LevelType = LevelType.BALLOONS, SceneName = Mission.TwoDSceneName },
        new Mission { Title = "3D Classic", Description = "Triple stage mission again. 3 dimensions. Still 2 sets of wings.", LevelType = LevelType.NORMAL, SceneName = Mission.ThreeDSceneName },
        new Mission { Title = "3D Boss Level", Description = "Guest boss appearance. Remember how to defeat this villain?", LevelType = LevelType.ROBOT_BOSS, SceneName = Mission.ThreeDSceneName },
        new Mission { Title = "Red Baron", Description = "How long did you think you could fly a Sopwith Camel without running into this enemy? First person view recommended.", LevelType = LevelType.RED_BARON_BOSS, SceneName = Mission.ThreeDSceneName },
        new Mission { Title = "Dam Busters", Description = "Night mission. Bouncier bombs. Based on a true story. Loosely.", LevelType = LevelType.DAM, SceneName = Mission.ThreeDSceneName },
    };

    void Start()
    {
        var uiDocument = GetComponent<UIDocument>();
        scrollLeftButton = uiDocument.rootVisualElement.Q<Button>("LeftScroll");
        scrollRightButton = uiDocument.rootVisualElement.Q<Button>("RightScroll");
        missionsScrollView = uiDocument.rootVisualElement.Q<ScrollView>("ScrollView");
        playButton = uiDocument.rootVisualElement.Q<Button>("PlayButton");

        scrollLeftButton.RegisterCallback<ClickEvent>(OnScrollLeftClicked);
        scrollRightButton.RegisterCallback<ClickEvent>(OnScrollRightClicked);
        playButton.RegisterCallback<ClickEvent>(OnPlayClicked);

        titleLabel = uiDocument.rootVisualElement.Q<Label>("Title");
        descriptionLabel = uiDocument.rootVisualElement.Q<Label>("Description");

        missionsElement = uiDocument.rootVisualElement.Q<VisualElement>("Missions");
        for (int i = 0; i < missionsElement.childCount; i++)
        {
            var capturedIndex = i; // Capture the current index for the callback
            var child = missionsElement.ElementAt(i);
            if (child is Button button)
            {
                button.RegisterCallback<ClickEvent>(evt => OnMissionButtonClicked(capturedIndex, evt));
            }
        }

        if (missionsElement.childCount != missions.Length)
        {
            Debug.LogWarning($"Expected {missions.Length} mission buttons, found {missionsElement.childCount}.");
        }

        selectedMissionIndex = Settings.GetSelectedMission();
        Settings.Update();
        GameState.GetInstance().SetPause(false);
        EnhancedTouchSupport.Enable();
    }

    void Update()
    {
        // Ensure the UI is ready before updating mission details
        if (updatesToWait > 0)
        {
            updatesToWait--;
            if (updatesToWait == 0)
            {
                UpdateMissionDetails();
            }
        }

        if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count > 0)
        {
            Globals.touchScreenDetected = true;
            Debug.Log("Touch screen detected");
        }
    }

    void UpdateMissionDetails()
    {
        if (selectedMissionIndex < 0 || selectedMissionIndex >= missions.Length)
        {
            titleLabel.text = "No mission selected";
            descriptionLabel.text = "Please select a mission.";
            return;
        }

        var selectedMission = missions[selectedMissionIndex];
        titleLabel.text = selectedMission.Title;
        descriptionLabel.text = selectedMission.Description;

        for (int i = 0; i < missionsElement.childCount; i++)
        {
            var child = missionsElement.ElementAt(i);
            if (child is Button btn)
            {
                if (i == selectedMissionIndex)
                {
                    btn.AddToClassList(selectedClassName);
                    missionsScrollView.ScrollTo(btn);
                    Debug.Log($"Scrolled to mission button {selectedMissionIndex + 1}: {btn.text}");
                }
                else
                {
                    btn.RemoveFromClassList(selectedClassName);
                }
            }
        }
    }

    void OnMissionButtonClicked(int missionIndex, ClickEvent evt)
    {
        if (evt.target is Button button)
        {
            selectedMissionIndex = missionIndex;
            Settings.SetSelectedMission(selectedMissionIndex);
            Debug.Log($"Mission button {missionIndex} clicked: {button.text}");
            UpdateMissionDetails();
        }
    }

    void OnScrollLeftClicked(ClickEvent evt)
    {
        Debug.Log("Scroll left button clicked");
        missionsScrollView.scrollOffset = new Vector2(
            Mathf.Max(0, missionsScrollView.scrollOffset.x - 100), // Adjust the scroll amount as needed
            missionsScrollView.scrollOffset.y
        );
    }

    void OnScrollRightClicked(ClickEvent evt)
    {
        Debug.Log("Scroll right button clicked");
        missionsScrollView.scrollOffset = new Vector2(
            Mathf.Max(0, missionsScrollView.scrollOffset.x + 100), // Adjust the scroll amount as needed
            missionsScrollView.scrollOffset.y
        );
    }

    void OnPlayClicked(ClickEvent evt)
    {
        if (evt.target is Button button && button.name == "PlayButton")
        {
            Debug.Log($"Play button clicked. Time to start mission {selectedMissionIndex}");
            var selectedMission = missions[selectedMissionIndex];
            LevelSelection.startLevelOverride = true;
            LevelSelection.startLevel = selectedMission.LevelType;
            SceneManager.LoadScene(selectedMission.SceneName);
        }
    }
}
