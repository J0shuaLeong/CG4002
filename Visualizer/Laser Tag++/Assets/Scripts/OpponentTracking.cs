using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour {

    [SerializeField] private OpponentDetection opponentDetection;

    Transform opponentTransform;


    private void Start() {
        opponentTransform = opponentDetection.GetOpponentTransform();
    }

    private void Update() {
        opponentTransform = opponentDetection.GetOpponentTransform();
    }

}