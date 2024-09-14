using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
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
            Debug.Log("Boooooooooooooooooooooooooooooooooooooom!");
            Destroy(gameObject);
        }
    }
}
