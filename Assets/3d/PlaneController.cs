using UnityEngine;

public class PlaneController : MonoBehaviour
{
    public Material crashedWingMaterial;
    public Material normalFuselageMaterial;
    public Material normalWingMaterial;
    public GameObject planeModel;
    public float correctionRate = 20f;
    public float maxRotation = 30f;
    bool alive = false;
    float targetZRotation;
    float currentZRotation;
    float yRotation = 0;

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

    public void SetOncoming(bool oncoming)
    {
        yRotation = oncoming ? 180 : 0;
    }

    public void Tilt()
    {
        currentZRotation += Random.Range(-maxRotation, maxRotation);
    }

    public void SetAppearance(float moveX, bool alive)
    {
        //Debug.Log($"SetAppearance moveX={moveX} alive={alive} oncoming={oncoming}");
        
        if (!alive)
        {
            //planeModel.transform.rotation = Quaternion.Euler(0, yRotation, 90);
        }
        else if (moveX > 0)
        {
            targetZRotation = -maxRotation;
        }
        else if (moveX < 0)
        {
            targetZRotation = maxRotation;
        }
        else
        {
            targetZRotation = 0;
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

    void Update()
    {
        currentZRotation += (targetZRotation - currentZRotation) * correctionRate * Time.deltaTime;
        planeModel.transform.rotation = Quaternion.Euler(0, yRotation, currentZRotation);
    }
}
