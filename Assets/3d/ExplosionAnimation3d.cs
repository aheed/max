using UnityEngine;

public class ExplosionAnimation3d : MonoBehaviour
{
    void OnParticleSystemStopped()
    {
        //Debug.Log("ExplosionAnimation3d.OnParticleSystemStopped");
        InterfaceHelper.GetInterface<FlackExplosion3d>(transform.parent.gameObject)?.AnimationStoppedCallback();
    }
}
