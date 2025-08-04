using UnityEngine;

public class ScreenHelper : MonoBehaviour
{
    static int oldWidth = -1;
    static int oldHeight = -1;

    static public void SetFullScreen(bool fullScreen)
    {
        if (fullScreen)
        {
            oldWidth = Screen.width;
            oldHeight = Screen.height;

            // Obtain the native screen resolution
            int screenWidth = Screen.currentResolution.width;
            int screenHeight = Screen.currentResolution.height;

            // Set the resolution to native resolution and enable fullscreen
            Screen.SetResolution(screenWidth, screenHeight, true);
        }
        else
        {
            if (oldWidth == -1 || oldHeight == -1)
            {
                // Just go to windowed mode without changing resolution
                Screen.fullScreen = false;
            }
            else
            {
                // Restore the previous resolution and disable fullscreen
                Screen.SetResolution(oldWidth, oldHeight, false);
            }
        }
    }

    static public bool IsFullScreen()
    {
        return Screen.fullScreen;
    }
}