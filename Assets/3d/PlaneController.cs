using UnityEngine;

public class PlaneController : MonoBehaviour
{
    public Material crashedWingMaterial;
    public Material normalFuselageMaterial;
    public Material normalWingMaterial;
    public GameObject planeModel;
    public float correctionRate = 20f;
    public float maxRotation = 30f;
    public float maxPitch = 20f;
    public float maxYaw = 35f;
    public float rollDurationSec = 0.89f;
    bool alive = false;
    float targetXRotation;
    float currentXRotation;
    float targetYRotation;
    float currentYRotation;
    float targetZRotation;
    float currentZRotation;
    float currentRollZRotation;
    float currentRollDurationSec = 100f;
    float rollRate;
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

    public void Roll(bool clockwise)
    {
        rollRate = (clockwise ? -1 : 1) * 360 / rollDurationSec;
        currentRollZRotation = 0;
        currentRollDurationSec = 0;
    }

    public void SetAppearance(float moveX, float moveY, bool alive)
    {
        //Debug.Log($"SetAppearance moveX={moveX} alive={alive} oncoming={oncoming}");

        if (alive)
        {
            if (moveX > 0)
            {
                targetZRotation = -maxRotation;
                targetYRotation = maxYaw;
            }
            else if (moveX < 0)
            {
                targetZRotation = maxRotation;
                targetYRotation = -maxYaw;
            }
            else
            {
                targetZRotation = 0;
                targetYRotation = 0;
            }

            if (moveY > 0)
            {
                targetXRotation = maxPitch;
            }
            else if (moveY < 0)
            {
                targetXRotation = -maxPitch;
            }
            else
            {
                targetXRotation = 0;
            }
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
        if (currentRollDurationSec < rollDurationSec)
        {
            currentRollDurationSec += Time.deltaTime;
            currentRollZRotation = rollRate * currentRollDurationSec;

            if (currentRollDurationSec > rollDurationSec)
            {
                currentRollZRotation = 0;
                if (currentZRotation > 180)
                {
                    currentZRotation -= 360;
                }
                else if (currentZRotation < -180)
                {
                    currentZRotation += 360;
                }
            }
        }

        currentZRotation += ((targetZRotation + currentRollZRotation) - currentZRotation) * correctionRate * Time.deltaTime;
        currentXRotation += (targetXRotation - currentXRotation) * correctionRate * Time.deltaTime;
        currentYRotation += (targetYRotation - currentYRotation) * correctionRate * Time.deltaTime;
        planeModel.transform.localRotation = Quaternion.Euler(currentXRotation, yRotation + currentYRotation, currentZRotation);
    }
}
