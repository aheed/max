using UnityEngine;
using UnityEngine.ProBuilder;

public class House3d : MonoBehaviour
{
    private readonly float sizeFactor = 0.5f;
    //private float xsize = 1.0f;

    /*public float Xsize
    {
        get { return xsize; }
        set { 
            Debug.Log($"Setting Xsize to {value}");
            xsize = value;
            Resize();
        }
    }*/

    public void SetSize(Vector3 newSize)
    {
        Debug.Log($"Setting size to {newSize}");
        
        var pbMesh = GetComponent<ProBuilderMesh>();
        if (pbMesh == null)
        {
            Debug.LogError("ProBuilderMesh component not found!");
            return;
        }
        
        //Vector3 newSize = new Vector3(xsize, pbMesh.transform.localScale.y, pbMesh.transform.localScale.z);
        pbMesh.transform.localScale = newSize * sizeFactor;

        var renderer = GetComponent<MeshRenderer>();
        var newMaterial = new Material(renderer.sharedMaterial);
        newMaterial.mainTextureScale = new Vector2(newSize.x, newSize.y) * 2;
        renderer.material = newMaterial;

        // Refresh the mesh to apply changes
        pbMesh.ToMesh();
        pbMesh.Refresh();
    }

    public void SetVip()
    {
        //Todo: Add VIP logic
    }

    /*void Resize()
    {
        Debug.Log("Resizing house");

        // Get the ProBuilder mesh component
        var pbMesh = GetComponent<ProBuilderMesh>();
        if (pbMesh == null)
        {
            Debug.LogError("ProBuilderMesh component not found!");
            return;
        }

        // Modify the ProBuilder shape properties
        Vector3 newSize = new Vector3(xsize, pbMesh.transform.localScale.y, pbMesh.transform.localScale.z);
        pbMesh.transform.localScale = newSize;

        // Refresh the mesh to apply changes
        pbMesh.ToMesh();
        pbMesh.Refresh();

    }*/

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Resize();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /*public void SetXsize(float newXsize)
    {
        Xsize = newXsize;
    }*/


}
