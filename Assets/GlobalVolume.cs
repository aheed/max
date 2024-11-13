using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GlobalVolume : MonoBehaviour, IGameStateObserver
{
    public float hueShiftlifeSpanSec = 0.05f;
    public float postExposurelifeSpanSec = 0.2f;
    public float hueShift = -100f;
    public float maxPostExposure = 2.5f;
    float currentHueShift = 0f;
    float currentPostExposure = 0f;
    float hueShiftTimeToLiveSec;
    float postExposureTimeToLiveSec;
    ColorAdjustments colorAdjustments;
    // Start is called before the first frame update
    void Start()
    {
        var volume = GetComponent<Volume>();
        VolumeProfile p = volume.profile;
        p.TryGet(out colorAdjustments);        
        GameState gameState = FindObjectOfType<GameState>();
        gameState.RegisterObserver(this);
    }

    // Update is called once per frame
    void Update()
    {
        hueShiftTimeToLiveSec -= Time.deltaTime;
        var newHueShift = hueShift;
        if (hueShiftTimeToLiveSec <= 0f)
        {
            newHueShift = 0f;
        }
        
        if (newHueShift != currentHueShift)
        {
            currentHueShift = newHueShift;
            colorAdjustments.hueShift.SetValue( new FloatParameter(currentHueShift, true));
        }

        postExposureTimeToLiveSec -= Time.deltaTime;
        var newPostExposure = 0f;
        if (postExposureTimeToLiveSec > 0f)
        {
            newPostExposure = postExposureTimeToLiveSec * maxPostExposure / postExposurelifeSpanSec;
        }
        
        if (newPostExposure != currentPostExposure)
        {
            currentPostExposure = newPostExposure;
            colorAdjustments.postExposure.SetValue( new FloatParameter(currentPostExposure, true));
        }
    }

    public void OnGameStatusChanged(GameStatus gameStatus) {}

    public void OnGameEvent(GameEvent ge) {
        if (ge != GameEvent.SMALL_DETONATION)
        {
            return;
        }
        
        hueShiftTimeToLiveSec = hueShiftlifeSpanSec;
        postExposureTimeToLiveSec = postExposurelifeSpanSec;
    }

    public void OnBombLanded(Bomb bomb, GameObject hitObject) {}

    public void OnEnemyPlaneStatusChanged(EnemyPlane enemyPlane, bool active) {}
}
