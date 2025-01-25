using UnityEngine;

public class Car3d : MonoBehaviour, IVip
{
    public float speedFactor = 1.0f;    
    GameState gameState;
    float speed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameState = FindAnyObjectByType<GameState>();
        speed = speedFactor * gameState.maxSpeed;
        //Debug.Log($"Car3d speed is {speed}");
    }    

    // Update is called once per frame
    void Update()
    {
        var progX = speed * Time.deltaTime;
        Vector3 progress = new (progX, 0f, 0f);
        transform.position += progress;
    }

    public void SetVip()
    {
        //todo: Add VIP logic
    }

    public bool IsVip()
    {
        return false; //TEMP
    }

    // Todo - add collision logic
}
