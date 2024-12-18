using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class prop : MonoBehaviour, IGameStateObserver
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
        FindAnyObjectByType<GameState>().RegisterObserver(this);
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
        FindAnyObjectByType<GameState>().UnregisterObserver(this);
    }

    public void OnGameStatusChanged(GameStatus gameStatus)
    {
        gameObject.SetActive(!(gameStatus == GameStatus.DEAD || gameStatus == GameStatus.KILLED_BY_FLACK));
    }

    public void OnGameEvent(GameEvent _) {}

    public void OnBombLanded(Bomb bomb, GameObject hitObject) {}
    public void OnEnemyPlaneStatusChanged(EnemyPlane enemyPlane, bool active) {}
}
