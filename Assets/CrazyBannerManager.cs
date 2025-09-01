using System.Linq;
using CrazyGames;
using UnityEngine;

public class CrazyBannerManager : MonoBehaviour
{
    public Vector2 bannerPosition = new Vector2(0, -200);
    public float minTimeBetweenBannerRefreshSeconds = 30f;
    private float timeOfLastBannerRefresh = -1000f;
    void Start()
    {
        if (!CrazySDK.IsInitialized || CrazySDK.Banner.Banners.Count < 1)
        {
            HideAllBanners();
            return;
        }

        var banner = CrazySDK.Banner.Banners[0];
        banner.Position = bannerPosition;

        GameState.GetInstance().Subscribe(GameEvent.GAME_STATUS_CHANGED, OnGameStatusChanged);
        GameState.GetInstance().Subscribe(GameEvent.START, RefreshBanners);
        RefreshBanners();
    }

    private void RefreshBanners()
    {
        var now = Time.fixedUnscaledTime;
        if (now - timeOfLastBannerRefresh < minTimeBetweenBannerRefreshSeconds)
        {
            Debug.Log("Skipping banner refresh");
            return;
        }

        Debug.Log("Refreshing banners");
        CrazySDK.Banner.RefreshBanners();
        timeOfLastBannerRefresh = now;
    }

    private void OnGameStatusChanged()
    {
        var gameStatus = GameState.GetInstance().GetStateContents().gameStatus;
        if (gameStatus == GameStatus.DECELERATING)
        {
            RefreshBanners();
        }
    }

    public void HideAllBanners()
    {
        CrazySDK.Banner.Banners.ForEach(b => b.gameObject.SetActive(false));
    }

    void OnDestroy()
    {
        Debug.Log("Hiding all banners");
        HideAllBanners();
    }
}
