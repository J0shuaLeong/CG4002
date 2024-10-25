using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainBombCollision : MonoBehaviour {

    [SerializeField] private GameEngine gameEngine;
    [SerializeField] private AREffects aREffects;



    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Opponent")) {
            gameEngine.OpponentRainEffect();
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Opponent")) {
            aREffects.RemoveRainEffect();
        }
    }

}