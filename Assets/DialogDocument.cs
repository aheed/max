using UnityEngine;
using UnityEngine.UIElements;

public class DialogDocument : MonoBehaviour
{
    TextField dialogTextField;
    //string dialogText;

    void DisplayDialog()
    {
        // Display the dialog text on the screen
        //Debug.Log(dialogText);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dialogTextField = GetComponent<UIDocument>().rootVisualElement.Q<TextField>("MsgToPlayer");

        dialogTextField.SetValueWithoutNotify("Welcome to the game! Press start to begin your adventure.");
        // Initialize dialog text
        //dialogText = "Welcome to the game! Press start to begin your adventure.";
        
        // Display the dialog text on the screen
        //DisplayDialog();
    }

}
