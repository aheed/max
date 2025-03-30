using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour
{
    UIDocument uiDocument;
    Button twoDbutton;
    Button threeDbutton;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        twoDbutton = uiDocument.rootVisualElement.Q<Button>("2d");
        threeDbutton = uiDocument.rootVisualElement.Q<Button>("3d");
        twoDbutton.RegisterCallback<ClickEvent>(On2dClicked);
        threeDbutton.RegisterCallback<ClickEvent>(On3dClicked);
        SceneManager.activeSceneChanged += ChangedActiveScene;
    }

    void On2dClicked(ClickEvent evt)
    {
        Debug.Log("2D button clicked");
        
        // Load 2D scene
        SceneManager.LoadScene("2DScene");
    }

    void On3dClicked(ClickEvent evt)
    {
        Debug.Log("3D button clicked");
        
        // Load 3D scene
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
