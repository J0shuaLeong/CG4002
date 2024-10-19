using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Vuforia;

public class OpponentDetection : MonoBehaviour {

    private ObserverBehaviour mObserverBehaviour;
    private Transform opponentTransform;

    // Testing Fields
    [SerializeField] private TextMeshProUGUI opponentTransformText;

    private void Start() {
        opponentTransform = null;

        mObserverBehaviour = GetComponent<ObserverBehaviour>();

        if (mObserverBehaviour) {
            mObserverBehaviour.OnTargetStatusChanged += OnTargetStatusChanged;
        }
    }

    private void OnTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus targetStatus) {
        if (targetStatus.Status == Status.TRACKED || targetStatus.Status == Status.EXTENDED_TRACKED) {
            opponentTransform = behaviour.transform;

            opponentTransformText.text = opponentTransform.position.ToString();
        } else if (targetStatus.Status == Status.NO_POSE) {
            opponentTransform = null;

            opponentTransformText.text = "NULL";
        }
    }

    public Transform GetOpponentTransform() {
        return opponentTransform;
    }

    private void OnDestroy() {
        if (mObserverBehaviour) {
            mObserverBehaviour.OnTargetStatusChanged -= OnTargetStatusChanged;
        }
    }
}
