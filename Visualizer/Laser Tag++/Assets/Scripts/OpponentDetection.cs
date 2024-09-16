using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class OpponentDetection : MonoBehaviour {

    [SerializeField] private ARTrackedImageManager arTrackedImageManager;

    private Transform opponentTransform;


    private void OnEnable() => arTrackedImageManager.trackedImagesChanged += OnChanged;
    private void OnDisable() => arTrackedImageManager.trackedImagesChanged -= OnChanged;

    private void OnChanged(ARTrackedImagesChangedEventArgs eventArgs) {
        foreach (var newImage in eventArgs.added) {
            if (newImage.trackingState == TrackingState.Tracking) {
                opponentTransform = newImage.transform;
                Debug.Log("Opponent is visible with transform: " + opponentTransform); // DEBUG
            }
        }

        foreach (var updatedImage in eventArgs.updated) {
            if (updatedImage.trackingState == TrackingState.Tracking) {
                opponentTransform = updatedImage.transform;
                Debug.Log("Opponent is visible with transform: " + opponentTransform); // DEBUG
            } else {
                opponentTransform = null;
                Debug.Log("Opponent moved out of view"); // DEBUG
            }
        }

        foreach (var removedImage in eventArgs.removed) {
            opponentTransform = null;
            Debug.Log("Opponent removed"); // DEBUG
        }
    }

    public Transform GetOpponentTransform() {
        // return opponentTransform;

        // FOR TESTING
        GameObject dummyOpponent = new GameObject("DummyOpponent");
        dummyOpponent.transform.position = new Vector3((float)-0.93, (float)-0.28, (float)0.23);
        return dummyOpponent.transform;
    }
}