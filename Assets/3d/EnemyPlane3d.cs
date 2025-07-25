using System.Globalization;
using UnityEngine;

public class EnemyPlane3d : MonoBehaviour, IVip
{
    public Material normalMaterial;
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
    bool crashed = false;
    bool lastCrashed = false;
    GameObject model;
    PlaneController controller;
    bool isVip = false;
    IEnemyPlaneNavigator navigator;
    bool isDestructible = true;
    static readonly int points = 100;

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
            controller = InterfaceHelper.GetInterface<PlaneController>(transform.GetChild(1).gameObject);
            controller.planeModel = GetModel();
        }
        return controller;
    }

    public void SetDestructible(bool destructible)
    {
        isDestructible = destructible;
    }

    public void SetNavigator(IEnemyPlaneNavigator navigator)
    {
        this.navigator = navigator;
    }

    public void SetSpeed(float speed)
    {
        this.speed = speed;
        if (speed < 0)
        {
            GetController().SetOncoming(true);
        }
    }

    void SetBlinkableMaterial(Material material)
    {
        GetController().normalWingMaterial = material;
    }

    public void SetVip()
    {
        SetBlinkableMaterial(GameState.planeBlinkMaterial);
        isVip = true;
    }

    public bool IsVip()
    {
        return isVip;
    }

    void SetMoveCooldown()
    {
        moveCooldownSec = UnityEngine.Random.Range(moveIntervalSecMin, moveIntervalSecMax);
    }

    void SetAppearance(int moveX)
    {
        if(crashed != lastCrashed)
        {
            lastCrashed = crashed;
            transform.GetChild(2).gameObject.SetActive(crashed);
        }
        GetController().SetAppearance(moveX, 0, !crashed);
    }

    // Start is called before the first frame update
    void Start()
    {
        SetMoveCooldown();

        if(!isVip)
        {
            SetBlinkableMaterial(normalMaterial);
        }

        Register();
        SetAppearance(0);
        navigator?.Start();
    }

    void Register()
    {
        GameState.GetInstance().AddEnemyPlane(gameObject, transform.localPosition.y);
    }

    void Deregister()
    {
        GameState.GetInstance().RemoveEnemyPlane(gameObject);
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
            //Debug.Log($"Enemy plane too far in front ({transform.position.z} vs {refObject.transform.position.z})");
            Deactivate();
        }
        else if (speed < 0 && refObject.transform.position.z - transform.position.z > maxDistanceBehind)
        {
            //Debug.Log($"Enemy plane too far behind ({transform.position.z} vs {refObject.transform.position.z})");
            Deactivate();
        }
        else if (speed == 0f)
        {
            return;
        }

        moveCooldownSec -= Time.deltaTime;
        if (moveCooldownSec <= 0)
        {
            SetMoveX(speed < 0 ? 0 : UnityEngine.Random.Range(-1, 2));
            SetMoveCooldown();
        }

        var progX = moveX * GameState.horizontalSpeed * Time.deltaTime;
        var progZ = speed * Time.deltaTime;
        Vector3 progress = new (progX, 0f, progZ);
        transform.position += progress;        

        navigator?.Update();
    }

    public void SetMoveX(int moveX)
    {
        this.moveX = moveX;
        if (moveX != lastMoveX)
        {
            lastMoveX = moveX;
            SetAppearance(moveX);
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (!isDestructible)
        {
            return;
        }

        if (col.name.StartsWith("bullet", true, CultureInfo.InvariantCulture))
        {
        }
        else if (col.name.StartsWith("player", true, CultureInfo.InvariantCulture))
        {
            // mid air collision
        }
        else if (col.name.StartsWith("bomb", true, CultureInfo.InvariantCulture))
        {
            Destroy(col.gameObject);
        }
        else
        {
            return; //no collision
        }

        Debug.Log($"Enemy plane down!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! hit by {col.gameObject.name}");
        var pointsScored = points;
        if (IsVip())
        {
            GameState.GetInstance().TargetHit();
            pointsScored *= 2;
        }
        crashed = true;
        Deregister();
        crashCooldownSec = crashDurationSec;
        crashExplosionsLeft = crashExplosions;

        SetAppearance(0);

        var collider = gameObject.GetComponent<BoxCollider>();
        if (collider != null)
        {
            collider.enabled = false;
        }
        GameState.GetInstance().ReportEvent(GameEvent.BIG_BANG);
        GameState.GetInstance().AddScore(pointsScored);
    }    
}
