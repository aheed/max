using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class boat2 : ManagedObject4, IPositionObservable, IVip
{
    public GameObject sunkBoatPrefab;
    public float speed = 0.8f;
    GameState gameState;
    Vector3 velocity;
    VipBlinker vipBlinker;

    // Start is called before the first frame update
    void Start()
    {
        gameState = GameState.GetInstance();    
        velocity = new Vector3(-speed, -speed, 0);
        
    }

    public override void Deactivate()
    {
        gameObject.GetComponent<Collider2D>().enabled = false;
        gameObject.SetActive(false);
        vipBlinker = null;
    }

    public override void Reactivate()
    {
        var spriteR = gameObject.GetComponent<SpriteRenderer>();
        spriteR.color = new Color(0.5f, 0.4f, 0f); // brown
        gameObject.SetActive(true);
        gameObject.GetComponent<Collider2D>().enabled = true;
    }

    public void SetVip()
    {
        vipBlinker = new(gameObject.GetComponent<SpriteRenderer>());
    }

    public bool IsVip()
    {
        return vipBlinker != null;
    }

    void Sink()
    {
        var parent = gameObject.transform.parent;
        Instantiate(sunkBoatPrefab, transform.position, Quaternion.identity, parent);
        Release();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.name.StartsWith("bomb"))
        {
            var bomb = col.gameObject.GetComponent<Bomb>();            
            gameState.BombLanded(bomb, null);
            gameState.ReportEvent(GameEvent.SMALL_DETONATION);
            gameState.ReportEvent(GameEvent.MEDIUM_BANG);
            if (IsVip())
            {
                gameState.IncrementTargetsHit();
            }
            Sink();
            return;
        }

        if ((col.name.StartsWith("bridge") && !col.name.StartsWith("bridge_mid")) ||
            col.name.StartsWith("boat") ||
            col.name.StartsWith("road_end"))
        {
            gameState.ReportEvent(GameEvent.SMALL_DETONATION);
            gameState.ReportEvent(GameEvent.SMALL_BANG);
            Sink();
            return;
        }

        var collObjName = CollisionHelper.GetObjectWithOverlappingAltitude(this, col.gameObject);
        if (collObjName.StartsWith("bullet"))
        {
            gameState.ReportEvent(GameEvent.MEDIUM_BANG);
            if (IsVip())
            {
                gameState.IncrementTargetsHit();
            }
            Sink();
        }
    }

    public Vector2 GetPosition() => transform.position;
    public float GetAltitude() => Altitudes.unsafeAltitude / 2;
    public float GetHeight() => Altitudes.unsafeAltitude;

    // Update is called once per frame
    void Update()
    {
        Vector3 position = transform.position;
        position += velocity * Time.deltaTime;
        transform.position = position;
        vipBlinker?.Update(Time.deltaTime);
    }
}
