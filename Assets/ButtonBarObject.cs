using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ButtonBarObject : MonoBehaviour
{
    Button fullScreenButton;
    VisualElement fullScreenElem;

    // Start is called before the first frame update
    void Start()
    {
        var uiDocument = GetComponent<UIDocument>();
        fullScreenElem = uiDocument.rootVisualElement.Q<VisualElement>("FullScreenElement");
        fullScreenButton = uiDocument.rootVisualElement.Q<Button>("FullScreenButton");        
        fullScreenElem.RegisterCallback<ClickEvent>(OnFullScreenClicked, TrickleDown.TrickleDown);
        fullScreenButton.RegisterCallback<ClickEvent>(OnFullScreenButtonClicked, TrickleDown.TrickleDown);
        //fullScreenButton.clicked += TmpAction;
        fullScreenButton.SetEnabled(true);
        Debug.Log("Click callbacks registered");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void TmpAction()
    {
        Debug.Log("Button clicked callback");
    }

    public void OnFullScreenButtonClicked(ClickEvent evt)
    {
        Debug.Log("Fullscreen button clicked");
        // Only perform this action at the target, not in a parent
        if (evt.propagationPhase != PropagationPhase.AtTarget)
        return;

        Screen.fullScreen = !Screen.fullScreen;
    }

    public void OnFullScreenClicked(ClickEvent evt)
    {
        Debug.Log("Fullscreen toggle clicked");
        // Only perform this action at the target, not in a parent
        if (evt.propagationPhase != PropagationPhase.AtTarget)
        return;

        Screen.fullScreen = !Screen.fullScreen;
    }
}
