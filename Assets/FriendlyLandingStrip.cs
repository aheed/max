using TMPro;
using UnityEngine;

public class FriendlyLandingStrip : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var airStripInfo = AirStripRepository.GetRandomAirStrip();

        var nameGameObject = transform.GetChild(1);
        var nameTextMesh = nameGameObject.GetComponent<TextMeshPro>();
        nameTextMesh.text = airStripInfo.name;

        var sloganGameObject = transform.GetChild(2);
        var sloganTextMesh = sloganGameObject.GetComponent<TextMeshPro>();
        sloganTextMesh.text = $"\"{airStripInfo.slogan}\"";
    }
}
