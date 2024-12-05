using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalloonManager : MonoBehaviour
{
    public float riseSpeed = 0.1f;
    public float deactivationDistance = 8f;
    Transform refTransform;

    public void SetRefTransform(Transform refTransform) => this.refTransform = refTransform;

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

        // Destroy max one balloon
        if (transform.childCount > 0)
        {
            var balloon = transform.GetChild(0).gameObject;
            if (refTransform.position.y - deactivationDistance > balloon.transform.position.y)
            {
                Destroy(balloon);
            }
        }  
    }
}
