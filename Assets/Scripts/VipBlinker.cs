using UnityEngine;

public class VipBlinker
{
    public static readonly float intervalSec = 1f;
    public static readonly float minIntensity = 0.5f;
    public static readonly float maxIntensity = 1.0f;
    private SpriteRenderer spriteR;
    private float elapsedSec;

    public VipBlinker (SpriteRenderer spriteR)
    {
        this.spriteR = spriteR;
    }

    public void Update (float deltaTime)
    {
        elapsedSec += deltaTime;
        var radians = 2 * Mathf.PI * elapsedSec / intervalSec;
        var halfDiff = (maxIntensity - minIntensity) / 2;
        var intensity = minIntensity + halfDiff + Mathf.Sin(radians) * halfDiff;
        spriteR.color = new Color(intensity, intensity, 1.0f);
    }
}