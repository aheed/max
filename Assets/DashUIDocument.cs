using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

public class DashUIDocument : MonoBehaviour
{
    public int maxSpeedDisplayed = 130;
    public int maxAltitudeDisplayed = 99;
    public int maxFuelDisplayed = 200;
    public float speedPitchMax = 1f;
    public float altitudePitchMax = 1f;
    
    public AudioClip bingClip;
    public AudioClip damageClip;
    public AudioClip alertClip;
    public AudioClip bangSmallClip;
    public AudioClip bangMediumClip;
    public AudioClip bangBigClip;
    public AudioClip rulebritanniaClip;
    AudioSource audioSource;
    AudioSource motorAudioSource;
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
    Label fpsLabel;
    Label targetsLabel;
    VisualElement dashBase;
    VisualElement topRowInner;
    VisualElement rankOuter;

    int lastDisplayedFuel;
    int lastDisplayedAltitude;
    SimpleBlinker dashBlinker;
    public float avgAlpha = 0.01f;
    float fpsAvg;
    int displayCnt = 0;
    GameState gameState;
    GameStateContents gameStateContents;
    float speedPitch;
    float altitudePitch;
    

    // Start is called before the first frame update
    void Start()
    {
        var audioSources = GetComponents<AudioSource>();
        audioSource = audioSources[0];
        motorAudioSource = audioSources[1];
        uiDocument = GetComponent<UIDocument>();
        dashBase = uiDocument.rootVisualElement.Q<VisualElement>("DashBase");
        topRowInner = uiDocument.rootVisualElement.Q<VisualElement>("TopRowInner");
        rankOuter = uiDocument.rootVisualElement.Q<VisualElement>("RankOuter");
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
        rankLabel = uiDocument.rootVisualElement.Q<Label>("Rank");
        fpsLabel = uiDocument.rootVisualElement.Q<Label>("Fps");
        targetsLabel = uiDocument.rootVisualElement.Q<Label>("Targets");

        gameState = GameState.GetInstance();
        gameStateContents = gameState.GetStateContents();
        
        SetupCallbacks();

        Reset();
    }

    // Update is called once per frame
    void Update()
    {
        var fps = 1f / Time.deltaTime;
        fpsAvg = fpsAvg * (1-avgAlpha) + fps * avgAlpha;
        
        dashBlinker?.Update(Time.deltaTime);

        if (++displayCnt > 10)
        {
            displayCnt = 0;
            fpsLabel.text = $"{fpsAvg:000}";
            UpdateFuel();
        }
    }

    void UpdateDashColor()
    {
        var bgColor = Color.black;
        var planeLowestPoint = gameStateContents.altitude - Altitudes.planeHeight / 2;
        if (planeLowestPoint < Altitudes.strafeMaxAltitude)
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

        if (gameState.AnyEnemyPlaneAtCollisionAltitude())
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
    }

    void UpdatePitch()
    {
        motorAudioSource.pitch = 1 + speedPitch + altitudePitch;
        var mute = 
            gameStateContents.gameStatus == GameStatus.FINISHED ||
            gameStateContents.gameStatus == GameStatus.DEAD ||
            gameStateContents.gameStatus == GameStatus.KILLED_BY_FLACK ||
            gameStateContents.gameStatus == GameStatus.OUT_OF_FUEL;
        motorAudioSource.mute = mute;
    }

    void UpdateSpeed()
    {
        var relSpeed = gameStateContents.speed / gameState.maxSpeed;
        var speed = (int)(relSpeed * maxSpeedDisplayed);
        speedLabel.text = $"{speed:000}";
        var color = gameStateContents.speed >= gameState.GetSafeTakeoffSpeed() ? Color.white : Color.gray;
        speedLabel.style.color = color;
        speedPitch = relSpeed * speedPitchMax;
        UpdatePitch();
        UpdateDashColor();
    }

    void UpdateAlt()
    {
        var relAltitude = gameStateContents.altitude / gameState.maxAltitude;
        var altitude = (int)(relAltitude * maxAltitudeDisplayed);
        if (altitude != lastDisplayedAltitude)
        {
            altLabel.text = $"{altitude:00}";
            altitudePitch = relAltitude * altitudePitchMax;
            UpdatePitch();
            lastDisplayedAltitude = altitude;
        }
        UpdateDashColor();
    }

    void UpdateBombs() 
    {
        bombsLabel.text = $"{gameStateContents.bombs:00}";
    }

    void UpdateScore() 
    {
        scoreLabel.text = $"{gameStateContents.score:0000}";
    }

    void UpdateTargets()
    {
        targetsLabel.text = $"{gameState.GetTargetsHit()}/{gameStateContents.targetsHitMin}";
    }

