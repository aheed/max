using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour
{
    UIDocument uiDocument;
    Button twoDbutton;
    Button twoDBalloonbutton;
    Button threeDbutton;
    Button threeDRobotButton;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        twoDbutton = uiDocument.rootVisualElement.Q<Button>("2d");
        twoDBalloonbutton = uiDocument.rootVisualElement.Q<Button>("2dBalloon");
        threeDbutton = uiDocument.rootVisualElement.Q<Button>("3d");
        threeDRobotButton = uiDocument.rootVisualElement.Q<Button>("3dRobot");
        twoDbutton.RegisterCallback<ClickEvent>(On2dClicked);
        twoDBalloonbutton.RegisterCallback<ClickEvent>(On2dBalloonClicked);
        threeDbutton.RegisterCallback<ClickEvent>(On3dClicked);
        threeDRobotButton.RegisterCallback<ClickEvent>(On3dRobotClicked);
        SceneManager.activeSceneChanged += ChangedActiveScene;
    }

    void On2dClicked(ClickEvent evt)
    {
        Debug.Log("2D button clicked");
        
        // Load 2D scene
        SceneManager.LoadScene("2DScene");
    }

    void On2dBalloonClicked(ClickEvent evt)
    {
        Debug.Log("2D balloon button clicked");

        LevelSelection.startLevelOverride = true;
        LevelSelection.startLevel = LevelType.BALLOONS;
        
        // Load 2D scene, balloons level
        SceneManager.LoadScene("2DScene");
    }

    void On3dClicked(ClickEvent evt)
    {
        Debug.Log("3D button clicked");
        
        // Load 3D scene
        SceneManager.LoadScene("3DScene");
    }

    void On3dRobotClicked(ClickEvent evt)
    {
        Debug.Log("3D Robot button clicked");

        LevelSelection.startLevelOverride = true;
        LevelSelection.startLevel = LevelType.ROBOT_BOSS;
        
        // Load 3D scene, robot boss level
        SceneManager.LoadScene("3DScene");
    }

    private void ChangedActiveScene(Scene current, Scene next)
    {
        string currentName = current.name;

        if (currentName == null)
        {
            // Scene1 has been removed
            currentName = "Replaced";
        }

        Debug.Log("Scenes: " + currentName + ", " + next.name);
    }
}
