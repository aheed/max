using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class ControlDocument : MonoBehaviour
{
    static readonly string swipeDownAnimationClass = "swipe-hint-down-animation";
    static readonly string swipeUpAnimationClass = "swipe-hint-up-animation";
    static readonly float delayAnimationShortSec = 0.3f;
    static readonly float delayAnimationLongSec = 1.6f;

    VisualElement upSwipeHintElem;
    VisualElement downSwipeHintElem;
    VisualElement fireTapHintElem;
    VisualElement fullScreenTapHintElem;
    //GameState gameState;
    GameStateContents gameStateContents;
    bool fireHintVisible = false;
    bool upSwipeHintVisible = false;
    bool downSwipeHintVisible = false;
    bool fullscreenTapHintVisible = false;

    public void SetFullScreenTapHintElement(VisualElement tapHintElem)
    {
        fullScreenTapHintElem = tapHintElem;
    }

    public void SetFullScreenTapHintVisible(bool visible)
    {
        fullscreenTapHintVisible = visible;
    }

    public void SetFireHintVisible(bool visible)
    {
        fireHintVisible = visible;
    }

    public void SetUpSwipeHintVisible(bool visible)
    {
        upSwipeHintVisible = visible;
    }
    
    public void SetDownSwipeHintVisible(bool visible)
    {
        downSwipeHintVisible = visible;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameStateContents = GameState.GetInstance().GetStateContents();
        var uiDocument = GetComponent<UIDocument>();

        downSwipeHintElem = uiDocument.rootVisualElement.Q<VisualElement>("DownSwipeHint");
        upSwipeHintElem = uiDocument.rootVisualElement.Q<VisualElement>("UpSwipeHint");
        fireTapHintElem = uiDocument.rootVisualElement.Q<VisualElement>("FireTapHint");

        //SetupSwipeAnimation();
        StartCoroutine(HintCoroutine());
    }

    IEnumerator HintCoroutine() {
        WaitForSeconds waitShort = new WaitForSeconds(delayAnimationShortSec);
        WaitForSeconds waitLong = new WaitForSeconds(delayAnimationLongSec);

        while (true) {
            upSwipeHintElem.RemoveFromClassList(swipeUpAnimationClass);
            downSwipeHintElem.RemoveFromClassList(swipeDownAnimationClass);
            upSwipeHintElem.visible = false;
            downSwipeHintElem.visible = false;
            fireTapHintElem.visible = false;
            if (fullScreenTapHintElem != null)
            {
                fullScreenTapHintElem.visible = false;
            }
            //Debug.Log("Hint Coroutine 1");
            yield return waitShort;
            
            upSwipeHintElem.AddToClassList(swipeUpAnimationClass);
            downSwipeHintElem.AddToClassList(swipeDownAnimationClass);
            upSwipeHintElem.visible = upSwipeHintVisible;
            downSwipeHintElem.visible = downSwipeHintVisible;
            fireTapHintElem.visible = fireHintVisible;
            if (fullScreenTapHintElem != null)
            {
                fullScreenTapHintElem.visible = fullscreenTapHintVisible;
            }
            //Debug.Log("Hint Coroutine 2");
            yield return waitLong;
        }
    }

}
