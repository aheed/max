using UnityEngine;

public class Bridge3d : MonoBehaviour, IVip
{
    public GameObject targetPrefab;
    GameObject target = null;
    static readonly int points = 50;


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

        var pointsScored = points;
        if (IsVip())
        {
            GameState.GetInstance().TargetHit();
            Destroy(target);
            target = null;
            pointsScored *= 2;
        }

        var tmpGameObject = new GameObject();
        var impactPosition = col.gameObject.transform.position;
        impactPosition.y = GameState.GetInstance().craterAltitude;
        tmpGameObject.transform.position = impactPosition;
        GameState.GetInstance().BombLanded(col.gameObject, tmpGameObject);
        GameState.GetInstance().AddScore(pointsScored);
    }
}
