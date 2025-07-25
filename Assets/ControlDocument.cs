using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class ControlDocument : MonoBehaviour
{
    public Texture2D fireTexture;
    public Texture2D startGameTexture;
    static readonly string swipeDownAnimationClass = "swipe-hint-down-animation";
    static readonly string swipeUpAnimationClass = "swipe-hint-up-animation";
    static readonly float delayAnimationShortSec = 0.3f;
    static readonly float delayAnimationLongSec = 1.6f;

    VisualElement upSwipeHintElem;
    VisualElement downSwipeHintElem;
    VisualElement fireTapHintElem;
    VisualElement fullScreenTapHintElem;
    VisualElement fireElem;
    GameStateContents gameStateContents;
    bool fireHintVisible = false;
    bool upSwipeHintVisible = false;
    bool downSwipeHintVisible = false;
    bool fullscreenTapHintVisible = false;
    bool fireButtonVisible = true;
    private Coroutine hintCoroutine;

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

    public void SetFireButtonVisible(bool visible)
    {
        fireButtonVisible = visible;        
        UpdateFireButton();
    }

    void Start()
    {
        GameState.GetInstance().Subscribe(GameEvent.GAME_STATUS_CHANGED, OnGameStatusChangedCallback);
        GameState.GetInstance().Subscribe(GameEvent.RESTART_TIMER_EXPIRED, OnGameStatusChangedCallback);
        GameState.GetInstance().Subscribe(GameEvent.START, OnGameStartCallback);

        StartGame();
    }

    void UpdateFireButton()
    {
        if (fireElem != null)
        {
            var newTexture = GameState.GetInstance().IsRestartAllowed() ? startGameTexture : fireTexture;
            fireElem.style.backgroundImage = new StyleBackground(newTexture);
            fireElem.style.display = fireButtonVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    private void OnGameStatusChangedCallback()
    {
        UpdateFireButton();
    }

    private void OnGameStartCallback()
    {
        //Debug.Log("ControlDocument.OnGameStartCallback");
        StartGame();
    }

    private void StartGame()
    {
        if (!gameObject.activeSelf)
        {
            return;
        }

        gameStateContents = GameState.GetInstance().GetStateContents();
        var uiDocument = GetComponent<UIDocument>();

        downSwipeHintElem = uiDocument.rootVisualElement.Q<VisualElement>("DownSwipeHint");
        upSwipeHintElem = uiDocument.rootVisualElement.Q<VisualElement>("UpSwipeHint");
        fireTapHintElem = uiDocument.rootVisualElement.Q<VisualElement>("FireTapHint");
        fireElem = uiDocument.rootVisualElement.Q<VisualElement>("FireButton");

        SetFireHintVisible(false);
        SetUpSwipeHintVisible(false);
        SetDownSwipeHintVisible(false);
        SetFullScreenTapHintVisible(false);
        UpdateFireButton();

        if (hintCoroutine != null)
        {
            StopCoroutine(hintCoroutine);
            hintCoroutine = null;
        }
        hintCoroutine = StartCoroutine(HintCoroutine());
    }

    void OnEnable()
    {
        //Debug.Log("ControlDocument.OnEnable");
        StartGame(); // Apparently needed to reinitialize UI elements references
    }

    IEnumerator HintCoroutine() {
        CustomYieldInstruction waitShort = new WaitForSecondsRealtime(delayAnimationShortSec);
        CustomYieldInstruction waitLong = new WaitForSecondsRealtime(delayAnimationLongSec);

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
