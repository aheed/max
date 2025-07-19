using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GlobalVolume : MonoBehaviour
{
    public float hueShiftlifeSpanSec = 0.05f;
    public float postExposurelifeSpanSec = 0.2f;
    public float hueShift = -100f;
    public float maxPostExposure = 2.5f;
    public float nightSaturation = -30f;
    public float tvSaturation = -17f;
    public float tvBloomIntensity = 2f;
    public float nightBloomIntensity = 8f;
    float currentHueShift = 0f;
    float currentPostExposure = 0f;
    float hueShiftTimeToLiveSec;
    float postExposureTimeToLiveSec;
    ColorAdjustments colorAdjustments;
    LensDistortion lensDistortion;
    ChromaticAberration chromaticAberration;
    FilmGrain filmGrain;
    Bloom bloom;
    Vignette vignette;
    // Start is called before the first frame update
    void Start()
    {
        var volume = GetComponent<Volume>();
        VolumeProfile p = volume.profile;
        p.TryGet(out colorAdjustments);        
        p.TryGet(out lensDistortion);
        p.TryGet(out chromaticAberration);
        p.TryGet(out filmGrain);
        p.TryGet(out bloom);
        p.TryGet(out vignette);
        GameState gameState = GameState.GetInstance();
        gameState.Subscribe(GameEvent.VIEW_MODE_CHANGED, UpdateViewMode);
        gameState.Subscribe(GameEvent.SMALL_DETONATION, OnSmallDetonation);
        UpdateViewMode();
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

    void UpdateViewMode()
    {
        var viewMode = GameState.GetInstance().viewMode;
        if (viewMode == ViewMode.NORMAL)
        {
            lensDistortion.active = false;
            chromaticAberration.active = false;
            filmGrain.active = false;
            if (GameState.GetInstance().IsNightTime())
            {
                bloom.active = true;
                bloom.threshold.overrideState = true;
                bloom.intensity.overrideState = true;
                bloom.intensity.SetValue(new FloatParameter(nightBloomIntensity, true));
            }
            else
            {
                bloom.active = false;
            }
            vignette.active = false;
            colorAdjustments.saturation.overrideState = GameState.GetInstance().IsNightTime();
            colorAdjustments.saturation.SetValue(new FloatParameter(nightSaturation, true));
        }
        else if (viewMode == ViewMode.TV_SIM)
        {
            lensDistortion.active = true;
            chromaticAberration.active = true;
            filmGrain.active = true;
            bloom.active = true;
            bloom.threshold.overrideState = false;
            bloom.intensity.overrideState = true;
            bloom.intensity.SetValue(new FloatParameter(tvBloomIntensity, true));
            vignette.active = true;
            colorAdjustments.saturation.overrideState = true;
            var possibleNightSaturation = GameState.GetInstance().IsNightTime() ? nightSaturation : 0f;
            colorAdjustments.saturation.SetValue(new FloatParameter(tvSaturation + possibleNightSaturation, true));
        }
    }

    void OnSmallDetonation() {
        hueShiftTimeToLiveSec = hueShiftlifeSpanSec;
        postExposureTimeToLiveSec = postExposurelifeSpanSec;
    }
}
