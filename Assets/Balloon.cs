using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Balloon : MonoBehaviour, IPositionObservable
{
    public Sprite popSprite;    
    public float poppedLifeSpanSec = 0.6f;
    public float startAltitudeQuotientMax = 0.3f;
    public float height = 0.3f;
    float timeToLiveSec;
    private SpriteRenderer spriteR;
    private bool popped = false;
    private GameObject shadow = null;
    private Vector3 riseVelocity;
    private float startParentAltitude;

    public void SetShadow(GameObject shadow) => this.shadow = shadow;

    float GetParentAltitude() => gameObject.transform.parent.localPosition.y;

    void Rise(float deltaAltitude) {
        Vector3 localPosition = transform.localPosition;
        localPosition += new Vector3(0, deltaAltitude, deltaAltitude);
        transform.localPosition = localPosition;
        spriteR.sortingOrder = (int)(GetAltitude() * 100.0f);        
        return;

        //Vector3 shadowLocalPosition = shadow.transform.localPosition;
        //shadowLocalPosition += new Vector3(0, -deltaAltitude, -deltaAltitude);
        //shadow.transform.localPosition = shadowLocalPosition;
        //shadow.transform.localPosition = new Vector3(0, -localPosition.y, -localPosition.y);

        var altitude = GetAltitude();
        shadow.transform.localPosition = new Vector3(0, -altitude);
    }

    // Start is called before the first frame update
    void Start()
    {
        var gameState = FindObjectOfType<GameState>();
        spriteR = gameObject.GetComponent<SpriteRenderer>();        

        /*
        riseVelocity = new Vector3(0, riseSpeed, riseSpeed);
        */
        
        var startAltitude = UnityEngine.Random.Range(
            gameState.minAltitude,
            gameState.maxAltitude * startAltitudeQuotientMax);
        
        startParentAltitude = GetParentAltitude();
        Rise(startAltitude);
    }

    // Update is called once per frame
    void Update()
    {
        if(popped)
        {
            timeToLiveSec -= Time.deltaTime;
            if (timeToLiveSec < 0f)
            {
                Destroy(gameObject);
                return;
            }

            var newOpacity = timeToLiveSec / poppedLifeSpanSec;
            var newColor = new Color(1f, 1f, 1f, newOpacity);
            spriteR.color = newColor;
            return;
        }

        //Rise(riseSpeed * Time.deltaTime);
    }

    void Pop()
    {
        Debug.Log($"Balloon popped!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        var spriteR = gameObject.GetComponent<SpriteRenderer>();
        spriteR.sprite = popSprite;
        timeToLiveSec = poppedLifeSpanSec;
        Destroy(shadow);
        shadow = null;
        var collider = gameObject.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }
        popped = true;
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
        if (!col.name.StartsWith("bullet"))
        {
            return;
        }

        var collObjName = CollisionHelper.GetObjectWithOverlappingAltitude(this, col.gameObject);

        if (collObjName.StartsWith("bullet"))
        {
            FindObjectOfType<GameState>().IncrementTargetsHit();
        }
        else 
        {
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
        return transform.localPosition.z + startParentAltitude - GetParentAltitude();
    }

    public float GetHeight()
    {
        return height;
    }
}
