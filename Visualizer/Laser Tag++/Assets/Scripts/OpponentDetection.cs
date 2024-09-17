using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class OpponentDetection : MonoBehaviour {

    [SerializeField] private ARTrackedImageManager arTrackedImageManager;

    private Transform opponentTransform;


    /* TESTING FIELDS */
    [SerializeField] private TextMeshProUGUI opponentTransformText;


    private void OnEnable() => arTrackedImageManager.trackedImagesChanged += OnChanged;
    private void OnDisable() => arTrackedImageManager.trackedImagesChanged -= OnChanged;

    private void OnChanged(ARTrackedImagesChangedEventArgs eventArgs) {
        foreach (var newImage in eventArgs.added) {
            if (newImage.trackingState == TrackingState.Tracking) {
                opponentTransform = newImage.transform;
                opponentTransformText.text = opponentTransform.position.ToString(); // DEBUG ON PHONE
            }
        }

        foreach (var updatedImage in eventArgs.updated) {
            if (updatedImage.trackingState == TrackingState.Tracking) {
                opponentTransform = updatedImage.transform;
                opponentTransformText.text = opponentTransform.position.ToString(); // DEBUG ON PHONE
            } else {
                opponentTransform = null;
                opponentTransformText.text = "cannot see me"; // DEBUG ON PHONE
            }
        }

        foreach (var removedImage in eventArgs.removed) {
            opponentTransform = null;
            opponentTransformText.text = "cannot see me"; // DEBUG ON PHONE
        }
    }

    public Transform GetOpponentTransform() {
        return opponentTransform;

        // FOR TESTING
        // GameObject dummyOpponent = new GameObject("DummyOpponent");
        // dummyOpponent.transform.position = new Vector3(-0.7f, 1f, -0.5f);
        // return dummyOpponent.transform;
    }
}