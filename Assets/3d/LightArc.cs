using UnityEngine;

public class LightArc : MonoBehaviour
{
    public float lifeSpanSec = 0.5f;
    FlipBook lightArcAnimation;

    void Start()
    {
        lightArcAnimation = gameObject.GetComponentInChildren<FlipBook>();
        lightArcAnimation.Activate();
        Destroy(gameObject, lifeSpanSec);    
    }
}
