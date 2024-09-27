using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateContents
{
    public float speed = 0f;    
    public GameStatus gameStatus = GameStatus.ACCELERATING;
}

public class GameState : MonoBehaviour
{
    public float maxSpeed = 2.0f;
    public float acceleration = 0.4f;
    GameStateContents gameStateContents = new GameStateContents();
    public GameStateContents GetStateContents() => gameStateContents;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
