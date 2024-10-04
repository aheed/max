using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour, IPositionObservable
{
    public float verticalSpeed = 1.9f;
    public float maxCollisionAltitude = 0.2f;

    BoxCollider2D bombCollider;
    GameState gameState;

    // Start is called before the first frame update
    void Start()
    {
        bombCollider = GetComponent<BoxCollider2D>();
        bombCollider.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameState == null)
        {
            gameState = FindObjectOfType<GameState>();
        }

        var tmpPos = transform.localPosition;
        var deltaVertical = -verticalSpeed * Time.deltaTime;
        tmpPos.z += deltaVertical; 
        tmpPos.y += deltaVertical;
        transform.localPosition = tmpPos;

        if (!bombCollider.enabled && tmpPos.z <= maxCollisionAltitude)
        {
            bombCollider.enabled = true;
        }

        if (tmpPos.z <= 0)
        {
            gameState.BombLanded(this);
        }
    }

    public Vector2 GetPosition() => transform.position;
    public float GetAltitude() => transform.localPosition.z;
    public float GetHeight() => 0.2f;
}
