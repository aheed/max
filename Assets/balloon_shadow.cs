using UnityEngine;

public class balloon_shadow : ManagedObject
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
