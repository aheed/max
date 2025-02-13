using System;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectCollection
{
    public float zCoord;
    public IEnumerable<IManagedObject> managedObjects;
}

public class GameObjectCollection3
{
    public float zCoord;
    public IEnumerable<Action> managedObjects;
}