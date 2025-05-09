using System;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogDocument : MonoBehaviour
{
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
        dialogTextField.style.visibility = Visibility.Visible;
    }

    public void HideDialog()
    {
        dialogTextField.style.visibility = Visibility.Hidden;
    }

    void Start()
    {
        var dialogDocument = GetComponent<UIDocument>();
        dialogTextField = dialogDocument.rootVisualElement.Q<TextField>("MsgToPlayer");
        okButton = dialogDocument.rootVisualElement.Q<Button>("OkButton");
        okButton.RegisterCallback<ClickEvent>(OnOkButtonClicked);

        dialogTextField.SetValueWithoutNotify("Welcome to the game! Press start to begin your adventure.");
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
