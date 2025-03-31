using UnityEngine;
using UnityEngine.UIElements;

public class TextureDocumentHorizontal : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var airStripInfo = AirStripRepository.GetRandomAirStrip();
        
        var uiDocument = GetComponent<UIDocument>();
        var nameLabel = uiDocument.rootVisualElement.Q<Label>("Name");
        var sloganLabel = uiDocument.rootVisualElement.Q<Label>("Slogan");

        nameLabel.text = airStripInfo.name;
        sloganLabel.text = $"\"{airStripInfo.slogan}\"";
    }
}
