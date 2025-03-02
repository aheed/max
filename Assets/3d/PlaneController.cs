using UnityEngine;

public class PlaneController : MonoBehaviour
{
    public Material normalWingMaterial;
    public Material crashedWingMaterial;
    public Material normalFuselageMaterial;    
    public GameObject planeModel;
    public bool oncoming;
    bool alive = true;

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
            var wings = planeModel.transform.GetChild(0).GetChild(0);
            for (var i = 0; i < wings.childCount; ++i)
            {
                var child = wings.GetChild(i);
                child.GetComponent<MeshRenderer>().material = currentWingMaterial;
            }

            var currentFuselageMaterial = alive ? normalFuselageMaterial : crashedWingMaterial; 
            var fuselage = planeModel.transform.GetChild(0).GetChild(1);
            for (var i = 0; i < fuselage.childCount; ++i)
            {
                var child = fuselage.GetChild(i);
                child.GetComponent<MeshRenderer>().material = currentFuselageMaterial;
            }            
        }
    }
}
