using UnityEngine;
using UnityEngine.UIElements;

public class MainMenu2 : MonoBehaviour
{
    Button scrollLeftButton;
    Button scrollRightButton;
    Button playButton;

    void Start()
    {
        var uiDocument = GetComponent<UIDocument>();
        scrollLeftButton = uiDocument.rootVisualElement.Q<Button>("LeftScroll");
        scrollRightButton = uiDocument.rootVisualElement.Q<Button>("RightScroll");
        playButton = uiDocument.rootVisualElement.Q<Button>("PlayButton");

        scrollLeftButton.RegisterCallback<ClickEvent>(OnScrollLeftClicked);
        scrollRightButton.RegisterCallback<ClickEvent>(OnScrollRightClicked);
        playButton.RegisterCallback<ClickEvent>(OnPlayClicked);
    }

    void Update()
    {

    }

    void OnScrollLeftClicked(ClickEvent evt)
    {
        Debug.Log("Scroll left button clicked");
        // Implement scrolling logic here
    }

    void OnScrollRightClicked(ClickEvent evt)
    {
        Debug.Log("Scroll right button clicked");
        // Implement scrolling logic here
    }
    
    void OnPlayClicked(ClickEvent evt)
    {
        if (evt.target is Button button && button.name == "PlayButton")
        {
            Debug.Log("Play button clicked according to button name");
        }
        Debug.Log("Play button clicked");
        // Implement play logic here, e.g., load the selected level
        // SceneManager.LoadScene("SelectedLevelScene");
    }
}
