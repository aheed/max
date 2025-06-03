using UnityEngine;

public class Dam : ManagedObject
{
    public GameObject[] GetMoveableObjects()
    {
        return new GameObject[] {
            transform.GetChild(0).gameObject, // wall
            transform.GetChild(1).gameObject, // net1
            transform.GetChild(2).gameObject, // net2
            transform.GetChild(3).gameObject, // target
            transform.GetChild(4).gameObject // bouncy water
        };
    }
}
