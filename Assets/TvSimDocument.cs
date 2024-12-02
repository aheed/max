using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TvSimDocument : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        OnViewModeChanged();
    }

    public void OnViewModeChanged()
    {
        gameObject.SetActive(FindObjectOfType<GameState>().viewMode == ViewMode.TV_SIM);
    }
}
