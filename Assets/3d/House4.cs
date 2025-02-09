using UnityEngine;
using UnityEngine.ProBuilder;

public class House4 : MonoBehaviour, IVip
{
    public GameObject targetPrefab;
    public float sizeFactor = 0.3f;
    Vector3 size;

    GameObject target = null;

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

        size = newSize;
    }

    public void SetVip()
    {
        var targetPosition = transform.position;
        targetPosition.y += (size.y / 2) * sizeFactor;
        target = Instantiate(targetPrefab, targetPosition, Quaternion.Euler(20f, 0f, 0f), transform);
    }

    public bool IsVip()
    {
        return target != null;
    }

    void OnTriggerEnter(Collider col)
    {        
        //Debug.Log($"House Hit!!!!!!!!!!!!!!!  collided with {col.gameObject.name}");
        
        if (!col.gameObject.name.StartsWith("Bomb"))
        {
            return;
        }

        if (IsVip())
        {
            GameState.GetInstance().IncrementTargetsHit();
        }

        GameState.GetInstance().BombLanded(col.gameObject, gameObject);

        // Todo: spawn explosion/destroyed house
    }

}
