using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandingStrip : MonoBehaviour
{
    GameState gameState;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (gameState == null)
        {
            gameState = FindObjectOfType<GameState>();
        }    
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.name.StartsWith("bomb"))
        {
            return;
        }

        var bomb = col.gameObject.GetComponent<Bomb>();
        var tmp = new GameObject("tmp"); // Pass a throwaway game object to indicate something was hit
        tmp.transform.position = bomb.transform.position;
        gameState.BombLanded(bomb, tmp);

        // To do: Report bombed enemy airstrip for scoring
    }
}
