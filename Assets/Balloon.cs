using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Balloon : MonoBehaviour, IPositionObservable
{
    public Sprite popSprite;
    public float riseSpeed = 1.8f;
    public float poppedLifeSpanSec = 0.6f;
    public float startAltitudeQuotientMax = 0.3f;
    float timeToLiveSec;
    private SpriteRenderer spriteR;
    private bool popped = false;
    private GameObject shadow = null;
    private Vector3 riseVelocity;

    void Rise(float deltaAltitude) {
        Vector3 localPosition = transform.localPosition;
        localPosition += new Vector3(0, deltaAltitude, deltaAltitude);
        transform.localPosition = localPosition;
        shadow.transform.localPosition = new Vector3(0, -localPosition.y, -localPosition.y);
    }

    // Start is called before the first frame update
    void Start()
    {
        spriteR = gameObject.GetComponent<SpriteRenderer>();
        // assume shadow is the only child
        shadow = transform.GetChild(0).gameObject;
        riseVelocity = new Vector3(0, riseSpeed, riseSpeed);
        var gameState = FindObjectOfType<GameState>();
        var startAltitude = UnityEngine.Random.Range(
            gameState.minAltitude,
            gameState.minAltitude * startAltitudeQuotientMax);
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

        /*Vector3 localPosition = transform.localPosition;
        localPosition += riseVelocity * Time.deltaTime;
        transform.localPosition = localPosition;
        shadow.transform.localPosition = new Vector3(0, -localPosition.y, -localPosition.y);*/
        Rise(riseSpeed * Time.deltaTime);
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

    void OnTriggerEnter2D(Collider2D col)
    {
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
        return transform.position.z;
    }

    public float GetHeight()
    {
        return Altitudes.planeHeight;
    }
}
