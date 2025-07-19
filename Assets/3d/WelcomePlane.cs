using System.Collections.Generic;
using UnityEngine;

public class WelcomePlane : MonoBehaviour
{
    public float maxOffsetX = 0.5f;
    public float speed = 0.5f;
    public float rollSpeed = 0.5f;
    public float offsetY = 0.2f;
    public int defaultFlights = 3;
    public float nonPlaneProbability = 0.1f;
    GameObject inner;
    int flights = 0;

    void Start()
    {
        inner = transform.GetChild(0).gameObject;
        Respawn();
    }

    void Respawn()
    {
        flights++;
        if (flights > defaultFlights)
        {
            var innerIndex = 0;
            if (Random.Range(0f, 1f) < nonPlaneProbability)
            {
                // Respawn with a non-plane object
                innerIndex = Random.Range(1, transform.childCount);
            }
            
            inner = transform.GetChild(innerIndex).gameObject;
        }
        
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(child.gameObject == inner);
        }

        var tmpPos = inner.transform.localPosition;
        tmpPos.y = flights % 2 == 0 ? -offsetY : offsetY;
        tmpPos.x = -maxOffsetX;
        inner.transform.localPosition = tmpPos;
    }

    void Update()
    {
        inner.transform.localPosition += new Vector3(
            Time.deltaTime * speed,
            0,
            0
        );

        inner.transform.Rotate(0,
            0,
            -rollSpeed * Time.deltaTime
        );

        if (inner.transform.localPosition.x > maxOffsetX)
        {
            Respawn();
        }
    }
}
