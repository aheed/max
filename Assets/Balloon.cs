using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Balloon : MonoBehaviour, IPositionObservable
{
    public float startAltitudeQuotientMax = 0.3f;
    public float height = 0.3f;
    public GameObject popPrefab;
    private SpriteRenderer spriteR;
    private GameObject shadow = null;

    private float startParentAltitude;

    public void SetShadow(GameObject shadow) => this.shadow = shadow;

    float GetParentAltitude() => gameObject.transform.parent.localPosition.y;

    int GetSortingOrder() => (int)(GetAltitude() * 100.0f);

    // Start is called before the first frame update
    void Start()
    {
        var gameState = FindAnyObjectByType<GameState>();
        spriteR = gameObject.GetComponent<SpriteRenderer>();        
        
        var startAltitude = UnityEngine.Random.Range(
            gameState.minAltitude,
            gameState.maxAltitude * startAltitudeQuotientMax);
        
        startParentAltitude = GetParentAltitude();

        Vector3 localPosition = transform.localPosition;
        localPosition.z = 0;
        localPosition += new Vector3(0, startAltitude, startAltitude);
        transform.localPosition = localPosition;
        spriteR.sortingOrder = GetSortingOrder();
    }

    void Pop()
    {
        var pop = Instantiate(popPrefab, gameObject.transform.position, Quaternion.identity);
        pop.GetComponent<SpriteRenderer>().sortingOrder = GetSortingOrder();
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (shadow != null)
        {
            Destroy(shadow);
            shadow = null;
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.name.StartsWith("bullet") &&
            !col.name.StartsWith("max"))
        {
            return;
        }

        var collObjName = CollisionHelper.GetObjectWithOverlappingAltitude(this, col.gameObject);
        //var bullet = InterfaceHelper.GetInterface<BulletControl>(col.gameObject);

        if (collObjName.StartsWith("bullet") ||
            collObjName.StartsWith("max"))
        {
            //Debug.Log($"bullet height:{bullet.GetHeight()} alt:{bullet.GetAltitude()} collision with balloon height:{GetHeight()} alt: {GetAltitude()} hit");
            FindAnyObjectByType<GameState>().IncrementTargetsHit();
        }
        else 
        {
            //Debug.Log($"bullet height:{bullet.GetHeight()} alt:{bullet.GetAltitude()} collision with balloon height:{GetHeight()} alt: {GetAltitude()} miss");
            return; //no collision
        }        
        
        Pop();
    }

    public Vector2 GetPosition()
    {
        return transform.position;
    }

    public float GetAltitude()
    {
        return (height / 4) + // offset up to the actual balloon, ignore string
            transform.localPosition.z + // start altitude
            GetParentAltitude() - startParentAltitude; // altitude gained since start
    }

    public float GetHeight()
    {
        return height;
    }
}
