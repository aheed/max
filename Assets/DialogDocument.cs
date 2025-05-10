using System;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogDocument : MonoBehaviour
{
    VisualElement dialogUIElem;
    TextField dialogTextField;
    Button okButton;

    Action onOkButtonClicked;

    public void SetOkButtonCallback(Action callback)
    {
        onOkButtonClicked = callback;
    }

    public void SetDialogText(string dialogText)
    {
        dialogTextField.SetValueWithoutNotify(dialogText);
    }

    public void ShowDialog()
    {
        dialogUIElem.style.visibility = Visibility.Visible;
        Debug.Log("Dialog shown");
    }

    public void HideDialog()
    {
        dialogUIElem.style.visibility = Visibility.Hidden;
        Debug.Log("Dialog hidden");
    }

    public void ShowOkButton()
    {
        okButton.style.visibility = Visibility.Visible;
    }
    public void HideOkButton()
    {
        okButton.style.visibility = Visibility.Hidden;
    }

    void Start()
    {
        var dialogDocument = GetComponent<UIDocument>();
        dialogUIElem = dialogDocument.rootVisualElement.Q<VisualElement>("DialogUI");
        dialogTextField = dialogDocument.rootVisualElement.Q<TextField>("MsgToPlayer");
        okButton = dialogDocument.rootVisualElement.Q<Button>("OkButton");
        okButton.RegisterCallback<ClickEvent>(OnOkButtonClicked);

        dialogTextField.SetValueWithoutNotify("");
        // Initialize dialog text
        //dialogText = "Welcome to the game! Press start to begin your adventure.";
        
        // Display the dialog text on the screen
        //DisplayDialog();
    }

    void OnOkButtonClicked(ClickEvent evt)
    {
        onOkButtonClicked();
    }

}
