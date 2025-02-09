using UnityEngine;

public class TargetMaterialBlinker
{
    public static readonly float intervalSec = 0.7f;
    public static readonly float minIntensity = 0.5f;
    public static readonly float maxIntensity = 1.0f;
    private Material[] sharedMaterials;
    private float elapsedSec;

    public TargetMaterialBlinker (Material[] sharedMaterials)
    {
        this.sharedMaterials = sharedMaterials;
    }

    public void Update (float deltaTime)
    {
        elapsedSec += deltaTime;
        var radians = 2 * Mathf.PI * elapsedSec / intervalSec;
        var halfDiff = (maxIntensity - minIntensity) / 2;
        var intensity = minIntensity + halfDiff + Mathf.Sin(radians) * halfDiff;
        foreach (var sharedMaterial in sharedMaterials)
        {
            sharedMaterial.color = new Color(intensity, intensity, 1.0f);
        }
    }
}