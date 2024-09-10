using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// TODO: separate UI + game engine stuff
// TODO: make 2 player version
// TODO: code game engine on ultra96 using python

public class GameState : MonoBehaviour {

    private int player1Score, player1HP, player1ShieldHP, player1ShieldCount, player1RainBombCount, player1AmmoCount;
    private int player2Score, player2HP, player2ShieldHP, player2ShieldCount;

    // private int player2RainBombCount, player2AmmoCount;


    [SerializeField] private TextMeshProUGUI Player1Score;
    [SerializeField] private TextMeshProUGUI Player2Score;
    [SerializeField] private Image Player1HPBar;
    [SerializeField] private Image Player1ShieldBar;
    [SerializeField] private TextMeshProUGUI Player1ShieldCount;
    [SerializeField] private Image Player2HPBar;
    [SerializeField] private Image Player2ShieldBar;
    [SerializeField] private TextMeshProUGUI Player2ShieldCount;
    [SerializeField] private TextMeshProUGUI AmmoCount;
    [SerializeField] private TextMeshProUGUI RainBombCount;


    private void Start() {
        /* initialization of all game stats */
        player1Score = 0;
        player2Score = 0;

        player1HP = 100;
        player2HP = 100;

        player1ShieldHP = 0;
        player2ShieldHP = 0;

        player1ShieldCount = 3;
        player2ShieldCount = 3;

        player1RainBombCount = 2;
        // player2RainBombCount = 2;

        player1AmmoCount = 6;
        // player2AmmoCount = 6;
    }


    // 2 player ver: public void Shoot(Player player)
    public void ShootPlayer2() {
        if (player1AmmoCount > 0) {
            /* decrease ammo */
            player1AmmoCount--;
            AmmoCount.text = player1AmmoCount.ToString() + "/∞";

            /* decrease player 2 shield or health */
            if (player2ShieldHP > 0) {
                player2ShieldHP -= 5;
                Player2ShieldBar.fillAmount = player2ShieldHP / 30f;
            } else {
                if (player2HP >= 0) {
                    player2HP -= 5;
                    Player2HPBar.fillAmount = player2HP / 100f;
                    if (player2HP == 0) {
                        UpdatePlayer1Score();
                    }
                }
            }
        }
    }

    public void ShootPlayer1() {
        /* decrease player 1 shield or health */
        if (player1ShieldHP > 0) {
            player1ShieldHP -= 5;
            Player1ShieldBar.fillAmount = player1ShieldHP / 30f;
        } else {
            if (player1HP >= 0) {
                player1HP -= 5;
                Player1HPBar.fillAmount = player1HP / 100f;
                if (player1HP == 0) {
                    UpdatePlayer2Score();
                }
            }
        }
    }


    // 2 player ver: public void UpdateScore(Player player)
    public void UpdatePlayer1Score() {
        /* player 2 died */
        player1Score ++;
        Player1Score.text = player1Score.ToString();

        player2HP = 100;
        Player2HPBar.fillAmount = 1f;
    }

    public void UpdatePlayer2Score() {
        /* player 1 died */
        player2Score ++;
        Player2Score.text = player2Score.ToString();

        player1HP = 100;
        Player1HPBar.fillAmount = 1f;
    }


    // 2 player ver: public void Reload(Player player)
    public void Reload() {
        if (player1AmmoCount == 0) {
            player1AmmoCount = 6;
            AmmoCount.text = "6/∞";
        }
    }


    // 2 player ver: public void Shield(Player player)
    public void ShieldPlayer1() {
        if (player1ShieldHP == 0 && player1ShieldCount > 0) {
            player1ShieldHP = 30;
            Player1ShieldBar.fillAmount = 1f;

            player1ShieldCount -= 1;
            Player1ShieldCount.text = player1ShieldCount.ToString() + "/3";
        }
    }

    public void ShieldPlayer2() {
        if (player2ShieldHP == 0 && player2ShieldCount > 0) {
            player2ShieldHP = 30;
            Player2ShieldBar.fillAmount = 1f;

            player2ShieldCount -= 1;
            Player2ShieldCount.text = player2ShieldCount.ToString() + "/3";
        }
    }


    // 2 player ver: public void SportsAction(Player player)
    public void SportsActionToPlayer1() {
        if (player1ShieldHP > 0) {
            player1ShieldHP -= 10;
            Player1ShieldBar.fillAmount = player1ShieldHP / 30f;
        } else {
            if (player1HP >= 0) {
                player1HP -= 10;
                Player1HPBar.fillAmount = player1HP / 100f;
                if (player1HP == 0) {
                    UpdatePlayer2Score();
                }
            }
        }
    }

    public void SportsActionToPlayer2() {
        if (player2ShieldHP > 0) {
            player2ShieldHP -= 10;
            Player2ShieldBar.fillAmount = player2ShieldHP / 30f;
        } else {
            if (player2HP >= 0) {
                player2HP -= 10;
                Player2HPBar.fillAmount = player2HP / 100f;
                if (player2HP == 0) {
                    UpdatePlayer1Score();
                }
            }
        }
    }


    // 2 player ver: public void RainBomb(Player player)
    // TODO: update rain bomb logic based on AR effects (detect when opponent steps into it and -5HP everytime)
    public void ThrowRainBomb() {
        if (player1RainBombCount > 0) {
            if (player2ShieldHP > 0) {
                player2ShieldHP -= 5;
                Player2ShieldBar.fillAmount = player2ShieldHP / 30f;
            } else {
                if (player2HP >= 0) {
                    player2HP -= 5;
                    Player2HPBar.fillAmount = player2HP / 100f;
                    if (player2HP == 0) {
                        UpdatePlayer1Score();
                    }
                }
            }

            player1RainBombCount--;
            RainBombCount.text = player1RainBombCount.ToString() + "/2";
        }
    }

    public void GetHitByRainBomb() {
        if (player1ShieldHP > 0) {
            player1ShieldHP -= 5;
            Player1ShieldBar.fillAmount = player1ShieldHP / 30f;
        } else {
            if (player1HP >= 0) {
                player1HP -= 5;
                Player1HPBar.fillAmount = player1HP / 100f;
                if (player1HP == 0) {
                    UpdatePlayer2Score();
                }
            }
        }
    }

}
