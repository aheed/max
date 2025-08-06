using UnityEngine;

public static class Globals
{
    static bool touchScreenDetected = false;

    public static bool IsTouchScreenDetected() => touchScreenDetected;

    public static void SetTouchScreenDetected()
    {
        if (touchScreenDetected)
        {
            return;
        }

        Debug.Log("Touch screen detected");
        touchScreenDetected = true;
        GameState.GetInstance().ReportEvent(GameEvent.TOUCH_SCREEN_DETECTED);
    }
}
