using UnityEngine;

public class BossMissile : MonoBehaviour
{
    public GameObject targetObject;
    public float speed = 1.0f;
    public float zDistanceMax = 1.5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Temp
        if(targetObject == null)
        {
            Debug.LogWarning("Target object is not assigned!");
            Destroy(gameObject);
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, targetObject.transform.position, speed * Time.deltaTime);
        if ((targetObject.transform.position.z - transform.position.z) > zDistanceMax)
        {
            Destroy(gameObject);
            Debug.Log("Missile out of range!");
        }
    }
}
