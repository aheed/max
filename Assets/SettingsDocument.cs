using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SettingsDocument : MonoBehaviour
{
    VisualElement closeElem;
    
    static SettingsDocument GetInstance() => FindObjectOfType<SettingsDocument>(true);
    public static void SetOpenState(bool open) => 
        GetInstance().gameObject.SetActive(open);
    
    // Start is called before the first frame update
    void OnEnable()
    {
        Debug.Log("Settings dialog started");
        var uiDocument = GetComponent<UIDocument>();

        closeElem = uiDocument.rootVisualElement.Q<VisualElement>("CloseButton");
        closeElem.RegisterCallback<ClickEvent>(OnCloseClicked);
    }

    void OnCloseClicked(ClickEvent evt)
    {
        Debug.Log("Settings dialog Close button clicked");
        if (evt.propagationPhase != PropagationPhase.AtTarget)
        return;
        
        SetOpenState(false);
    }
}
