using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class OpponentDetection : MonoBehaviour {

    [SerializeField] private ARTrackedImageManager arTrackedImageManager;

    private bool isVisible;
    private Transform opponentTransform;


    private void Start() {
        isVisible = false;
    }


    private void OnEnable() => arTrackedImageManager.trackedImagesChanged += OnChanged;
    private void OnDisable() => arTrackedImageManager.trackedImagesChanged -= OnChanged;

    private void OnChanged(ARTrackedImagesChangedEventArgs eventArgs) {
        isVisible = true;
        Debug.Log("isVisible: " + isVisible); // DEBUG

        foreach (var newImage in eventArgs.added) {
            if (newImage.trackingState == TrackingState.Tracking) {
                isVisible = true;
                opponentTransform = newImage.transform;
                Debug.Log("Opponent is visible: " + isVisible + " with transform: " + opponentTransform); // DEBUG
            }
        }

        foreach (var updatedImage in eventArgs.updated) {
            if (updatedImage.trackingState == TrackingState.Tracking) {
                isVisible = true;
                opponentTransform = updatedImage.transform;
                Debug.Log("Opponent is visible: " + isVisible + " with transform: " + opponentTransform); // DEBUG
            } else {
                isVisible = false;
                opponentTransform = null;
                Debug.Log("Opponent moved out of view: " + isVisible); // DEBUG
            }
        }

        foreach (var removedImage in eventArgs.removed) {
            isVisible = false;
            opponentTransform = null;
            Debug.Log("Opponent removed: " + isVisible); // DEBUG
        }
    }
}