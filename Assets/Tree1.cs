using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree1 : ManagedObject3, IPositionObservable
{
    public Vector2 GetPosition() => transform.position;
    public float GetAltitude() => Altitudes.unsafeAltitude / 2;
    public float GetHeight() => Altitudes.unsafeAltitude;
}
