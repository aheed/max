using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class prop : MonoBehaviour
{
    public Sprite leftSprite;
    public Sprite rightSprite;
    private SpriteRenderer spriteR;
    public float spriteSwapIntervalSeconds = 0.1f;
    private float spriteSwapCooldown = 0.0f;
    private GameState gameState;

    // Start is called before the first frame update
    void Start()
    {
        spriteR = gameObject.GetComponent<SpriteRenderer>();
        gameState = GameState.GetInstance();
        gameState.Subscribe(GameEvent.GAME_STATUS_CHANGED, OnGameStatusChanged);
    }

    // Update is called once per frame
    void Update()
    {
        spriteSwapCooldown -= Time.deltaTime;
        if (spriteSwapCooldown < 0.0f)
        {
            spriteR.sprite = spriteR.sprite == leftSprite ? rightSprite : leftSprite;
            spriteSwapCooldown = spriteSwapIntervalSeconds;
        }
    }

    void OnDestroy()
    {
        gameState.Unsubscribe(GameEvent.GAME_STATUS_CHANGED, OnGameStatusChanged);
    }

    void OnGameStatusChanged()
    {
        var gameStatus = gameState.GetStateContents().gameStatus;
        gameObject.SetActive(!(gameStatus == GameStatus.DEAD || gameStatus == GameStatus.KILLED_BY_FLACK));
    }
}
