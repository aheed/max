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
    VisualElement tvElem;
    VisualElement fullScreenElem;
    VisualElement muteElem;
    VisualElement helpElem;
    GameState gameState;
    float audioVolume;
    bool fullScreen = false; //expected state, could change any time
    

    // Start is called before the first frame update
    void Start()
    {
        gameState = FindObjectOfType<GameState>();
        var uiDocument = GetComponent<UIDocument>();

        tvElem = uiDocument.rootVisualElement.Q<VisualElement>("TvButton");
        tvElem.RegisterCallback<ClickEvent>(OnTVClicked);

        fullScreenElem = uiDocument.rootVisualElement.Q<VisualElement>("FullScreenButton");
        fullScreenElem.RegisterCallback<PointerDownEvent>(OnFullScreenClicked);

        muteElem = uiDocument.rootVisualElement.Q<VisualElement>("MuteButton");
        muteElem.RegisterCallback<ClickEvent>(OnMuteClicked);

        helpElem = uiDocument.rootVisualElement.Q<VisualElement>("HelpButton");
        helpElem.RegisterCallback<ClickEvent>(OnHelpClicked);

        UpdateAll();
    }

    void UpdateAll()
    {
        UpdateTvSimButton();
        UpdateFullScreenButton();
        UpdateMuteButton();
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
        var newTexture = AudioListener.volume == 0 ? volumeMuteTexture : volumeUpTexture;
        muteElem.style.backgroundImage = new StyleBackground(newTexture);        
    }

    void Update()
    {
        CheckFullScreenButton();
    }

    void OnTVClicked(ClickEvent evt)
    {
        Debug.Log("TV sim toggle clicked");
        if (evt.propagationPhase != PropagationPhase.AtTarget)
        return;
        
        gameState.SetViewMode(gameState.viewMode == ViewMode.NORMAL ? ViewMode.TV_SIM : ViewMode.NORMAL);
        UpdateTvSimButton();
    }

    void OnFullScreenClicked(PointerDownEvent evt)
    {
        Debug.Log("Fullscreen toggle clicked");
        // Only perform this action at the target, not in a parent
        if (evt.propagationPhase != PropagationPhase.AtTarget)
        return;

        Screen.fullScreen = !Screen.fullScreen;
        //UpdateFullScreenButton();
    }

    void OnMuteClicked(ClickEvent evt)
    {
        Debug.Log("Mute clicked");
        if (evt.propagationPhase != PropagationPhase.AtTarget)
        return;
        
        var audioListener = FindObjectOfType<AudioListener>(true); //assume there is only one
        if (AudioListener.volume == 0)
        {
            AudioListener.volume = audioVolume;
        }
        else
        {
            audioVolume = AudioListener.volume;
            AudioListener.volume = 0;
        }
        
        //AudioListener.pause = !AudioListener.pause;
        UpdateMuteButton();
    }

    void OnHelpClicked(ClickEvent evt)
    {
        Debug.Log("Help clicked");
        if (evt.propagationPhase != PropagationPhase.AtTarget)
        return;
        
        FindObjectOfType<UserGuide>(true).gameObject.SetActive(true);
    }
}
