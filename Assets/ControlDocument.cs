using UnityEngine;
using UnityEngine.UIElements;

public class ControlDocument : MonoBehaviour
{
    VisualElement fireElem;
    //GameState gameState;
    GameStateContents gameStateContents;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameStateContents = GameState.GetInstance().GetStateContents();
        var uiDocument = GetComponent<UIDocument>();
        
        fireElem = uiDocument.rootVisualElement.Q<VisualElement>("FireButton");
        fireElem.RegisterCallback<PointerDownEvent>(OnFireDown);
        fireElem.RegisterCallback<PointerUpEvent>(OnFireUp);
        fireElem.RegisterCallback<PointerOutEvent>(OnFireOut);        
    }

    // Update is called once per frame
    void Update()
    {
        //fireElem.
    }

    void OnFireDown(PointerDownEvent evt)
    {
        Debug.Log("Fire button pressed");
        if (evt.target != fireElem)
            return;
        gameStateContents.firing = true;
    }

    void OnFireUp(PointerUpEvent evt)
    {
        Debug.Log("Fire button released");
        if (evt.target != fireElem)
            return;
        gameStateContents.firing = false;
    }

    void OnFireOut(PointerOutEvent evt)
    {
        Debug.Log("Fire button released");
        if (evt.target != fireElem)
            return;
        gameStateContents.firing = false;
    }
}
