using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerInitializationUI : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI popupText;


    private const string POPUP_TEXT = "Please stand in front of your opponent and ensure the alien target on their vest is visible on your screen. The game will start once both players are ready!";


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