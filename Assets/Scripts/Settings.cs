
using UnityEngine;

public static class Settings
{
    static readonly string seenPrefsKey = "userGuideSeen";
    static readonly string controlPrefsKey = "control";
    static readonly string mutePrefsKey = "mute";
    static readonly string selectedMissionPrefsKey = "selectedMission";

    public static bool UserGuideHasBeenDisplayed() => PlayerPrefs.GetInt(seenPrefsKey) != 0;
    public static void SetUserGuideHasBeenDisplayed() => PlayerPrefs.SetInt(seenPrefsKey, 1);

    static float audioVolume = 1f;


    public static bool GetPilotControl()
    {
        if (!PlayerPrefs.HasKey(controlPrefsKey))
        {
            PlayerPrefs.SetInt(controlPrefsKey, 1);
        }
        
        return PlayerPrefs.GetInt(controlPrefsKey) != 0;
    }
    public static void SetPilotControl(bool pilot) => PlayerPrefs.SetInt(controlPrefsKey, pilot ? 1 : 0);

    public static int GetSelectedMission() => PlayerPrefs.GetInt(selectedMissionPrefsKey, 0);
    public static void SetSelectedMission(int mission) => PlayerPrefs.SetInt(selectedMissionPrefsKey, mission);

    public static bool GetMute() => PlayerPrefs.GetInt(mutePrefsKey) != 0;
    public static void SetMute(bool mute)
    {
        var audioListener = GameObject.FindAnyObjectByType<AudioListener>(FindObjectsInactive.Include); //assume there is only one

        if (mute && AudioListener.volume != 0f)
        {
            audioVolume = AudioListener.volume;
            AudioListener.volume = 0;
        }
        else if (!mute && AudioListener.volume == 0f)
        {
            AudioListener.volume = audioVolume;
        }

        PlayerPrefs.SetInt(mutePrefsKey, mute ? 1 : 0);
    }

    // Get all settings from permanent storage and
    // make them take effect accordingly.
    public static void Update()
    {        
        SetMute(GetMute());
    }
}