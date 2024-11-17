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

        opponentTransformText.text = "Enemy Not Found";
        opponentTransformText.color = Color.red;
    }

    private void Update() {
        if (mObserverBehaviour && mObserverBehaviour.TargetStatus.Status != Status.TRACKED && opponentTransform != null) {
            opponentTransform = null;
            opponentTransformText.text = "Enemy Not Found";
            opponentTransformText.color = Color.red;
        }
    }


    private void OnTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus targetStatus) {
        if (targetStatus.Status == Status.TRACKED || targetStatus.Status == Status.EXTENDED_TRACKED) {
            opponentTransform = behaviour.transform;

            opponentTransformText.text = "Enemy Detected";
            opponentTransformText.color = Color.green;
        } else {
            opponentTransform = null;

            opponentTransformText.text = "Enemy Not Found";
            opponentTransformText.color = Color.red;
        }
    }

    public Transform GetOpponentTransform() {
        // Vector3 opponentWorldPosition = opponentTransform.TransformPoint(opponentTransform.localPosition);
        return opponentTransform;
    }

    private void OnDestroy() {
        if (mObserverBehaviour) {
            mObserverBehaviour.OnTargetStatusChanged -= OnTargetStatusChanged;
        }
    }
}
