using UnityEngine;
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
    int selectedMissionIndex = 6; //0;   TEMP
    static readonly int initialUpdates = 3; // Number of initial updates to ensure UI is ready
    static readonly string selectedClassName = "selected-mission-button";
    int updatesToWait = initialUpdates;

    Mission[] missions = new Mission[]
    {
        new Mission { Title = "Intro", Description = "This is the introduction mission.", LevelType = LevelType.INTRO, SceneName = Mission.ThreeDSceneName },
        new Mission { Title = "2D Classic", Description = "This is the first mission.", LevelType = LevelType.NORMAL, SceneName = Mission.TwoDSceneName },
        new Mission { Title = "2D Balloons", Description = "This is the second mission.", LevelType = LevelType.BALLOONS, SceneName = Mission.TwoDSceneName },
        new Mission { Title = "3D Classic", Description = "This is the third mission.", LevelType = LevelType.NORMAL, SceneName = Mission.ThreeDSceneName },
        new Mission { Title = "3D Boss Level", Description = "This is the fourth mission.", LevelType = LevelType.ROBOT_BOSS, SceneName = Mission.ThreeDSceneName },
        new Mission { Title = "Red Baron", Description = "This is the fifth mission. First person view recommended.", LevelType = LevelType.RED_BARON_BOSS, SceneName = Mission.ThreeDSceneName },
        new Mission { Title = "Dam Busters", Description = "This is the sixth mission. Night mission.", LevelType = LevelType.DAM, SceneName = Mission.ThreeDSceneName },
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
        descriptionLabel.text = $"This is a description for mission {selectedMissionIndex + 1}: {selectedMission.Description}";

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
