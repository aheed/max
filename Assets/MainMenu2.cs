using UnityEngine;
using UnityEngine.UIElements;

public class Mission
{
    public string Title { get; set; }
    public string Description { get; set; }
    //public string SceneName { get; set; }

    /*public Mission(string title, string description, string sceneName)
    {
        Title = title;
        Description = description;
        //SceneName = sceneName;
    }*/
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
        new Mission { Title = "Intro", Description = "This is the introduction mission." },
        new Mission { Title = "Mission 1", Description = "This is the first mission." },
        new Mission { Title = "Mission 2", Description = "This is the second mission." },
        new Mission { Title = "Mission 3", Description = "This is the third mission." },
        new Mission { Title = "Mission 4", Description = "This is the fourth mission." },
        new Mission { Title = "Mission 5", Description = "This is the fifth mission." },
        new Mission { Title = "Mission 6", Description = "This is the sixth mission." },
        new Mission { Title = "Mission 7", Description = "This is the seventh mission." },
        new Mission { Title = "Mission 8", Description = "This is the eighth mission." },
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
        titleLabel.text = $"Mission {selectedMissionIndex + 1} Title: {selectedMission.Title}";
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
            Debug.Log($"Play button clicked according to button name. Time to start mission {selectedMissionIndex}");
        }
        Debug.Log("Play button clicked");
        // Implement play logic here, e.g., load the selected level
        // SceneManager.LoadScene("SelectedLevelScene");
    }
}
