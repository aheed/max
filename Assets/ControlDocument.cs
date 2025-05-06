using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class ControlDocument : MonoBehaviour
{
    static readonly string swipeDownAnimationClass = "swipe-hint-down-animation";
    static readonly int delayRerunAnimationMs = 600;
    static readonly float delayAnimationShortSec = 0.3f;
    static readonly float delayAnimationLongSec = 1.6f;

    VisualElement fireElem;
    VisualElement downSwipeHintElem;
    VisualElement fireTapHintElem;
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

        downSwipeHintElem = uiDocument.rootVisualElement.Q<VisualElement>("DownSwipeHint");
        fireTapHintElem = uiDocument.rootVisualElement.Q<VisualElement>("FireTapHint");

        //SetupSwipeAnimation();
        StartCoroutine(HintCoroutine());
    }

    void SetupSwipeAnimation()
    {
        downSwipeHintElem.RemoveFromClassList(swipeDownAnimationClass);
        downSwipeHintElem.RegisterCallback<TransitionEndEvent>(evt =>
        {
            //downSwipeHintElem.RemoveFromClassList(swipeDownAnimationClass);
            downSwipeHintElem.schedule.Execute(() => downSwipeHintElem.RemoveFromClassList(swipeDownAnimationClass)).StartingIn(delayRerunAnimationMs);
            downSwipeHintElem.schedule.Execute(() => downSwipeHintElem.AddToClassList(swipeDownAnimationClass)).StartingIn(delayRerunAnimationMs + 10);
        });
        downSwipeHintElem.schedule.Execute(() => downSwipeHintElem.AddToClassList(swipeDownAnimationClass)).StartingIn(100);
    }

    IEnumerator SomeCoroutine() {
        //Declare a yield instruction.
        WaitForSeconds wait = new WaitForSeconds(3);

        //for(int i = 0; i < 10; i++) {
        while (true) {
            Debug.Log("Coroutine runningggggggggggggggggggggg");
            yield return wait; //Pause the loop for 3 seconds.
        }
    }

    IEnumerator HintCoroutine() {
        WaitForSeconds waitShort = new WaitForSeconds(delayAnimationShortSec);
        WaitForSeconds waitLong = new WaitForSeconds(delayAnimationLongSec);

        while (true) {
            downSwipeHintElem.RemoveFromClassList(swipeDownAnimationClass);
            downSwipeHintElem.visible = false;
            fireTapHintElem.visible = false;
            Debug.Log("Hint Coroutine 1");
            yield return waitShort;
            downSwipeHintElem.visible = true;
            downSwipeHintElem.AddToClassList(swipeDownAnimationClass);
            fireTapHintElem.visible = true;
            Debug.Log("Hint Coroutine 2");
            yield return waitLong;
        }
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
