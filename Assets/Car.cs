using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class Car : MonoBehaviour
{
    public float speedFactor = 1.0f;
    public Sprite[] sprites;
    private SpriteRenderer spriteR;
    GameState gameState;
    float speed;


    // Start is called before the first frame update
    void Start()
    {
        spriteR = gameObject.GetComponent<SpriteRenderer>();
        gameState = FindObjectOfType<GameState>();
        speed = speedFactor * gameState.maxSpeed;
        var spriteIndex = Random.Range(0, sprites.Length);
        spriteR.sprite = sprites[spriteIndex];
    }

    // Update is called once per frame
    void Update()
    {
        var progX = speed * Time.deltaTime;
        Vector3 progress = new (progX, 0f, 0f);
        transform.position += progress;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.name.StartsWith("bomb") ||
            col.name.StartsWith("mushroom", true, CultureInfo.InvariantCulture))
        {
            var bomb = col.gameObject.GetComponent<Bomb>();
            gameState.BombLanded(bomb, gameObject);
            return;
        }
    }
}
