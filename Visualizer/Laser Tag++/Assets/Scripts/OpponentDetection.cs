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
                opponentTransformText.text = opponentTransform.ToString(); // DEBUG ON PHONE
                Debug.Log("Opponent is visible with transform: " + opponentTransform); // DEBUG
            }
        }

        foreach (var updatedImage in eventArgs.updated) {
            if (updatedImage.trackingState == TrackingState.Tracking) {
                opponentTransform = updatedImage.transform;
                opponentTransformText.text = opponentTransform.ToString(); // DEBUG ON PHONE
                Debug.Log("Opponent is visible with transform: " + opponentTransform); // DEBUG
            } else {
                opponentTransform = null;
                opponentTransformText.text = "cannot see me"; // DEBUG ON PHONE
                Debug.Log("Opponent moved out of view"); // DEBUG
            }
        }

        foreach (var removedImage in eventArgs.removed) {
            opponentTransform = null;
            opponentTransformText.text = "cannot see me"; // DEBUG ON PHONE
            Debug.Log("Opponent removed"); // DEBUG
        }
    }

    public Transform GetOpponentTransform() {
        // return opponentTransform;

        // FOR TESTING
        GameObject dummyOpponent = new GameObject("DummyOpponent");
        // dummyOpponent.transform.position = new Vector3((float)-0.15, (float)0.1, (float)0.5);
        dummyOpponent.transform.position = new Vector3((float)-0.93, (float)-0.28, (float)0.23);
        return dummyOpponent.transform;
    }
}