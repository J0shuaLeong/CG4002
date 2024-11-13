using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainBombCollision : MonoBehaviour {

    [SerializeField] private GameEngine gameEngine;
    [SerializeField] private OpponentDetection opponentDetection;

    [SerializeField] private Transform cam;
    [SerializeField] private GameObject opponentRainEffect;


    private GameObject currentOpponentRainEffect;

    private Transform opponentTransform;
    private bool isInRange = true;


    private const float RAIN_BOMB_RADIUS = 0.65f;


    private void Start() {
        opponentTransform = opponentDetection.GetOpponentTransform();

        ShowOpponentRainEffect();
        gameEngine.PublishOpponentEnteredRainBomb();
    }

    private void Update() {
        opponentTransform = opponentDetection.GetOpponentTransform();

        if (opponentTransform == null && isInRange) {
            isInRange = false;

            RemoveOpponentRainEffect();
            gameEngine.PublishOpponentExitedRainBomb();

            Debug.Log("Opponent not visible");
        } else if (opponentTransform != null) {
            Vector3 rainBombPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            float distance = Vector3.Distance(rainBombPosition, opponentTransform.position);

            if (distance <= RAIN_BOMB_RADIUS && !isInRange) {
                isInRange = true;

                gameEngine.OpponentRainBombCollision();
                gameEngine.PublishOpponentEnteredRainBomb();

                ShowOpponentRainEffect();

                Debug.Log("Opponent entered rain bomb");
            } else if (distance > RAIN_BOMB_RADIUS && isInRange) {
                isInRange = false;
                
                RemoveOpponentRainEffect();
                gameEngine.PublishOpponentExitedRainBomb();

                Debug.Log("Opponent exited rain bomb");
            }
        }
    }

    private void ShowOpponentRainEffect() {
        Transform fixedTransform = opponentTransform;

        currentOpponentRainEffect = Instantiate(opponentRainEffect, fixedTransform.position, cam.rotation);
        currentOpponentRainEffect.SetActive(true);

        currentOpponentRainEffect.transform.SetParent(fixedTransform);

        currentOpponentRainEffect.transform.localRotation = Quaternion.Euler(180f, 0f, 0f);
        currentOpponentRainEffect.transform.localPosition = new Vector3(0f, 0f, 1f);
        currentOpponentRainEffect.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
    }

    private void RemoveOpponentRainEffect() {
        if (currentOpponentRainEffect != null) {
            Destroy(currentOpponentRainEffect);
            currentOpponentRainEffect = null;
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