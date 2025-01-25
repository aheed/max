using UnityEngine;
using UnityEngine.ProBuilder;

public class House4 : MonoBehaviour
{
    public float sizeFactor = 0.3f;

    public void SetSize(Vector3 newSize)
    {
        //Debug.Log($"House4 Setting size to {newSize}");
        
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

        // Move the house up by half its height so it sits on the ground
        var position = transform.position;
        position.y += newSize.y * sizeFactor / 2;
        transform.position = position;
    }

    public void SetVip()
    {
        //Todo: Add VIP logic
    }

    void OnCollisionEnter(Collision col)
    {
        //Debug.Log($"House Hit!!!!!!!!!!!!!!!  collided with {col.gameObject.name}");

        /*
        if (col.gameObject.name.StartsWith("max") || 
            col.gameObject.name.StartsWith("flack_expl") ||
            col.gameObject.name.StartsWith("bomb") ||
            col.gameObject.name.StartsWith("balloon"))
        {
            return;
        }

        Destroy(gameObject);
        */
    }

}
