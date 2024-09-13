using UnityEngine;
public interface IPositionObservable
{
    public Vector2 GetPosition();

    public float GetAltitude();

    public float GetHeight();

    public float GetMoveX();
}
