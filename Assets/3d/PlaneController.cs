using UnityEngine;

public class PlaneController : MonoBehaviour
{
    public GameObject planeModel;
    public bool oncoming;

    public void SetAppearance(float moveX, bool alive)
    {
        //Debug.Log($"SetAppearance moveX={moveX} alive={alive} oncoming={oncoming}");
        var yRotation = oncoming ? 180 : 0;
        if (!alive)
        {
            //planeModel.transform.rotation = Quaternion.Euler(0, yRotation, 90);
        }
        else if (moveX > 0)
        {
            planeModel.transform.rotation = Quaternion.Euler(0, yRotation, -30);
        }
        else if (moveX < 0)
        {
            planeModel.transform.rotation = Quaternion.Euler(0, yRotation, 30);
        }
        else
        {
            planeModel.transform.rotation = Quaternion.Euler(0, yRotation, 0);
        }
    }
}
