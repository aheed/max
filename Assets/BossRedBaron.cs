using UnityEngine;

public class BossRedBaron : MonoBehaviour
{
    public float moveDelayMaxSec = 4.5f;
    public float moveDelayMinSec = 1.5f;
    public float offsetMaxX = 2.5f;

    float moveCooldown = 0.0f;
    Vector3 targetLocalPosition;
    Vector3 startLocalPosition;

    void ResetMoveCooldown()
    {
        //destinationOffset = GetRandomOffset();
        targetLocalPosition = startLocalPosition + new Vector3(
            Random.Range(-offsetMaxX, offsetMaxX),
            0f, // TEMP
            0f  // TEMP
        );
        moveCooldown = Random.Range(moveDelayMinSec, moveDelayMaxSec);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startLocalPosition = transform.localPosition;
        ResetMoveCooldown();
    }

    // Update is called once per frame
    void Update()
    {
        moveCooldown -= Time.deltaTime;
        if (moveCooldown <= 0.0f)
        {
            ResetMoveCooldown();
            
        }

        transform.localPosition = Vector3.MoveTowards(
                transform.localPosition,
                targetLocalPosition,
                Time.deltaTime * 2.0f
            );
    }
}
