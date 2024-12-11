using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UserGuide : MonoBehaviour
{
    Button gotItButtonElem;
    
    // Start is called before the first frame update
    void OnEnable()
    {
        Debug.Log("User Guide started");
        var uiDocument = GetComponent<UIDocument>();

        gotItButtonElem = uiDocument.rootVisualElement.Q<Button>("GotIt");
        gotItButtonElem.RegisterCallback<ClickEvent>(OnGotItClicked);
    }

    void OnGotItClicked(ClickEvent evt)
    {
        Debug.Log("Got it button clicked");
        if (evt.propagationPhase != PropagationPhase.AtTarget)
        return;
        
        gameObject.SetActive(false);
    }

}
