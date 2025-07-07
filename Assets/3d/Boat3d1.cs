using System.Globalization;
using UnityEngine;

public class Boat3d1 : ManagedObject
{
    static readonly int maxHealth = 3;
    public GameObject sunkBoatPrefab;
    int health = maxHealth;
    static readonly int points = 50;

    void Sink()
    {
        Instantiate(sunkBoatPrefab, transform.position, transform.rotation);
        GameState.GetInstance().AddScore(points);
    }

    void OnTriggerEnter(Collider col)
    {
        //Debug.Log($"3D Boat 1 Hit! Collided with {col.gameObject.name}");

        if (col.name.StartsWith("Bomb"))
        {
            GameState.GetInstance().BombLanded(col.gameObject, gameObject);
            Sink();
            return;
        }

        if (col.name.StartsWith("bullet", true, CultureInfo.InvariantCulture))
        {
            --health;
            if (health <= 0)
            {
                GameState.GetInstance().ReportEvent(GameEvent.MEDIUM_BANG);
                Sink();
                Release();
            }
            return;
        }
    }

    public override void Deactivate()
    {
        gameObject.SetActive(false);
    }

    public override void Reactivate()
    {
        gameObject.SetActive(true);
    }
}
