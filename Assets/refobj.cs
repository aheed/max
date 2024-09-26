using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class refobj : MonoBehaviour
{
    public static readonly float levelSpeed = 2.0f;
    public Vector2 levelVelocity = new(levelSpeed, levelSpeed);
    Vector2 startPos = new(0.0f, 0.0f);
    public float maxY = 20.0f;

    // Start is called before the first frame update
    void Start()
    {
        //transform.position = startPos;
        startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 delta = levelVelocity * Time.deltaTime;
        transform.position += delta;

        /*if (transform.position.y > maxY)
        {
            transform.position = startPos;
        }*/
    }
}
