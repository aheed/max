using UnityEngine;

public class Target3d : MonoBehaviour
{
    MeshRenderer GetBlinkableRenderer() =>
        // Assume a certain structure of the model
        transform.GetChild(0).GetComponent<MeshRenderer>();

    void SetBlinkableMaterial(Material material) => GetBlinkableRenderer().material = material;

    void Start()
    {
        SetBlinkableMaterial(GameState.targetBlinkMaterial);
    }

}
