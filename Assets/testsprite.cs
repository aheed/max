using UnityEngine;

public class testsprite : MonoBehaviour
{
    Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogError("Main camera not found");
            return;
        }        
    }

    void Update() {
        //transform.LookAt(mainCam.transform);
        transform.rotation = Quaternion.Euler(0f, mainCam.transform.eulerAngles.y, 0f);
        //transform.rotation = Quaternion.Euler(0f, mainCam.transform.eulerAngles.y, mainCam.transform.eulerAngles.z);
    }
}
