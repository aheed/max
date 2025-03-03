using UnityEngine;

public class PlaneController : MonoBehaviour
{
    public Material crashedWingMaterial;
    public Material normalFuselageMaterial;
    public Material normalWingMaterial;
    public GameObject planeModel;
    public bool oncoming;
    bool alive = false;

    MeshRenderer[] GetBlinkableRenderers()
    {
        // Assume a certain structure of the model
        var wings = planeModel.transform.GetChild(0).GetChild(0);
        return wings.GetComponentsInChildren<MeshRenderer>();        
    }

    MeshRenderer[] GetFuselageRenderers()
    {
        // Assume a certain structure of the model
        var fuselage = planeModel.transform.GetChild(0).GetChild(1);
        return fuselage.GetComponentsInChildren<MeshRenderer>();        
    }

    public void SetAppearance(float moveX, bool alive)
    {
        //Debug.Log($"SetAppearance moveX={moveX} alive={alive} oncoming={oncoming}");
        var yRotation = oncoming ? 180 : 0;
        if (!alive)
        {
            //planeModel.transform.rotation = Quaternion.Euler(0, yRotation, 90);
        }
        else if (moveX > 0)
        {
            planeModel.transform.rotation = Quaternion.Euler(0, yRotation, -30);
        }
        else if (moveX < 0)
        {
            planeModel.transform.rotation = Quaternion.Euler(0, yRotation, 30);
        }
        else
        {
            planeModel.transform.rotation = Quaternion.Euler(0, yRotation, 0);
        }

        if (this.alive != alive)
        {
            this.alive = alive;
            var currentWingMaterial = alive ? normalWingMaterial : crashedWingMaterial;
            foreach (var renderer in GetBlinkableRenderers())
            {
                renderer.material = currentWingMaterial;
            }

            var currentFuselageMaterial = alive ? normalFuselageMaterial : crashedWingMaterial; 
            foreach (var renderer in GetFuselageRenderers())
            {
                renderer.material = currentFuselageMaterial;
            }
        }
    }
}
