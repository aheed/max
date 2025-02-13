using UnityEngine;

public class BalloonManager : MonoBehaviour
{
    public float riseSpeed = 0.1f;
    public float deactivationDistance = 8f;

    void Rise(float deltaAltitude) {
        Vector3 localPosition = transform.localPosition;
        localPosition += new Vector3(0, deltaAltitude, deltaAltitude);
        transform.localPosition = localPosition;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Rise(riseSpeed * Time.deltaTime);
    }
}
