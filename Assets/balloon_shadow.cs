using UnityEngine;

public class balloon_shadow : ManagedObject4
{
    public override void Deactivate()
    {
        gameObject.SetActive(false);
    }

    public override void Reactivate()
    {
        gameObject.SetActive(true);
    }
}
