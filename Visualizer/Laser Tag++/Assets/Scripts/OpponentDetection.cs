using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class OpponentDetection : MonoBehaviour {

    [SerializeField] ARTrackedImageManager arTrackedImageManager;

    private void OnEnable() => arTrackedImageManager.trackedImagesChanged += OnChanged;
    private void OnDisable() => arTrackedImageManager.trackedImagesChanged -= OnChanged;

    private void OnChanged(ARTrackedImagesChangedEventArgs eventArgs) {
        foreach (var newImage in eventArgs.added) {
            // DEBUG
            Debug.Log("new image:" + newImage.transform);
        }

        foreach (var updatedImage in eventArgs.updated){
            // DEBUG
            Debug.Log("updated image:" + updatedImage.transform);
        }
    }
}