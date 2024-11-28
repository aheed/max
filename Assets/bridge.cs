using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class bridge : MonoBehaviour, IPositionObservable, IVip, ITrigger2D
{
    public Target targetPrefab;
    public float targetOffset = 0.1f;
    Target target;

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
        if (col.name.StartsWith("bomb", true, CultureInfo.InvariantCulture))
        {
            if (target != null)
            {
                Destroy(target.gameObject);
                target = null;
                FindObjectOfType<GameState>().IncrementTargetsHit();
            }

            //todo: report road or bridge hit for scoring
        }
    }

    public Vector2 GetPosition() => transform.position;
    public float GetAltitude() => 0.1f;
    public float GetHeight() => 0.4f;
}
