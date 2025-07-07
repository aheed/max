using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandingStrip : MonoBehaviour
{
    static readonly int points = 10;

    // Start is called before the first frame update
    void Start()
    {

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
        GameState.GetInstance().BombLanded(bomb, tmp);
        GameState.GetInstance().AddScore(points);
    }
}
