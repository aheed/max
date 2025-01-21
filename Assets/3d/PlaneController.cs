using UnityEngine;

public class PlaneController : MonoBehaviour
{
    public GameObject planeModel;
    public bool oncoming;

    public void SetAppearance(float moveX, bool alive)
    {
        if (!alive)
        {
            transform.rotation = Quaternion.Euler(0, 0, 90);
        }
        else if (moveX > 0)
        {
            transform.rotation = Quaternion.Euler(0, 0, -30);
        }
        else if (moveX < 0)
        {
            transform.rotation = Quaternion.Euler(0, 0, 30);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }

        if (oncoming)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
