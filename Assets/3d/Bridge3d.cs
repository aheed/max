using UnityEngine;

public class Bridge3d : MonoBehaviour, IVip
{
    public GameObject targetPrefab;
    GameObject target = null;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void SetVip()
    {
        var targetPosition = transform.position;
        target = Instantiate(targetPrefab, targetPosition, Quaternion.Euler(20f, 0f, 0f), transform);
    }

    public bool IsVip()
    {
        return target != null;
    }

    void OnTriggerEnter(Collider col)
    {        
        //Debug.Log($"***** Bridge Trigger Hit!!!!!!!!!!!!!!!  collided with {col.gameObject.name}");
        
        if (!col.gameObject.name.StartsWith("Bomb"))
        {
            return;
        }

        if (IsVip())
        {
            GameState.GetInstance().IncrementTargetsHit();
            Destroy(target);
            target = null;
        }

        GameState.GetInstance().BombLanded(col.gameObject, new GameObject());
    }
}
