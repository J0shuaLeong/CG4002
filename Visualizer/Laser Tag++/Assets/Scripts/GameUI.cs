using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// THIS VERSION IS ONLY FOR TESTING ON UNITY
// will need to modify to get game stats from game engine on ultra96 and update accordingly
public class GameUI : MonoBehaviour {

    [Header("Players")]
    [SerializeField] private Player player1;
    [SerializeField] private Player player2;

    [Header("UI Elements")]
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

    [Header("Game Engine (TEST)")]
    public GameEngine gameEngine;


    public void UpdatePlayer1Score() {
        player1Score.text = player1.Score.ToString();
    }

    public void UpdatePlayer2Score() {
        player2Score.text = player2.Score.ToString();
    }

    public void UpdatePlayer1HPBar() {
        player1HPBar.fillAmount = player1.HP / 100f;
    }

    public void UpdatePlayer2HPBar() {
        player2HPBar.fillAmount = player2.HP / 100f;
    }

    public void UpdatePlayer1ShieldBar() {
        player1ShieldBar.fillAmount = player1.ShieldHP / 30f;
    }

    public void UpdatePlayer2ShieldBar() {
        player2ShieldBar.fillAmount = player2.ShieldHP / 30f;
    }

    public void UpdatePlayer1ShieldCount() {
        player1ShieldCount.text = player1.ShieldCount.ToString() + "/3";
    }

    public void UpdatePlayer2ShieldCount() {
        player2ShieldCount.text = player2.ShieldCount.ToString() + "/3";
    }

    public void UpdateAmmoCount() {
        ammoCount.text = player1.Ammo.ToString() + "/∞";
    }

    public void UpdateRainBombCount() {
        rainBombCount.text = player1.RainBombCount.ToString() + "/2";
    }

}