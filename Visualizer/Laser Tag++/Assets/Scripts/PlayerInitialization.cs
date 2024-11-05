using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerInitialization : MonoBehaviour {

    [SerializeField] private OpponentDetection opponentDetection;

    private Transform opponentTransform;

    private bool playerReady = false;
    private bool opponentReady = false;

    private int playerID;
    private int opponentID;


    private void Start() {
        playerID = PlayerPrefs.GetInt("SelectedPlayerID", 1); // default: player 1
        opponentID = playerID == 1 ? 2 : 1;

        opponentTransform = opponentDetection.GetOpponentTransform();
    }

    private void Update() {
        opponentTransform = opponentDetection.GetOpponentTransform();

        if (opponentTransform != null) {
            playerReady = true;
            // TODO: publish to mqtt player x ready
        } else {
            playerReady = false;
            // TODO: publish to mqtt player x not ready
        }

        // TODO: keep checking topic to see if opponent is ready

        if (playerReady && opponentReady) {
            SceneManager.LoadScene("Main");
        }
    }

}