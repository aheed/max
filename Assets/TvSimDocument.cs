using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TvSimDocument : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        OnViewModeChanged();
        GameState.GetInstance().Subscribe(GameEvent.VIEW_MODE_CHANGED, OnViewModeChanged);
    }

    public void OnViewModeChanged()
    {
        gameObject.SetActive(GameState.GetInstance().viewMode == ViewMode.TV_SIM);
    }
}
