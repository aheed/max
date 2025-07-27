using UnityEngine;
using UnityEngine.UIElements;

public class FriendlyLandingStrip : MonoBehaviour
{
    public AirStripInfo airStripInfo;
    UIDocument billboardDocument;
    UIDocument tarmacDocument;
    static readonly int initialUpdates = 3; // Number of initial updates to ensure UI is updated
    int updatesToWait = initialUpdates;

    void Start()
    {
        airStripInfo ??= AirStripRepository.GetRandomAirStrip();

        billboardDocument = transform.GetChild(2).GetComponent<UIDocument>();
        SetText(billboardDocument, airStripInfo.name.ToUpper());
        //SetText(billboardDocument, "TOTALLY NOT BEHIND ENEMY LINES AIRSTRIP");
        //SetText(billboardDocument, "CHECKPOINT CHARLIE’S REFUEL & AMMO");
        //SetText(billboardDocument, "REFUEL & RELOAD: ENEMY TERRITORY EDITION");

        tarmacDocument = transform.GetChild(1).GetComponent<UIDocument>();
        SetText(tarmacDocument, string.IsNullOrEmpty(airStripInfo.slogan) ? string.Empty : $"\"{airStripInfo.slogan}\"");
    }

    void SetText(UIDocument uiDocument, string slogan)
    {
        var sloganLabel = uiDocument.rootVisualElement.Q<Label>("Slogan");
        sloganLabel.text = slogan;
    }

    void Update()
    {
        if (updatesToWait > 0)
        {
            updatesToWait--;
            if (updatesToWait <= 0)
            {
                // Stop updating text for performance reasons
                billboardDocument.enabled = false;
                tarmacDocument.enabled = false;
            }
        }
    }
}
