using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour, IPositionObservable
{
    public float verticalSpeed = 1.9f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var tmpPos = transform.localPosition;
        tmpPos.z -= verticalSpeed * Time.deltaTime;
        tmpPos.y = tmpPos.z;
        transform.localPosition = tmpPos;

        if (tmpPos.z <= 0)
        {
            //Debug.Log("Boooooooooooooooooooooooooooooooooooooom!");
            Destroy(gameObject);
        }
    }

    public Vector2 GetPosition() => transform.position;
    public float GetAltitude() => transform.localPosition.z;
    public float GetHeight() => 0.2f;
    public float GetMoveX() => 0;
}
