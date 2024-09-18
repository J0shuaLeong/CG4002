using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// THIS CODE WILL BE REWRITTEN ON THE ULTRA96, THIS VERSION IS ONLY FOR DEBUGGING AND TESTING ON UNITY
public class GameEngine : MonoBehaviour {

    [Header("Players")]
    [SerializeField] private Player player1;
    [SerializeField] private Player player2;

    [Header("Game UI")]
    public GameUI gameUI;

    [Header("Visualization")]
    [SerializeField] private AREffects aREffects;
    [SerializeField] private OpponentDetection opponentDetection;

    [SerializeField] private GameObject rainBomb;
    [SerializeField] private GameObject basketball;
    [SerializeField] private GameObject soccerBall;
    [SerializeField] private GameObject volleyball;
    [SerializeField] private GameObject bowlingBall;


    private const float BASKETBALL_TIME = 0.7f;
    private const float SOCCER_BALL_TIME = 0.5f;
    private const float VOLLEYBALL_TIME = 1f;
    private const float BOWLING_BALL_TIME = 0.2f;
    private const float RAIN_BOMB_TIME = 1f;
    private const float RAIN_BOMB_DELAY = 1.5f;


    public void Player1Shoot() {
        if (player1.Ammo > 0) {
            Player2TakeDamage(5);
            player1.Ammo--;
            gameUI.UpdateAmmoCount();
            
            aREffects.SpawnOpponentBulletHitEffect();
        }
    }

    public void Player2Shoot() {
        if (player2.Ammo > 0) {
            Player1TakeDamage(5);
            player2.Ammo--;

            aREffects.SpawnPlayerHitEffect();
        }
    }

    public void Player1Basketball() {
        Player2TakeDamage(10);

        aREffects.Throw(basketball, BASKETBALL_TIME);
    }

    public void Player1Soccer() {
        Player2TakeDamage(10);

        aREffects.Throw(soccerBall, SOCCER_BALL_TIME);
    }

    public void Player1Volleyball() {
        Player2TakeDamage(10);

        aREffects.Throw(volleyball, VOLLEYBALL_TIME);
    }

    public void Player1Bowling() {
        Player2TakeDamage(10);

        aREffects.Throw(bowlingBall, BOWLING_BALL_TIME);
    }

    public void Player2SportsAction() {
        Player1TakeDamage(10);

        aREffects.SpawnPlayerHitEffect();
    }

    public void Player1ThrowRainBomb() {
        if (player1.RainBombCount > 0) {
            Player2TakeDamage(5);

            player1.RainBombCount--;
            gameUI.UpdateRainBombCount();
            
            aREffects.Throw(rainBomb, RAIN_BOMB_TIME);
            StartCoroutine(aREffects.SpawnRainEffect(RAIN_BOMB_DELAY));
        }
    }

    public void Player2ThrowRainBomb() {
        if (player2.RainBombCount > 0) {
            Player1TakeDamage(5);

            player2.RainBombCount--;
            aREffects.SpawnPlayerHitEffect();
        }
    }


/* BELOW ONWARDS TO MOVE TO PLAYER CLASS */
    public void Player1TakeDamage(int damage) {
        if (player1.ShieldHP > 0) {
            player1.ShieldHP -= damage;
            gameUI.UpdatePlayer1ShieldBar();
            if (player1.ShieldHP == 0) {
                aREffects.RemovePlayerShield();
            }
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

    public void Player2TakeDamage(int damage) {
        if (player2.ShieldHP > 0) {
            player2.ShieldHP -= damage;
            gameUI.UpdatePlayer2ShieldBar();
            if (player2.ShieldHP == 0) {
                aREffects.RemoveOpponentShield();
            }
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


    public void Player1ActivateShield() {
        if (player1.ShieldCount > 0 && player1.ShieldHP == 0) {
            player1.ShieldHP = 30;
            player1.ShieldCount--;
            gameUI.UpdatePlayer1ShieldBar();
            gameUI.UpdatePlayer1ShieldCount();

            aREffects.ShowPlayerShield();
        }
    }

    public void Player2ActivateShield() {
        if (player2.ShieldCount > 0 && player2.ShieldHP == 0) {
            player2.ShieldHP = 30;
            player2.ShieldCount--;
            gameUI.UpdatePlayer2ShieldBar();
            gameUI.UpdatePlayer2ShieldCount();

            aREffects.ShowOpponentShield();
        }
    }

}
