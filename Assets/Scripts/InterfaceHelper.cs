using UnityEngine;

public static class InterfaceHelper
{
    public static T GetInterface<T>(GameObject go)
    {
        var tempMonoArray = go.GetComponents<MonoBehaviour>();

        foreach (var monoBehaviour in tempMonoArray)
        {
            if (monoBehaviour is T posobs)
            {
                return posobs;
            }
        }
        return default(T);
    }
}