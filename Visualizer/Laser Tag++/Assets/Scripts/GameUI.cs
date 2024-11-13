using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour {

    [Header("Players")]
    [SerializeField] private Player player;
    [SerializeField] private Player opponent;


    [Header("Canvas")]
    [SerializeField] private Transform canvasTransform;


    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI playerNumberText;
    [SerializeField] private TextMeshProUGUI playerScore;
    [SerializeField] private Image[] playerShields;
    [SerializeField] private TextMeshProUGUI opponentScore;
    [SerializeField] private Image[] opponentShields;
    [SerializeField] private Image[] bullets;
    [SerializeField] private Image[] rainBombs;
    [SerializeField] private TextMeshProUGUI playerHPText;
    [SerializeField] private Image playerHPBar;
    [SerializeField] private TextMeshProUGUI playerShieldHPText;
    [SerializeField] private Image playerShieldBar;
    [SerializeField] private TextMeshProUGUI opponentHPText;
    [SerializeField] private Image opponentHPBar;
    [SerializeField] private TextMeshProUGUI opponentShieldHPText;
    [SerializeField] private Image opponentShieldBar;
    [SerializeField] private GameObject killDeathPopup;
    [SerializeField] private TextMeshProUGUI killDeathPopupText;
    [SerializeField] private GameObject nullActionPopup;
    [SerializeField] private TextMeshProUGUI nullActionPopupText;




    [Header("Devices")]
    [SerializeField] private TextMeshProUGUI gunStatusText;
    [SerializeField] private TextMeshProUGUI legStatusText;
    [SerializeField] private TextMeshProUGUI gloveStatusText;


    [Header("Game Engine")]
    public GameEngine gameEngine;


    private int playerID;


    private void Start() {
        playerID = PlayerPrefs.GetInt("SelectedPlayerID", 1);

        playerNumberText.text = $"P{playerID}";
    }


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

        playerHPText.text = player.HP.ToString();
    }

    public void UpdateOpponentHPBar() {
        opponentHPBar.fillAmount = opponent.HP / 100f;

        opponentHPText.text = opponent.HP.ToString();
    }


    // ---------- Shield Bar ----------
    public void UpdatePlayerShieldBar() {
        playerShieldBar.fillAmount = player.ShieldHP / 30f;

        playerShieldHPText.text = player.ShieldHP.ToString();
    }

    public void UpdateOpponentShieldBar() {
        opponentShieldBar.fillAmount = opponent.ShieldHP / 30f;

        opponentShieldHPText.text = opponent.ShieldHP.ToString();
    }


    // ---------- Shield Count ----------
    public void UpdatePlayerShieldCount() {
        int currentShields = player.ShieldCount;

        for (int i = 0; i < playerShields.Length; i++) {
            if (i < currentShields) {
                playerShields[i].enabled = true;
            }
            else {
                playerShields[i].enabled = false;
            }
        }
    }

    public void UpdateOpponentShieldCount() {
        int currentShields = opponent.ShieldCount;

        for (int i = 0; i < opponentShields.Length; i++) {
            if (i < currentShields) {
                opponentShields[i].enabled = true;
            }
            else {
                opponentShields[i].enabled = false;
            }
        }
    }


    // ------------ Player-Specific UI Elements ----------
    // [ Ammo, Rain Bomb Count ]
    public void UpdateAmmoCount() {
        int currentAmmo = player.Ammo;

        for (int i = 0; i < bullets.Length; i++) {
            if (i < currentAmmo) {
                bullets[i].enabled = true;
            }
            else {
                bullets[i].enabled = false;
            }
        }
    }

    public void UpdateRainBombCount() {
        int currentRainBombs = player.RainBombCount;

        for (int i = 0; i < rainBombs.Length; i++) {
            if (i < currentRainBombs) {
                rainBombs[i].enabled = true;
            }
            else {
                rainBombs[i].enabled = false;
            }
        }
    }


    // ------------ Kill/Death Popup ----------
    public IEnumerator ShowKillPopup() {
        GameObject popup = Instantiate(killDeathPopup, canvasTransform);
        popup.SetActive(true);
        killDeathPopupText.text = "Nice work!";

        yield return new WaitForSeconds(2);

        Destroy(popup);
    }

    public IEnumerator ShowDeathPopup() {
        GameObject popup = Instantiate(killDeathPopup, canvasTransform);
        popup.SetActive(true);
        killDeathPopupText.text = "Do better!";

        yield return new WaitForSeconds(2);

        Destroy(popup);
    }


    // ------------ Null Action Popup ----------
    public IEnumerator ShowNullActionPopup() {
        GameObject popup = Instantiate(nullActionPopup, canvasTransform);
        popup.SetActive(true);
        nullActionPopupText.text = "Redo Action!";

        yield return new WaitForSeconds(2);

        Destroy(popup);
    }


    // ------------ Devices ----------
    public void UpdateGunStatus(bool isConnected) {
        if (isConnected) {
            gunStatusText.color = Color.green;
        } else {
            gunStatusText.color = Color.red;
        }
    }

    public void UpdateLegStatus(bool isConnected) {
        if (isConnected) {
            legStatusText.color = Color.green;
        } else {
            legStatusText.color = Color.red;
        }
    }

    public void UpdateGloveStatus(bool isConnected) {
        if (isConnected) {
            gloveStatusText.color = Color.green;
        } else {
            gloveStatusText.color = Color.red;
        }
    }

}