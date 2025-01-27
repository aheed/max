using UnityEngine;

public class Camera3d : MonoBehaviour
{
    public GameObject viewTarget;

    void SetViewDirection()
    {
        transform.LookAt(viewTarget.transform.position);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetViewDirection();
    }

    // Update is called once per frame
    void Update()
    {
        SetViewDirection(); //TEMP!!!!!!!
    }
}
