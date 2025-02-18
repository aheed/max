using UnityEngine;

public class Car3d : ManagedObject, IVip
{
    public Material normalMaterial;
    public float speedFactor = 1.0f;    
    GameState gameState;
    float speed;
    bool isVip = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameState = GameState.GetInstance();
        speed = speedFactor * gameState.maxSpeed;
        //Debug.Log($"Car3d speed is {speed}");

        if(!isVip)
        {
            SetBlinkableMaterial(normalMaterial);
        }
    }    

    // Update is called once per frame
    void Update()
    {
        var progX = speed * Time.deltaTime;
        Vector3 progress = new (progX, 0f, 0f);
        transform.position += progress;
    }

    MeshRenderer[] GetBlinkableRenderers()
    {
        // Assume a certain structure of the model
        return new MeshRenderer[] {
            transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>(),
        };
    }

    void SetBlinkableMaterial(Material material)
    {
        foreach (var renderer in GetBlinkableRenderers())
        {
            renderer.material = material;
        }
    }

    public void SetVip()
    {
        SetBlinkableMaterial(GameState.carBlinkMaterial);
        isVip = true;
    }

    public bool IsVip()
    {
        return isVip;
    }    

    // Todo - add collision logic
}
