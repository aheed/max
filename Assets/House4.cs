using UnityEngine;
using UnityEngine.ProBuilder;

public class House4 : MonoBehaviour
{
    private readonly float sizeFactor = 0.5f;

    public void SetSize(Vector3 newSize)
    {
        Debug.Log($"House4 Setting size to {newSize}");
        
        var inner = transform.GetChild(0);
        var eastWall = inner.GetChild(0).gameObject;
        var southWall = inner.GetChild(1).gameObject;
        //var roof = inner.GetChild(2).gameObject;
        
        inner.transform.localScale = newSize * sizeFactor;

        var eastRenderer = eastWall.GetComponent<MeshRenderer>();
        var newEastMaterial = new Material(eastRenderer.sharedMaterial);
        newEastMaterial.mainTextureScale = new Vector2(newSize.y, newSize.z);
        eastRenderer.material = newEastMaterial;

        var southRenderer = southWall.GetComponent<MeshRenderer>();
        var newSouthMaterial = new Material(southRenderer.sharedMaterial);
        newSouthMaterial.mainTextureScale = new Vector2(newSize.x, newSize.y);
        southRenderer.material = newSouthMaterial;
    }

    public void SetVip()
    {
        //Todo: Add VIP logic
    }

}
