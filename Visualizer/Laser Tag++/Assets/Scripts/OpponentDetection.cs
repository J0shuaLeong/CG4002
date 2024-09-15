using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class OpponentDetection : MonoBehaviour {

    [SerializeField] private ARTrackedImageManager arTrackedImageManager;

    private bool isVisible;


    private void Start() {
        isVisible = false;
    }


    private void OnEnable() => arTrackedImageManager.trackedImagesChanged += OnChanged;
    private void OnDisable() => arTrackedImageManager.trackedImagesChanged -= OnChanged;

    private void OnChanged(ARTrackedImagesChangedEventArgs eventArgs) {
        isVisible = true;
        Debug.Log("isVisible: " + isVisible); // DEBUG

        foreach (var newImage in eventArgs.added) {
            Debug.Log("new image:" + newImage.transform); // DEBUG
        }

        foreach (var updatedImage in eventArgs.updated){
            Debug.Log("updated image:" + updatedImage.transform); // DEBUG
        }
    }
}