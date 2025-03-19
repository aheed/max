using System.Globalization;
using UnityEngine;

public class Boat3d2 : ManagedObject, IVip
{
    public GameObject sunkBoatPrefab;
    public Material normalMaterial;
    public float speed = 0.8f;
    //GameState gameState;
    Vector3 velocity;
    bool alive = true;
    bool isVip = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        velocity = new Vector3(0, 0, -speed);
        if(!isVip)
        {
            SetBlinkableMaterial(normalMaterial);
        }
    }

    public void SetVip()
    {
        SetBlinkableMaterial(GameState.boatBlinkMaterial);
        isVip = true;
    }

    public bool IsVip() => isVip;

    MeshRenderer GetBlinkableRenderer() =>
        // Assume a certain structure of the model
        transform.GetChild(0).GetComponent<MeshRenderer>();

    void SetBlinkableMaterial(Material material) => GetBlinkableRenderer().material = material;

    // Update is called once per frame
    void Update()
    {
        if (!alive)
        {
            return;
        }

        Vector3 position = transform.position;
        position += velocity * Time.deltaTime;
        transform.position = position;        
    }

    void Sink()
    {
        Instantiate(sunkBoatPrefab, transform.position, transform.rotation);
        alive = false;
    }

    void OnTriggerEnter(Collider col)
    {
        //Debug.Log($"3D Boat 2 Hit!!!!!! Collided with {col.gameObject.name}");
        if (col.gameObject.name.StartsWith("riversection"))
        {
            return;
        }

        if (col.gameObject.name.StartsWith("Bomb"))
        {
            if (IsVip())
            {
                GameState.GetInstance().TargetHit();
            }

            GameState.GetInstance().BombLanded(col.gameObject, gameObject);
            Sink();
        }

        if (col.name.StartsWith("bullet", true, CultureInfo.InvariantCulture))
        {
            GameState.GetInstance().ReportEvent(GameEvent.MEDIUM_BANG);
            Sink();
            Release();
            return;
        }
    }

    public override void Deactivate()
    {
        alive = false;
        gameObject.SetActive(false);
    }

    public override void Reactivate()
    {
        alive = true;
        gameObject.SetActive(true);
    }
}
