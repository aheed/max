using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class EnemyPlane : MonoBehaviour, IPlaneObservable, IVip
{
    public Transform refObject;    
    public float maxDistance = 8f;
    public float maxDistanceBehind = 1f;
    public float moveIntervalSecMin = 0.1f;
    public float moveIntervalSecMax = 3f;
    public float crashDurationSec = 0.4f;
    public float oncomingPropOffsetX = -0.1f;
    public float oncomingPropOffsetY = -0.1f;
    public Sprite leftSprite;
    public Sprite rightSprite;
    public Sprite straightSprite;
    public Sprite crashedSprite;
    public Sprite oncomingSprite;
    float lastAltitude;
    float moveCooldownSec;
    float crashCooldownSec;
    float speed = 0.1f;
    private SpriteRenderer spriteR;
    int moveX = 0;
    int lastMoveX = 0;
    bool crashed =  false;
    VipBlinker vipBlinker;
    GameState gameState;

    public void SetSpeed(float speed)
    {
        this.speed = speed;
        if (speed < 0)
        {
            spriteR = gameObject.GetComponent<SpriteRenderer>();
            spriteR.sprite = oncomingSprite;

            // assume prop is the only child
            var propTransform = transform.GetChild(0);
            propTransform.localPosition = new Vector3(oncomingPropOffsetX, oncomingPropOffsetY, 0f);
        }
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
        return 0.3f;
    }

    public float GetMoveX() => moveX;

    public bool IsAlive() => !crashed;

    public void SetVip()
    {
        vipBlinker = new(gameObject.GetComponent<SpriteRenderer>());
    }

    public bool IsVip()
    {
        return vipBlinker != null;
    }

    void SetMoveCooldown()
    {
        moveCooldownSec = UnityEngine.Random.Range(moveIntervalSecMin, moveIntervalSecMax);
    }

    // Start is called before the first frame update
    void Start()
    {
        SetMoveCooldown();
        spriteR = gameObject.GetComponent<SpriteRenderer>();
        spriteR.color = new Color(1f, 1f, 0.1f); // yellow
        gameState = FindObjectOfType<GameState>();
        Register();
    }

    void Register()
    {
        gameState.EnemyPlaneStatusChanged(this, true);
    }

    void Deregister()
    {
        gameState.EnemyPlaneStatusChanged(this, false);
    }

    void Deactivate()
    {
        Deregister();
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (crashed)
        {
            crashCooldownSec -= Time.deltaTime;
            if (crashCooldownSec <= 0f)
            {
                Deactivate();
            }
            return;
        }

        if (speed > 0 && transform.position.y - refObject.transform.position.y > maxDistance)
        {
            Debug.Log($"Enemy plane too far in front ({transform.position.y} vs {refObject.transform.position.y})");
            Deactivate();
        }

        if (speed < 0 && refObject.transform.position.y - transform.position.y > maxDistanceBehind)
        {
            Debug.Log($"Enemy plane too far behind ({transform.position.y} vs {refObject.transform.position.y})");
            Deactivate();
        }

        moveCooldownSec -= Time.deltaTime;
        if (moveCooldownSec <= 0)
        {
            moveX = speed < 0 ? 0 : UnityEngine.Random.Range(-1, 2);
            SetMoveCooldown();
        }

        var progX = (speed + moveX * GameState.horizontalSpeed) * Time.deltaTime;
        var progY = speed * Time.deltaTime;
        Vector3 progress = new (progX, progY, 0.0f);
        transform.position += progress;

        if (moveX != lastMoveX)
        {
            var newSprite = straightSprite;
            if (moveX < 0)
            {
                newSprite = leftSprite;
            }
            else if (moveX > 0)
            {
                newSprite = rightSprite;
            }
            spriteR.sprite = newSprite;
            lastMoveX = moveX;
        }

        if (GetAltitude() != lastAltitude)
        {
            lastAltitude = GetAltitude();
            spriteR.sortingOrder = (int)(lastAltitude * 100.0f);
        }

        vipBlinker?.Update(Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        var collObjName = CollisionHelper.GetObjectWithOverlappingAltitude(this, col.gameObject);

        if (collObjName.StartsWith("bullet"))
        {
            // Todo: report the victory
        }
        else if (collObjName.StartsWith("max"))
        {
            // mid air collision
        }
        else 
        {
            return; //no collision
        }

        Debug.Log($"Enemy plane down!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! hit by {collObjName}");
        crashed = true;
        crashCooldownSec = crashDurationSec;
        spriteR.color = Color.white;
        spriteR.sprite = crashedSprite;
        var collider = gameObject.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }
    }

}
