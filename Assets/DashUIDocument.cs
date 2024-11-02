using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

public class DashUIDocument : MonoBehaviour, IGameStateObserver
{
    UIDocument uiDocument;
    
    Label altLabel;
    Label fuelLabel;
    Label FLabel;
    Label BLabel;
    Label MLabel;
    Label GLabel;
    Label speedLabel;
    Label bombsLabel;
    Label scoreLabel;
    Label alertLabel;
    Label rankLabel;
    VisualElement dashBase;
    
    public int maxSpeedDisplayed = 130;
    int lastDisplayedSpeed;
    public int maxAltitudeDisplayed = 99;
    public int maxFuelDisplayed = 200;
    int lastDisplayedFuel;
    bool dirty = true;
    SimpleBlinker dashBlinker;
    HashSet<EnemyPlane> enemyPlaneSet;
    

    // Start is called before the first frame update
    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        dashBase = uiDocument.rootVisualElement.Q<VisualElement>("DashBase");
        altLabel = uiDocument.rootVisualElement.Q<Label>("Alt");
        fuelLabel = uiDocument.rootVisualElement.Q<Label>("Fuel");
        FLabel = uiDocument.rootVisualElement.Q<Label>("F");
        BLabel = uiDocument.rootVisualElement.Q<Label>("B");
        MLabel = uiDocument.rootVisualElement.Q<Label>("M");
        GLabel = uiDocument.rootVisualElement.Q<Label>("G");
        speedLabel = uiDocument.rootVisualElement.Q<Label>("Speed");
        bombsLabel = uiDocument.rootVisualElement.Q<Label>("Bombs");
        scoreLabel = uiDocument.rootVisualElement.Q<Label>("Score");
        alertLabel = uiDocument.rootVisualElement.Q<Label>("Alert");
        //rankLabel = uiDocument.rootVisualElement.Q<Label>("Rank");

        enemyPlaneSet = new();
        GameState gameState = FindObjectOfType<GameState>();
        gameState.RegisterObserver(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (dirty)
        {
            UpdateContents();
        }

        dashBlinker?.Update(Time.deltaTime);
    }

    void UpdateContents()
    {
        GameState gameState = FindObjectOfType<GameState>();
        GameStateContents gameStateContents = gameState.GetStateContents();
        var speed = (int)((gameStateContents.speed * maxSpeedDisplayed) / gameState.maxSpeed);
        if (speed != lastDisplayedSpeed)
        {
            lastDisplayedSpeed = speed;
            speedLabel.text = $"{speed:000}";
            var color = gameStateContents.speed >= gameState.GetSafeTakeoffSpeed() ? Color.white : Color.gray;
            speedLabel.style.color = color;
        }

        var altitude = (int)((gameStateContents.altitude * maxAltitudeDisplayed) / gameState.maxAltitude);
        altLabel.text = $"{altitude:00}";
        
        var bgColor = Color.black;
        var planeLowestPoint = gameStateContents.altitude - Altitudes.planeHeight / 2;
        if (planeLowestPoint < (Altitudes.strafeMaxAltitude + Altitudes.planeHeight / 2))
        {
            if (planeLowestPoint < Altitudes.unsafeAltitude && gameStateContents.speed >= (gameState.maxSpeed - 0.001f))
            {
                if (dashBlinker == null)
                {
                    dashBlinker = new SimpleBlinker(dashBase);
                }
            }   
            else
            {
                bgColor = new Color(0.5f, 0.4f, 0f); // brown
            }
        }

        if (enemyPlaneSet.Any(e => CollisionHelper.IsOverlappingAltitude(
                gameStateContents.altitude,
                Altitudes.planeHeight,
                e.GetAltitude(),
                e.GetHeight()) && e.IsAlive()))
        {
            bgColor = Color.blue;
        }

        if (gameStateContents.gameStatus != GameStatus.FLYING || gameStateContents.speed < (gameState.maxSpeed - 0.001f))
        {   
            bgColor = Color.black;         
            dashBlinker = null;
        }

        if (planeLowestPoint >= Altitudes.unsafeAltitude)
        {
            dashBlinker = null;
        }

        if (dashBlinker == null)
        {
            dashBase.style.backgroundColor = bgColor;
        }

        var fuel = (int)((gameStateContents.fuel * maxFuelDisplayed) / gameState.maxFuel);
        if (fuel != lastDisplayedFuel)
        {
            lastDisplayedFuel = fuel;
            fuelLabel.text = $"{fuel:000}";
        }

        bombsLabel.text = $"{gameStateContents.bombs:00}";
        scoreLabel.text = $"{gameStateContents.score:0000}";
        alertLabel.text = gameStateContents.alert.ToString();
        FLabel.text = gameState.GotDamage(DamageIndex.F) ? "F" : "";
        BLabel.text = gameState.GotDamage(DamageIndex.B) ? "B" : "";
        MLabel.text = gameState.GotDamage(DamageIndex.M) ? "M" : "";
        GLabel.text = gameState.GotDamage(DamageIndex.G) ? "G" : "";

        dirty = false;
    }

    public void OnGameStatusChanged(GameStatus gameStatus)
    {
        dirty = true;
    }

    public void OnGameEvent(GameEvent ge)
    {
        if (ge == GameEvent.START)
        {
            enemyPlaneSet = new();
        }
        dirty = true;
    }

    public void OnBombLanded(Bomb bomb, GameObject hitObject) {}

    public void OnEnemyPlaneStatusChanged(EnemyPlane enemyPlane, bool active)
    {
        
        if (active)
        {
            enemyPlaneSet.Add(enemyPlane);
        }
        else
        {
            enemyPlaneSet.Remove(enemyPlane);
        }
        Debug.Log($"enemy airplane status: {active} {enemyPlaneSet.Count}");
    }
}
