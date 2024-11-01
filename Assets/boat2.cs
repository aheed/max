using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class boat2 : MonoBehaviour, IPositionObservable, IVip
{
    public GameObject sunkBoatPrefab;
    public float speed = 0.8f;
    GameState gameState;
    Vector3 velocity;
    VipBlinker vipBlinker;

    // Start is called before the first frame update
    void Start()
    {
        gameState = FindObjectOfType<GameState>();    
        velocity = new Vector3(-speed, -speed, 0);
        var spriteR = gameObject.GetComponent<SpriteRenderer>();
        spriteR.color = new Color(0.5f, 0.4f, 0f); // brown
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
        var collider = gameObject.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        var parent = gameObject.transform.parent;
        Instantiate(sunkBoatPrefab, transform.position, Quaternion.identity, parent);
        gameObject.SetActive(false);

        // Todo: report destroyed boat for scoring
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.name.StartsWith("bomb"))
        {
            var bomb = col.gameObject.GetComponent<Bomb>();
            gameState.BombLanded(bomb, null);
            Sink();
            return;
        }

        var collObjName = CollisionHelper.GetObjectWithOverlappingAltitude(this, col.gameObject);
        if (collObjName.StartsWith("bullet"))
        {
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
