using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ButtonBarDocument : MonoBehaviour
{
    public Texture2D crtTexture;
    public Texture2D flatscreenTexture;
    VisualElement tvElem;
    VisualElement fullScreenElem;
    GameState gameState;
    

    // Start is called before the first frame update
    void Start()
    {
        gameState = FindObjectOfType<GameState>();
        var uiDocument = GetComponent<UIDocument>();

        tvElem = uiDocument.rootVisualElement.Q<VisualElement>("TvButton");
        tvElem.RegisterCallback<ClickEvent>(OnTVClicked);

        fullScreenElem = uiDocument.rootVisualElement.Q<VisualElement>("FullScreenButton");
        fullScreenElem.RegisterCallback<ClickEvent>(OnFullScreenClicked);
    }

    void UpdateAll()
    {
        UpdateTvSimButton();
    }

    void UpdateTvSimButton()
    {
        var newTexture = gameState.viewMode == ViewMode.NORMAL ? crtTexture : flatscreenTexture;
        tvElem.style.backgroundImage = new StyleBackground(newTexture);
    }

    void OnTVClicked(ClickEvent evt)
    {
        Debug.Log("TV sim toggle clicked");
        if (evt.propagationPhase != PropagationPhase.AtTarget)
        return;
        
        gameState.SetViewMode(gameState.viewMode == ViewMode.NORMAL ? ViewMode.TV_SIM : ViewMode.NORMAL);
        UpdateTvSimButton();
    }

    void OnFullScreenClicked(ClickEvent evt)
    {
        Debug.Log("Fullscreen toggle clicked");
        // Only perform this action at the target, not in a parent
        if (evt.propagationPhase != PropagationPhase.AtTarget)
        return;

        Screen.fullScreen = !Screen.fullScreen;
    }

    
}
