using UnityEngine;

public class WelcomePlane : MonoBehaviour
{
    public float maxOffsetX = 0.5f;
    public float speed = 0.5f;
    public float rollSpeed = 0.5f;
    public float offsetY = 0.2f;
    GameObject inner;

    void Start()
    {
        inner = transform.GetChild(0).gameObject;
        Respawn();
    }

    void Respawn()
    {
        var tmpPos = inner.transform.localPosition;
        tmpPos.y = tmpPos.y <= 0 ? offsetY : -offsetY;
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
