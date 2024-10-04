using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class EnemyPlane : MonoBehaviour, IPlaneObservable
{
    public Transform refObject;
    public float speed = 0.1f;
    public float maxDistance = 50f;
    public float moveIntervalSecMin = 0.1f;
    public float moveIntervalSecMax = 3f;
    public float crashDurationSec = 0.4f;
    public Sprite leftSprite;
    public Sprite rightSprite;
    public Sprite straightSprite;
    public Sprite crashedSprite;
    float lastAltitude;
    float moveCooldownSec;
    float crashCooldownSec;
    private SpriteRenderer spriteR;
    int moveX = 0;
    int lastMoveX = 0;
    bool crashed =  false;

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

    void SetMoveCooldown()
    {
        moveCooldownSec = UnityEngine.Random.Range(moveIntervalSecMin, moveIntervalSecMax);
    }

    // Start is called before the first frame update
    void Start()
    {
        SetMoveCooldown();
        spriteR = gameObject.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (crashed)
        {
            crashCooldownSec -= Time.deltaTime;
            if (crashCooldownSec <= 0f)
            {
                //Destroy(gameObject);
                gameObject.SetActive(false);
            }
            return;
        }

        if (Math.Abs(transform.position.x - refObject.transform.position.x) > maxDistance)
        {
            Debug.Log($"Enemy plane too far away ({transform.position.x} vs {refObject.transform.position.x})");
            gameObject.SetActive(false);
        }

        moveCooldownSec -= Time.deltaTime;
        if (moveCooldownSec <= 0)
        {
            moveX = UnityEngine.Random.Range(-1, 2);
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
        spriteR.sprite = crashedSprite;
        var collider = gameObject.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }
    }

}
