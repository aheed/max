using System.Globalization;
using UnityEngine;

public class Vehicle3d : ManagedObject
{
    static readonly int points = 10;

    void OnTriggerEnter(Collider col)
    {
        GameObject bombGameObject = null;
        if (col.name.StartsWith("bullet", true, CultureInfo.InvariantCulture))
        {

        }
        else if (col.name.StartsWith("bomb", true, CultureInfo.InvariantCulture))
        {
            bombGameObject = col.gameObject;
        }
        else
        {
            return;
        }

        GameState.GetInstance().BombLanded(bombGameObject, gameObject);
        GameState.GetInstance().AddScore(points);
    }
}
