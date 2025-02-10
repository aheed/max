using System.Collections.Generic;
using UnityEngine;

public class GameObjectCollection
{
    public float zCoord;
    public IEnumerable<IManagedObject> gameObjects;
}

// Todo: remove
public class GameObjectCollectionOld
{
    public float zCoord;
    public IEnumerable<GameObject> gameObjects;
}
