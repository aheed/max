using UnityEngine;

public static class PositionObservableHelper
{
    public static T GetPositionObservable<T>(GameObject go)
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