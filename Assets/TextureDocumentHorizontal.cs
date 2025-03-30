using UnityEngine;
using UnityEngine.UIElements;

public class TextureDocumentHorizontal : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var uiDocument = GetComponent<UIDocument>();
        var nameLabel = uiDocument.rootVisualElement.Q<Label>("Name");
        var sloganLabel = uiDocument.rootVisualElement.Q<Label>("Slogan");

        //nameLabel.text = "Random Name";
        nameLabel.text = "We Have No Idea How We Got Here Either Airbase";
        //sloganLabel.text = $"Random Slogan with a bit longer text {Random.Range(0, 100)}. This could potentially be a very long text that needs to be truncated.";
        sloganLabel.text = "\"Nothing to see here, just a completely normal, friendly airstrip in a completely illogical location.\"";
    }
}
