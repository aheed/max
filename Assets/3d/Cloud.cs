using UnityEngine;

public class Cloud : MonoBehaviour
{
    public float maxOffsetX = 0.5f;
    public float maxOffsetY = 0.2f;
    public float speed = 1f;
    public float speedFactor = 0.1f;

    void Start()
    {
        var tmpPos = transform.localPosition;
        tmpPos.x = Random.Range(-maxOffsetX, maxOffsetX);
        tmpPos.y = Random.Range(-maxOffsetY, maxOffsetY);
        transform.localPosition = tmpPos;
    }

    void Respawn()
    {
        var tmpPos = transform.localPosition;
        tmpPos.x = -maxOffsetX;
        tmpPos.y = Random.Range(-maxOffsetY, maxOffsetY);
        transform.localPosition = tmpPos;
    }

    void Update()
    {
        transform.localPosition += new Vector3(
            Time.deltaTime * speed * speedFactor,
            0,
            0
        );

        if (transform.localPosition.x > maxOffsetX)
        {
            Respawn();
        }
    }
}
