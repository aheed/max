using UnityEngine;

public class Bomb3d : MonoBehaviour
{
    public float verticalSpeed = 1.9f;
    public float maxCollisionAltitude = 0.2f;


    // Update is called once per frame
    void Update()
    {
        var tmpPos = transform.localPosition;
        var deltaVertical = -verticalSpeed * Time.deltaTime;
        tmpPos.y += deltaVertical;
        transform.localPosition = tmpPos;

        if (tmpPos.y <= 0)
        {
            // Todo: handle bomb impact
            Destroy(gameObject);
        }
    }
}
