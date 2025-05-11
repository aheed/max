using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    VisualElement buttonBarUIElem;
    VisualElement tvElem;
    VisualElement fullScreenElem;
    VisualElement dotsElem;
    VisualElement muteElem;
    VisualElement pilotElem;
    VisualElement helpElem;
    VisualElement fullScreenTapHintElem;
    GameState gameState;
    bool fullScreen = false; //expected state, could change any time
    bool rightSideExpanded = false;
    List<VisualElement> expandedRightSide = new();
    
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

        fullScreenTapHintElem = uiDocument.rootVisualElement.Q<VisualElement>("FullScreenTapHint");

        muteElem = uiDocument.rootVisualElement.Q<VisualElement>("MuteButton");
        muteElem.RegisterCallback<ClickEvent>(OnMuteClicked);

        pilotElem = uiDocument.rootVisualElement.Q<VisualElement>("PilotButton");
        pilotElem.RegisterCallback<ClickEvent>(OnPilotClicked);

        helpElem = uiDocument.rootVisualElement.Q<VisualElement>("HelpButton");
        helpElem.RegisterCallback<ClickEvent>(OnHelpClicked);

        dotsElem = uiDocument.rootVisualElement.Q<VisualElement>("DotsButton");
        dotsElem.RegisterCallback<ClickEvent>(OnDotsClicked);

        expandedRightSide.Add(muteElem);
        expandedRightSide.Add(pilotElem);
        expandedRightSide.Add(helpElem);
        

        UpdateAll();
    }

    void UpdateAll()
    {
        UpdateTvSimButton();
        UpdateFullScreenButton();
        UpdateMuteButton();
        UpdatePilotButton();
        UpdateRightSideExpanded();
    }

    void UpdateTvSimButton()
    {
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

    void OnMuteClicked(ClickEvent evt)
    {
        Debug.Log("Mute clicked");
        if (evt.target != muteElem)
        return;
        
        Settings.SetMute(!Settings.GetMute());
        
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

    void UpdateRightSideExpanded()
    {
        var dotsDisplayStyle = rightSideExpanded ? DisplayStyle.None : DisplayStyle.Flex;
        var expansionDisplayStyle = rightSideExpanded ? DisplayStyle.Flex : DisplayStyle.None;

        dotsElem.style.display = dotsDisplayStyle;
        foreach (var item in expandedRightSide)
        {
            item.style.display = expansionDisplayStyle;
        }
    }

    void OnDotsClicked(ClickEvent evt)
    {
        Debug.Log("Dots clicked");
        if (evt.target != dotsElem)
        return;

        rightSideExpanded = true;
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
}
