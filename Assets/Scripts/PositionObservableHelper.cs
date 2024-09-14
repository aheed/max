using UnityEngine;

public static class PositionObservableHelper
{
    public static IPositionObservable GetPositionObservable(GameObject go)
    {
        var tempMonoArray = go.GetComponents<MonoBehaviour>();

        foreach (var monoBehaviour in tempMonoArray)
        {
            if (monoBehaviour is IPositionObservable posobs)
            {
                return posobs;
            }
        }
        return null;
    }
}