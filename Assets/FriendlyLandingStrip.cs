using UnityEngine;
using UnityEngine.UIElements;

public class FriendlyLandingStrip : MonoBehaviour
{
    void Start()
    {
        var airStripInfo = AirStripRepository.GetRandomAirStrip();

        var billboardDocument = transform.GetChild(2).GetComponent<UIDocument>();
        SetText(billboardDocument, airStripInfo.name.ToUpper());
        //SetText(billboardDocument, "TOTALLY NOT BEHIND ENEMY LINES AIRSTRIP");
        //SetText(billboardDocument, "CHECKPOINT CHARLIE’S REFUEL & AMMO");
        //SetText(billboardDocument, "REFUEL & RELOAD: ENEMY TERRITORY EDITION");

        var tarmacDocument = transform.GetChild(1).GetComponent<UIDocument>();
        SetText(tarmacDocument, $"\"{airStripInfo.slogan}\"");
    }

    void SetText(UIDocument uiDocument, string slogan)
    {
        var sloganLabel = uiDocument.rootVisualElement.Q<Label>("Slogan");
        sloganLabel.text = slogan;
    }
}
