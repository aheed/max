using UnityEngine;

public class EnemyPlane3d : MonoBehaviour, IVip
{
    public Transform refObject;    
    public float maxDistance = 8f;
    public float maxDistanceBehind = 1f;
    public float moveIntervalSecMin = 0.1f;
    public float moveIntervalSecMax = 3f;
    public float crashDurationSec = 0.4f;
    public float oncomingPropOffsetX = -0.1f;
    public float oncomingPropOffsetY = -0.1f;
    public GameObject explosionPrefab;
    public int crashExplosions = 4;
    public float explosionDistanceMax = 0.4f;
    float lastAltitude;
    float moveCooldownSec;
    float crashCooldownSec;    
    int crashExplosionsLeft;
    float speed = 0.1f;
    int moveX = 0;
    int lastMoveX = 0;
    bool crashed =  false;
    VipBlinker vipBlinker;
    GameState gameState;
    GameObject model;
    PlaneController controller;

    GameObject GetModel()
    {
        if (model == null)
        {
            model = transform.GetChild(0).gameObject;
        }
        return model;
    }

    PlaneController GetController()
    {
        if (controller == null)
        {
            controller = model.GetComponent<PlaneController>(); //Todo: check if this is correct
            controller.planeModel = GetModel();
        }
        return controller;
    }

    public void SetSpeed(float speed)
    {
        this.speed = speed;
        if (speed < 0)
        {
            GetController().oncoming = true;
        }
    }

    public void SetVip()
    {
        // Todo: implement this

        //vipBlinker = new(gameObject.GetComponent<SpriteRenderer>());
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

        // Todo: set color to yellow

        gameState = FindAnyObjectByType<GameState>();
        Register();
    }

    void Register()
    {
        // Todo: implement this

        //gameState.EnemyPlaneStatusChanged(this, true);
    }

    void Deregister()
    {
        // Todo: implement this

        //gameState.EnemyPlaneStatusChanged(this, false);
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
            else
            {
                /*
                var fractionTimeLeft = crashCooldownSec / crashDurationSec;
                var rgb = 1f-fractionTimeLeft;
                spriteR.color = new Color(rgb, rgb, rgb, 0.5f + fractionTimeLeft/2);

                if (crashExplosionsLeft > fractionTimeLeft * crashExplosions)
                {
                    var newExplosion = Instantiate(explosionPrefab, gameObject.transform);
                    newExplosion.transform.localPosition = new Vector3(
                        UnityEngine.Random.Range(-explosionDistanceMax, explosionDistanceMax),
                        UnityEngine.Random.Range(-explosionDistanceMax, explosionDistanceMax),
                        0f);
                    --crashExplosionsLeft;
                }
                */
            }
            return;
        }

        if (speed > 0 && transform.position.z - refObject.transform.position.z > maxDistance)
        {
            Debug.Log($"Enemy plane too far in front ({transform.position.z} vs {refObject.transform.position.z})");
            Deactivate();
        }

        if (speed < 0 && refObject.transform.position.z - transform.position.z > maxDistanceBehind)
        {
            Debug.Log($"Enemy plane too far behind ({transform.position.z} vs {refObject.transform.position.z})");
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
            lastMoveX = moveX;
            GetController().SetAppearance(moveX, !crashed);
        }

        vipBlinker?.Update(Time.deltaTime);
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.name.StartsWith("bullet"))
        {
            // Todo: report the victory
        }
        else if (col.gameObject.name.StartsWith("player"))
        {
            // mid air collision
        }
        else 
        {
            return; //no collision
        }

        Debug.Log($"Enemy plane down!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! hit by {col.gameObject.name}");
        if(IsVip())
        {
            gameState.IncrementTargetsHit();
        }
        crashed = true;
        crashCooldownSec = crashDurationSec;
        crashExplosionsLeft = crashExplosions;

        GetController().SetAppearance(0, false);
        
        /*var collider = gameObject.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }*/
        gameState.ReportEvent(GameEvent.BIG_BANG);
    }    
}
