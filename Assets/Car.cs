using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class Car : ManagedObject, IVip
{
    public float speedFactor = 1.0f;
    public Sprite[] sprites;
    public static readonly Color[] colors = {new Color(0f, 0f, 0f), new Color(1f, 1f, 0f)};
    private SpriteRenderer spriteR;
    GameState gameState;
    float speed;
    VipBlinker vipBlinker;
    static readonly int points = 10;


    public void SetVip()
    {
        vipBlinker = new(gameObject.GetComponent<SpriteRenderer>());
    }

    public bool IsVip()
    {
        return vipBlinker != null;
    }

    // Update is called once per frame
    void Update()
    {
        var progX = speed * Time.deltaTime;
        Vector3 progress = new (progX, 0f, 0f);
        transform.position += progress;
        vipBlinker?.Update(Time.deltaTime);
    }

    int GetPoints()
    {
        return IsVip() ? points * 2 : points;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.name.StartsWith("bomb") ||
            col.name.StartsWith("mushroom", true, CultureInfo.InvariantCulture))
        {
            var bomb = col.gameObject.GetComponent<Bomb>();
            gameState.BombLanded(bomb, gameObject);
            if (IsVip())
            {
                gameState.TargetHit();
            }
            gameState.AddScore(GetPoints());
        }
    }

    // Override
    public override void Deactivate()
    {
        vipBlinker = null;
        gameObject.SetActive(false);
    }

    public override void Reactivate()
    {
        spriteR = gameObject.GetComponent<SpriteRenderer>();
        gameState = GameState.GetInstance();
        speed = speedFactor * gameState.maxSpeed;
        var spriteIndex = Random.Range(0, sprites.Length);
        spriteR.sprite = sprites[spriteIndex];
        var colorIndex = Random.Range(0, colors.Length);
        spriteR.color = colors[colorIndex];
        gameObject.SetActive(true);
    }
}
