using UnityEngine;
using UnityEngine.UIElements;

public class ControlDocument : MonoBehaviour
{
    VisualElement fireElem;
    GameState gameState;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameState = GameState.GetInstance();
        var uiDocument = GetComponent<UIDocument>();
        
        fireElem = uiDocument.rootVisualElement.Q<VisualElement>("FireButton");
        fireElem.RegisterCallback<PointerDownEvent>(OnFireDown);
        fireElem.RegisterCallback<PointerUpEvent>(OnFireUp);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnFireDown(PointerDownEvent evt)
    {
        Debug.Log("Fire button pressed");
        if (evt.target != fireElem)
            return;        
    }

    void OnFireUp(PointerUpEvent evt)
    {
        Debug.Log("Fire button released");
        if (evt.target != fireElem)
            return;        
    }
}
