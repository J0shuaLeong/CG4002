using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerSelectionUI : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI popupText;


    private const string POPUP_TEXT = "Welcome, Astronaut Trainee!\n\n" +
                                      "Today, you'll embark on your first combat training mission—a critical simulation designed to prepare you for encounters with hostile alien forces.\n\n" +
                                      "Please choose your team and gear up for battle!";


    private void Start() {
        ShowPopupText(POPUP_TEXT);
    }

    
    public void ShowPopupText(string message) {
        StartCoroutine(TypeText(message));
    }

    private IEnumerator TypeText(string message) {
        popupText.text = "";

        foreach (char letter in message.ToCharArray()) {
            popupText.text += letter;
            yield return new WaitForSeconds(0.05f);
        }
    }

}