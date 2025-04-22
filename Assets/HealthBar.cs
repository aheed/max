using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public float offsetY = 1f;
    GameObject images;
    GameObject parent;
    UnityEngine.UI.Image healthBarImage;
    Camera cam;
    bool isVisible;

    public void SetHealth(int currentHealth, int maxHealth)
    {
        /*if (!isVisible && currentHealth != maxHealth)
        {
            images.SetActive(true);
            isVisible = true;
        }*/
        
        var relativeHealth = (float)currentHealth / maxHealth;

        // Set the fill amount of the health bar image
        healthBarImage.fillAmount = relativeHealth;
    }

    void Start()
    {
        cam = Camera.main;
        parent = transform.parent.gameObject;

        images = transform.GetChild(0).gameObject;
        healthBarImage = images.transform.GetChild(1).GetComponent<UnityEngine.UI.Image>();
        if (healthBarImage == null)
        {
            Debug.LogError("HealthBar: Image component not found!");
            return;
        }

        isVisible = false;
        //images.SetActive(isVisible);
        images.SetActive(true); //TEMP

        SetHealth(1, 1);
    }

    void Update()
    {
        var healthBarPosition = parent.transform.position;
        healthBarPosition.y += offsetY;
        var healthBarScreenPosition = cam.WorldToScreenPoint(healthBarPosition);
        images.transform.position = healthBarScreenPosition;

        //var healthBarScreenPosition = cam.WorldToScreenPoint(transform.position);
    }
}
