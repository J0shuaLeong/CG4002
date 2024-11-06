using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainBombCollision : MonoBehaviour {

    [SerializeField] private GameEngine gameEngine;
    [SerializeField] private AREffects aREffects;
    [SerializeField] private OpponentDetection opponentDetection;


    private Transform opponentTransform;
    private bool isInRange = true;


    private const float RAIN_BOMB_RADIUS = 0.65f;


    private void Start() {
        opponentTransform = opponentDetection.GetOpponentTransform();

        aREffects.ShowOpponentRainEffect();
        gameEngine.PublishOpponentEnteredRainBomb();
    }

    private void Update() {
        Debug.Log(transform.position);

        opponentTransform = opponentDetection.GetOpponentTransform();

        if (opponentTransform == null && isInRange) {
            isInRange = false;

            aREffects.RemoveOpponentRainEffect();
            gameEngine.PublishOpponentExitedRainBomb();

            Debug.Log("Opponent not visible");
        } else if (opponentTransform != null) {
            Vector3 rainBombPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            float distance = Vector3.Distance(rainBombPosition, opponentTransform.position);

            if (distance <= RAIN_BOMB_RADIUS && !isInRange) {
                isInRange = true;

                gameEngine.OpponentRainBombCollision();
                gameEngine.PublishOpponentEnteredRainBomb();

                Debug.Log("Opponent entered rain bomb");
            } else if (distance > RAIN_BOMB_RADIUS && isInRange) {
                isInRange = false;
                
                aREffects.RemoveOpponentRainEffect();
                gameEngine.PublishOpponentExitedRainBomb();

                Debug.Log("Opponent exited rain bomb");
            }
        }
    }

    // for 2 player eval
    public bool CheckForRainBombCollision() {
        if (opponentTransform != null) {
            Vector3 rainBombPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            float distance = Vector3.Distance(rainBombPosition, opponentTransform.position);

            if (distance <= RAIN_BOMB_RADIUS) {
                return true;
            }
        }

        return false;
    }

}