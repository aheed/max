using System.Globalization;
using UnityEngine;

public class Tank3d : ManagedObject
{
    private bool demolished = false;

    GameObject GetHealthyModel()
    {
        return transform.GetChild(0).gameObject;
    }

    GameObject GetDemolishedModel()
    {
        return transform.GetChild(1).gameObject;
    }

    void Demolish()
    {
        if (demolished)
        {
            return;
        }

        demolished = true;

        GetHealthyModel().SetActive(false);
        GetDemolishedModel().SetActive(true);

        var collider = gameObject.GetComponent<BoxCollider>();
        if (collider != null)
        {
            collider.enabled = false;
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.name.StartsWith("Bomb"))
        {
            Demolish();
            GameState.GetInstance().BombLanded(col.gameObject, gameObject);

            // Todo: report destroyed tank for scoring
        }
        else if (col.name.StartsWith("bullet", true, CultureInfo.InvariantCulture))
        {
            Demolish();
            var gameState = GameState.GetInstance();
            gameState.ReportEvent(GameEvent.SMALL_DETONATION);
            gameState.ReportEvent(GameEvent.SMALL_BANG);

            // Todo: report destroyed tank for scoring
        }
    }

    public override void Reactivate()
    {
        if (demolished)
        {
            demolished = false;
            GetHealthyModel().SetActive(true);
            GetDemolishedModel().SetActive(false);
            var collider = gameObject.GetComponent<BoxCollider>();
            if (collider != null)
            {
                collider.enabled = true;
            }
        }
    }
}
