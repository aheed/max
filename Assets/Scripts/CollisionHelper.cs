using Unity.Burst.CompilerServices;
using UnityEngine;

public static class CollisionHelper
{
    public const string NoObject = "None";
    public const string UnknownObject = "Unknown";
    public static bool IsOverlappingAltitude(float altitude1, float height1, float altitude2, float height2)
    {
        return ((altitude1 + height1 / 2) >= (altitude2 - height2 / 2)) &&
               ((altitude1 - height1 / 2) <= (altitude2 + height2 / 2));
    }

    public static string GetObjectWithOverlappingAltitude(IPositionObservable obj1, GameObject obj2)
    {
        var tempMonoArray = obj2.GetComponents<MonoBehaviour>();

        foreach (var monoBehaviour in tempMonoArray)
        {
            if (monoBehaviour is IPositionObservable posobs)
            {
                //Debug.Log($"{obj2.name} {obj1.GetAltitude()} {obj1.GetHeight()} {posobs.GetAltitude()} {posobs.GetHeight()}");
                if (!IsOverlappingAltitude(obj1.GetAltitude(), obj1.GetHeight(), posobs.GetAltitude(), posobs.GetHeight()))
                {
                    return NoObject;    
                }
                return obj2.name;
            }
        }

        // No altitude info found in obj2. Assume it overlaps.
        return UnknownObject;
    }
}