    void UpdateAlert()
    {
        if (gameStateContents.gameStatus != GameStatus.FLYING)
        {
            alertLabel.text = "";
            return;
        }

        if (gameState.AnyEnemyPlanes())
        {
            alertLabel.text = "P";
            return;
        }

        if (gameStateContents.approachingLanding)
        {
            alertLabel.text = "L";
            return;
        }

        if (gameStateContents.wind)
        {
            alertLabel.text = "W";
            return;
        }

        alertLabel.text = "";
    }

    void UpdateDamage() 
    {
        FLabel.text = gameState.GotDamage(DamageIndex.F) ? "F" : "";
        BLabel.text = gameState.GotDamage(DamageIndex.B) ? "B" : "";
        MLabel.text = gameState.GotDamage(DamageIndex.M) ? "M" : "";
        GLabel.text = gameState.GotDamage(DamageIndex.G) ? "G" : "";
    }

    void UpdateGameStatus()
    {
        if (gameStateContents.gameStatus == GameStatus.DEAD ||
            gameStateContents.gameStatus == GameStatus.FINISHED)
        {
            topRowInner.style.display = DisplayStyle.None;
            rankOuter.style.display = DisplayStyle.Flex;
            var rank = gameStateContents.gameStatus == GameStatus.FINISHED ?
                "Blue Max class 1" :
                "Kamikaze trainee class 4";
            rankLabel.text = $"Rank: {rank}";
            if (gameStateContents.gameStatus == GameStatus.FINISHED)
            {
                audioSource.PlayOneShot(rulebritanniaClip);
            }
        }
        else
        {
            topRowInner.style.display = DisplayStyle.Flex;
            rankOuter.style.display = DisplayStyle.None;
        }
    }

    void Reset()
    {
        UpdateAll();
    }

    void UpdateAll()
    {
        UpdateAlert();
        UpdateAlt();
        UpdateScore();
        UpdateGameStatus();
        UpdateDamage();
        UpdateBombs();
        UpdateDashColor();
        UpdateTargets();
    }

    void UpdateFuel()
    {
        var fuel = (int)((gameStateContents.fuel * maxFuelDisplayed) / gameState.maxFuel);
        if (fuel != lastDisplayedFuel)
        {
            lastDisplayedFuel = fuel;
            fuelLabel.text = $"{fuel:000}";
        }
    }

    public void OnGameStatusChanged(GameStatus gameStatus)
    {
        if (gameStatus == GameStatus.REPAIRING ||
            gameStatus == GameStatus.LOADING_BOMBS)
        {
            audioSource.clip = bingClip;
            audioSource.Play();
        }
        UpdateGameStatus();
        UpdateAlert();
    }

    private void SetupCallbacks()
    {
        gameState.Subscribe(GameEvent.GAME_STATUS_CHANGED, () => OnGameStatusChanged(gameStateContents.gameStatus));
        gameState.Subscribe(GameEvent.START, Reset);
        gameState.Subscribe(GameEvent.SPEED_CHANGED, UpdateSpeed);
        gameState.Subscribe(GameEvent.ALT_CHANGED, UpdateAlt);
        gameState.Subscribe(GameEvent.BOMBS_CHANGED, UpdateBombs);
        gameState.Subscribe(GameEvent.SCORE_CHANGED, UpdateScore);
        gameState.Subscribe(GameEvent.TARGETS_CHANGED, UpdateTargets);
        gameState.Subscribe(GameEvent.TARGET_HIT, UpdateTargets);
        gameState.Subscribe(GameEvent.DAMAGE_SUSTAINED, () => {
            audioSource.PlayOneShot(damageClip);
            UpdateDamage();
        });
        gameState.Subscribe(GameEvent.DAMAGE_REPAIRED, () => {
            audioSource.PlayOneShot(bingClip);
            UpdateDamage();
        });
        gameState.Subscribe(GameEvent.LANDING_CHANGED, () => {
            if (gameStateContents.approachingLanding)
            {
                audioSource.PlayOneShot(alertClip);
            }
            UpdateAlert();
        });
        gameState.Subscribe(GameEvent.WIND_CHANGED, UpdateAlert);
        gameState.Subscribe(GameEvent.SMALL_BANG, () => audioSource.PlayOneShot(bangSmallClip));
        gameState.Subscribe(GameEvent.MEDIUM_BANG, () => audioSource.PlayOneShot(bangMediumClip));
        gameState.Subscribe(GameEvent.BIG_BANG, () => audioSource.PlayOneShot(bangBigClip));
        gameState.Subscribe(GameEvent.ENEMY_PLANE_STATUS_CHANGED, OnEnemyPlaneStatusChanged);
    }    

    public void OnEnemyPlaneStatusChanged()
    {
        UpdateDashColor();
        UpdateAlert();
    }
}
