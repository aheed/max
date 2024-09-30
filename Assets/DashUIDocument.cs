using UnityEngine;
using UnityEngine.UIElements;

public class DashUIDocument : MonoBehaviour, IGameStateObserver
{
    UIDocument uiDocument;
    Label speedLabel;
    public int maxSpeedDisplayed = 130;
    int lastDisplayedSpeed;


    // Start is called before the first frame update
    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        speedLabel = uiDocument.rootVisualElement.Q<Label>("Speed");

        GameState gameState = FindObjectOfType<GameState>();
        gameState.RegisterObserver(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateContents()
    {
        GameState gameState = FindObjectOfType<GameState>();
        GameStateContents gameStateContents = gameState.GetStateContents();
        var speed = (int)((gameStateContents.speed * maxSpeedDisplayed) / gameState.maxSpeed);
        if (speed != lastDisplayedSpeed)
        {
            lastDisplayedSpeed = speed;
            speedLabel.text = speed.ToString();
        }
    }

    public void OnGameStatusChanged(GameStatus gameStatus)
    {
        UpdateContents();
    }

    public void OnGameEvent(GameEvent _)
    {
        UpdateContents();
    }
}
