using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainBombCollision : MonoBehaviour {

    [SerializeField] private GameEngine gameEngine;
    [SerializeField] private AREffects aREffects;
    [SerializeField] private OpponentDetection opponentDetection;


    private Transform opponentTransform;
    private bool isInRange = true;


    private void Start() {
        opponentTransform = opponentDetection.GetOpponentTransform();

        aREffects.SpawnRainEffect();
    }

    private void Update() {
        Debug.Log(transform.position);

        opponentTransform = opponentDetection.GetOpponentTransform();

        if (opponentTransform != null) {
            float distance = Vector3.Distance(transform.position, opponentTransform.position);

            if (distance <= 1.0f && !isInRange) {
                isInRange = true;
                gameEngine.OpponentRainEffect();

                gameEngine.PublishOpponentEnteredRainBomb();

                Debug.Log("Opponent entered rain bomb");
            } else if (distance > 1.0f && isInRange) {
                isInRange = false;
                aREffects.RemoveRainEffect();

                gameEngine.PublishOpponentExitedRainBomb();

                Debug.Log("Opponent exited rain bomb");
            }
        }
    }

}