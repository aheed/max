using UnityEngine;
public interface IPlaneObservable : IPositionObservable
{
    public float GetMoveX();
    public bool IsAlive();
}

