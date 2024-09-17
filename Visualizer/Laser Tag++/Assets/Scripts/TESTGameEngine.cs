using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// THIS CODE WILL BE REWRITTEN ON THE ULTRA96, THIS VERSION IS ONLY FOR DEBUGGING AND TESTING ON UNITY
public class GameEngine : MonoBehaviour {

    public Player player1;
    public Player player2;

    public GameUI gameUI;

    [SerializeField] private AREffects aREffects;
    [SerializeField] private OpponentDetection opponentDetection;

    [SerializeField] private GameObject rainBomb;
    [SerializeField] private GameObject ball;


    /* handles shooting action from player 1 to player 2 */
    public void Player1Shoot() {
        if (player1.Ammo > 0) {
            Player2TakeDamage(5);
            player1.Ammo--;
            gameUI.UpdateAmmoCount();
        }
    }

    /* handles shooting action from player 2 to player 1 */
    public void Player2Shoot() {
        if (player2.Ammo > 0) {
            Player1TakeDamage(5);
            player2.Ammo--;
        }
    }

    /* handles sports action from player 1 to player 2 */
    public void Player1SportsAction() {
        Player2TakeDamage(10);

        Transform opponentTransform = opponentDetection.GetOpponentTransform();
        // testing soccer
        aREffects.Throw(opponentTransform, 20, 5, ball);
    }

    /* handles sports action from player 2 to player 1 */
    public void Player2SportsAction() {
        Player1TakeDamage(10);
    }

    /* handles throw rain bomb action from player 1 to player 2 */
    public void Player1ThrowRainBomb() {
        if (player1.RainBombCount > 0) {
            Player2TakeDamage(5);
            // TODO: update this logic to -5HP everytime the opponent steps into the rain bomb

            player1.RainBombCount--;
            gameUI.UpdateRainBombCount();
            
            Transform opponentTransform = opponentDetection.GetOpponentTransform();
            aREffects.Throw(opponentTransform, 10, 10, rainBomb);
            StartCoroutine(aREffects.SpawnRainEffect(opponentTransform, 3f));
        }
    }

    /* handles throw rain bomb action from player 2 to player 1 */
    public void Player2ThrowRainBomb() {
        if (player2.RainBombCount > 0) {
            Player1TakeDamage(5);
            // TODO: update this logic to -5HP everytime the opponent steps into the rain bomb

            player2.RainBombCount--;
        }
    }


    /* reduces player 1's HP or shield HP when hit */
    public void Player1TakeDamage(int damage) {
        if (player1.ShieldHP > 0) {
            player1.ShieldHP -= damage;
            gameUI.UpdatePlayer1ShieldBar();
        } else {
            player1.HP -= damage;
            gameUI.UpdatePlayer1HPBar();
        }

        if (player1.HP == 0) {
            player2.Score++;
            gameUI.UpdatePlayer2Score();
            player1.HP = 100;
            gameUI.UpdatePlayer1HPBar();
        }
    }

    /* reduces player 2's HP or shield HP when hit */
    public void Player2TakeDamage(int damage) {
        if (player2.ShieldHP > 0) {
            player2.ShieldHP -= damage;
            gameUI.UpdatePlayer2ShieldBar();
        } else {
            player2.HP -= damage;
            gameUI.UpdatePlayer2HPBar();
        }

        if (player2.HP == 0) {
            player1.Score++;
            gameUI.UpdatePlayer1Score();
            player2.HP = 100;
            gameUI.UpdatePlayer2HPBar();
        }
    }


    /* activates a shield on player 1 if available */
    public void Player1ActivateShield() {
        if (player1.ShieldCount > 0 && player1.ShieldHP == 0) {
            player1.ShieldHP = 30;
            player1.ShieldCount--;
            gameUI.UpdatePlayer1ShieldBar();
            gameUI.UpdatePlayer1ShieldCount();

            Transform opponentTransform = opponentDetection.GetOpponentTransform();
            aREffects.ShowOpponentShield(opponentTransform);
        }
    }

    /* activates a shield on player 2 if available */
    public void Player2ActivateShield() {
        if (player2.ShieldCount > 0 && player2.ShieldHP == 0) {
            player2.ShieldHP = 30;
            player2.ShieldCount--;
            gameUI.UpdatePlayer2ShieldBar();
            gameUI.UpdatePlayer2ShieldCount();
        }
    }

}
