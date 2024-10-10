using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// THIS VERSION IS ONLY FOR TESTING ON UNITY
// will need to modify to get game stats from game engine on ultra96 and update accordingly
public class GameUI : MonoBehaviour {

    [Header("Players")]
    [SerializeField] private Player player;
    [SerializeField] private Player opponent;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI playerScore;
    [SerializeField] private TextMeshProUGUI playerShieldCount;
    [SerializeField] private TextMeshProUGUI opponentScore;
    [SerializeField] private TextMeshProUGUI opponentShieldCount;
    [SerializeField] private TextMeshProUGUI ammoCount;
    [SerializeField] private TextMeshProUGUI rainBombCount;
    [SerializeField] private Image playerHPBar;
    [SerializeField] private Image playerShieldBar;
    [SerializeField] private Image opponentHPBar;
    [SerializeField] private Image opponentShieldBar;

    [Header("Game Engine")]
    public GameEngine gameEngine;


    // ---------- Score ----------
    public void UpdatePlayerScore() {
        playerScore.text = player.Score.ToString();
    }

    public void UpdateOpponentScore() {
        opponentScore.text = opponent.Score.ToString();
    }


    // ---------- HP Bar ----------
    public void UpdatePlayerHPBar() {
        playerHPBar.fillAmount = player.HP / 100f;
    }

    public void UpdateOpponentHPBar() {
        opponentHPBar.fillAmount = opponent.HP / 100f;
    }


    // ---------- Shield Bar ----------
    public void UpdatePlayerShieldBar() {
        playerShieldBar.fillAmount = player.ShieldHP / 30f;
    }

    public void UpdateOpponentShieldBar() {
        opponentShieldBar.fillAmount = opponent.ShieldHP / 30f;
    }


    // ---------- Shield Count ----------
    public void UpdatePlayerShieldCount() {
        playerShieldCount.text = player.ShieldCount.ToString() + "/3";
    }

    public void UpdateOpponentShieldCount() {
        opponentShieldCount.text = opponent.ShieldCount.ToString() + "/3";
    }


    // ------------ Player-Specific UI Elements ----------
    // [ Ammo, Rain Bomb Count ]
    public void UpdateAmmoCount() {
        ammoCount.text = player.Ammo.ToString() + "/∞";
    }

    public void UpdateRainBombCount() {
        rainBombCount.text = player.RainBombCount.ToString() + "/2";
    }

}