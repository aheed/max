using UnityEngine;

public class Hangar3d : ManagedObject
{
    private bool alive = true;
    static readonly int points = 50;

    void SetAlive(bool isAlive)
    {
        alive = isAlive;
        transform.GetChild(0).gameObject.SetActive(isAlive);
        transform.GetChild(1).gameObject.SetActive(!isAlive);
    }

    void OnTriggerEnter(Collider col)
    {
        Debug.Log($"Hangar Hit!!!!!!!!!!!!!!!  collided with {col.gameObject.name}");

        if (!col.gameObject.name.StartsWith("Bomb"))
        {
            return;
        }

        GameState.GetInstance().BombLanded(col.gameObject, new GameObject());
        SetAlive(false);
        GameState.GetInstance().AddScore(points);
    }

    public override void Reactivate()
    {
        if (!alive)
        {
            SetAlive(true);
        }
    }
}
