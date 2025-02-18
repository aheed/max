using UnityEngine;

public class Boat3d1 : ManagedObject
{
    void OnTriggerEnter(Collider col)
    {
        //Debug.Log($"3D Boat 1 Hit! Collided with {col.gameObject.name}");
    }
}
