using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UserGuide : MonoBehaviour
{
    Button gotItButtonElem;
    VisualElement closeElem;
    
    static UserGuide GetInstance() => FindAnyObjectByType<UserGuide>(FindObjectsInactive.Include);
    public static void SetOpenState(bool open) => 
        GetInstance().gameObject.SetActive(open);
    
    // Start is called before the first frame update
    void OnEnable()
    {
        Debug.Log("User Guide started");
        var uiDocument = GetComponent<UIDocument>();

        closeElem = uiDocument.rootVisualElement.Q<VisualElement>("CloseButton");
        closeElem.RegisterCallback<ClickEvent>(OnCloseClicked);
    }

    void OnCloseClicked(ClickEvent evt)
    {
        Debug.Log("Close button clicked");
        if (evt.target != closeElem)
        return;
        
        SetOpenState(false);
        Settings.SetUserGuideHasBeenDisplayed();
    }

}
