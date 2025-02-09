using UnityEngine;

public class FlackExplosion3d : MonoBehaviour
{
    public void AnimationStoppedCallback()
    {
        //Debug.Log("FlackExplosion3d.AnimationStoppedCallback");
        Destroy(gameObject);
    }
}
