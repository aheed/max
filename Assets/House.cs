using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class House : MonoBehaviour, IPositionObservable, IVip
{
    public FlipBook bombedPrefab;
    public Target targetPrefab;
    public float targetOffset;
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

    // Update is called once per frame
    void Update()
    {
        
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
    }

    public Vector2 GetPosition() => transform.position;
    public float GetAltitude() => 0.1f;
    public float GetHeight() => 0.4f;
}
