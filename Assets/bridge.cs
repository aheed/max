using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class bridge : MonoBehaviour, IPositionObservable, IVip, ITrigger2D
{
    public Target targetPrefab;
    public float targetOffset = 0.1f;
    Target target;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SetVip()
    {
        target = Instantiate(targetPrefab, gameObject.transform);
        var localPos = target.transform.localPosition;
        localPos.y += targetOffset;
        target.transform.localPosition = localPos;
    }

    public bool IsVip()
    {
        return target != null;
    }

    public void OnTriggerEnter2D(Collider2D col)
    {   
        //Debug.Log($"bridge collided with {col.name}");
        if (col.name.StartsWith("bomb", true, CultureInfo.InvariantCulture))
        {
            Debug.Log($"bridge bombed !!!!!!!!!!!!!!!!!!!!!!!!!!! collided with {col.name}");
            if (target != null)
            {
                Destroy(target.gameObject);
                target = null;
            }

            //todo: report road or bridge hit for scoring
        }
        
    }

    public Vector2 GetPosition() => transform.position;
    public float GetAltitude() => 0.1f;
    public float GetHeight() => 0.4f;
}
