using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    public float speed = 1.5f;    
    private SpriteRenderer spriteR;
    GameState gameState;


    // Start is called before the first frame update
    void Start()
    {
        spriteR = gameObject.GetComponent<SpriteRenderer>();
        gameState = FindObjectOfType<GameState>();
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
        if (col.name.StartsWith("bomb"))
        {
            var bomb = col.gameObject.GetComponent<Bomb>();
            gameState.BombLanded(bomb, gameObject);
            return;
        }
    }
}
