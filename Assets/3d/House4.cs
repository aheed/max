using UnityEngine;
using UnityEngine.ProBuilder;

public class House4 : MonoBehaviour, IVip
{
    public GameObject targetPrefab;
    public GameObject explosionPrefab;
    public float sizeFactor = 0.3f;
    Vector3 size;

    GameObject target = null;

    Color[] randomColors = new Color[] {
        new Color(0.98f, 0.8f, 0.5f),
        new Color(0.9f, 0.1f, 0.9f),
        new Color(0.9f, 0.2f, 0.2f),
        new Color(0.9f, 0.9f, 0.9f),
    };

    public void SetAppearance(Vector3 newSize, bool randomColor)
    {
        var inner = transform.GetChild(0);
        var alive = inner.GetChild(0);
        var eastWall = alive.GetChild(0).gameObject;
        var westWall = alive.GetChild(1).gameObject;
        var southWall = alive.GetChild(2).gameObject;
        
        inner.transform.localScale = newSize * sizeFactor;

        var eastRenderer = eastWall.GetComponent<MeshRenderer>();
        var newEastMaterial = new Material(eastRenderer.sharedMaterial);
        newEastMaterial.mainTextureScale = new Vector2(newSize.y, newSize.z);
        eastRenderer.material = newEastMaterial;

        var westRenderer = westWall.GetComponent<MeshRenderer>();
        westRenderer.material = newEastMaterial;

        var southRenderer = southWall.GetComponent<MeshRenderer>();
        var newSouthMaterial = new Material(southRenderer.sharedMaterial);
        newSouthMaterial.mainTextureScale = new Vector2(newSize.x, newSize.y);
        southRenderer.material = newSouthMaterial;

        if (randomColor)
        {
            var colorIndex = Random.Range(0, randomColors.Length);
            var color = randomColors[colorIndex];
            newEastMaterial.color = color;
            newSouthMaterial.color = color;
        }

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
            GameState.GetInstance().TargetHit();
            Destroy(target);
            target = null;
        }

        GameState.GetInstance().BombLanded(col.gameObject, new GameObject());

        var inner = transform.GetChild(0);
        var alive = inner.GetChild(0);
        var bombed = inner.GetChild(1);
        alive.gameObject.SetActive(false);
        bombed.gameObject.SetActive(true);
        var collider = gameObject.GetComponentInChildren<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
    }
}
