using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public float offsetY = 0.1f;
    public float offsetX = 0.1f;
    GameObject images;
    GameObject inner;
    UnityEngine.UI.Image healthBarImage;
    Camera cam;
    bool isVisible;

    public void SetHealth(int currentHealth, int maxHealth)
    {
        if (!isVisible && currentHealth != maxHealth)
        {
            images.SetActive(true);
            isVisible = true;
        }
        
        var relativeHealth = (float)currentHealth / maxHealth;

        // Set the fill amount of the health bar image
        healthBarImage.fillAmount = relativeHealth;
    }

    void UpdateCamera()
    {
        cam = Camera.main;
    }

    void Start()
    {
        UpdateCamera();
        inner = transform.GetChild(0).gameObject;
        images = inner.transform.GetChild(0).GetChild(0).gameObject;
        healthBarImage = images.transform.GetChild(1).GetComponent<UnityEngine.UI.Image>();
        if (healthBarImage == null)
        {
            Debug.LogError("HealthBar: Image component not found!");
            return;
        }

        isVisible = false;
        images.SetActive(isVisible);

        SetHealth(1, 1);

        GameState.GetInstance().Subscribe(GameEvent.CAMERA_CHANGED, UpdateCamera);
    }

    void Update()
    {
        inner.transform.rotation = Quaternion.Euler(0, 0, 0);
        var healthBarPosition = transform.position;
        healthBarPosition.y += offsetY;
        healthBarPosition.x += offsetX;
        var healthBarScreenPosition = cam.WorldToScreenPoint(healthBarPosition);
        images.transform.position = healthBarScreenPosition;
    }
}
