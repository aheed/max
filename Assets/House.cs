using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class House : MonoBehaviour, IPositionObservable, IVip
{
    public FlipBook bombedPrefab;
    public Target targetPrefab;
    public float targetOffset;
    Target target;
    static readonly int points = 50;

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

    void OnTriggerEnter2D(Collider2D col)
    {
        //Debug.Log($"********************** House at {transform.position} collided with {col.name} at {col.transform.position}");
        var collObjName = CollisionHelper.GetObjectWithOverlappingAltitude(this, col.gameObject);
        if (!collObjName.StartsWith("bomb"))
        {
            return;
        }

        //Debug.Log($"House!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! hit by {col.name}");
        //Destroy(gameObject);
        gameObject.SetActive(false);
        var parent = gameObject.transform.parent;
        var bombed_house = Instantiate(bombedPrefab, transform.position, Quaternion.identity, parent);
        bombed_house.Activate();
        GameState.GetInstance().AddScore(points);
    }

    public Vector2 GetPosition() => transform.position;
    public float GetAltitude() => Altitudes.unsafeAltitude / 2;
    public float GetHeight() => Altitudes.unsafeAltitude;
}
