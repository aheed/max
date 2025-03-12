using UnityEngine;

public class Boat3d1 : ManagedObject
{
    public GameObject sunkBoatPrefab;

    void OnTriggerEnter(Collider col)
    {
        //Debug.Log($"3D Boat 1 Hit! Collided with {col.gameObject.name}");

        if (col.gameObject.name.StartsWith("Bomb"))
        {
            GameState.GetInstance().BombLanded(col.gameObject, gameObject);
            GameState.GetInstance().ReportEvent(GameEvent.SMALL_DETONATION);
            GameState.GetInstance().ReportEvent(GameEvent.MEDIUM_BANG);
            Instantiate(sunkBoatPrefab, transform.position, transform.rotation);
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
