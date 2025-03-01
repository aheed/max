using UnityEngine;

public class Splash3d : MonoBehaviour
{
    public float lifeSpanSec = 1.2f;
    public float minScale = 0.1f;
    public float maxScale = 0.5f;
    float timeToLiveSec;
    Material material;

    void UpdateScale()
    {
        var scale = minScale + (maxScale - minScale) * (1f - timeToLiveSec / lifeSpanSec);
        transform.localScale = new Vector3(scale, 1f, scale);
        var opacity = timeToLiveSec / lifeSpanSec;
        material.SetFloat("_Alpha", opacity);
    }

    // Start is called before the first frame update
    void Start()
    {
        timeToLiveSec = lifeSpanSec;
        material = transform.GetChild(0).GetComponent<MeshRenderer>().material;
        UpdateScale();
    }

    // Update is called once per frame
    void Update()
    {
        timeToLiveSec -= Time.deltaTime;
        if (timeToLiveSec < 0f)
        {
            Destroy(gameObject);
        }

        UpdateScale();        
    }
}
