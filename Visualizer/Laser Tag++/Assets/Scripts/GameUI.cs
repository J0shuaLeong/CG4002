using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// TODO: rethink 2 player logic (this version is only for testing UI on unity)
public class GameUI : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI player1Score;
    [SerializeField] private TextMeshProUGUI player1ShieldCount;
    [SerializeField] private TextMeshProUGUI player2Score;
    [SerializeField] private TextMeshProUGUI player2ShieldCount;
    [SerializeField] private TextMeshProUGUI ammoCount;
    [SerializeField] private TextMeshProUGUI rainBombCount;
    [SerializeField] private Image player1HPBar;
    [SerializeField] private Image player1ShieldBar;
    [SerializeField] private Image player2HPBar;
    [SerializeField] private Image player2ShieldBar;

    public GameEngine gameEngine;


    public void UpdatePlayer1Score() {
        player1Score.text = gameEngine.player1.Score.ToString();
    }

    public void UpdatePlayer2Score() {
        player2Score.text = gameEngine.player2.Score.ToString();
    }

    public void UpdatePlayer1HPBar() {
        player1HPBar.fillAmount = gameEngine.player1.HP / 100f;
    }

    public void UpdatePlayer2HPBar() {
        player2HPBar.fillAmount = gameEngine.player2.HP / 100f;
    }

    public void UpdatePlayer1ShieldBar() {
        player1ShieldBar.fillAmount = gameEngine.player1.ShieldHP / 30f;
    }

    public void UpdatePlayer2ShieldBar() {
        player2ShieldBar.fillAmount = gameEngine.player2.ShieldHP / 30f;
    }

    public void UpdatePlayer1ShieldCount() {
        player1ShieldCount.text = gameEngine.player1.ShieldCount.ToString() + "/3";
    }

    public void UpdatePlayer2ShieldCount() {
        player2ShieldCount.text = gameEngine.player2.ShieldCount.ToString() + "/3";
    }

    public void UpdateAmmoCount() {
        ammoCount.text = gameEngine.player1.Ammo.ToString() + "/∞";
    }

    public void UpdateRainBombCount() {
        rainBombCount.text = gameEngine.player1.RainBombCount.ToString() + "/2";
    }

}