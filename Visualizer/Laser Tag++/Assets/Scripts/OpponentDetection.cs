using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

// this might not work, maybe use image tracking instead?
public class OpponentDetection : MonoBehaviour {

    [SerializeField] private ARHumanBodyManager humanBodyManager;

    private ARHumanBody opponent;
    public bool isOpponentVisible;
    // TODO: some variable to determine the coordinates of the opponent


    private void Start() {
        opponent = null;
        isOpponentVisible = false;
    }

    private void OnEnable() {
        if (humanBodyManager != null) {
            humanBodyManager.humanBodiesChanged += OnHumanBodiesChanged;
        }
    }

    private void OnDisable() {
        if (humanBodyManager != null) {
            humanBodyManager.humanBodiesChanged -= OnHumanBodiesChanged;
        }
    }


    private void OnHumanBodiesChanged(ARHumanBodiesChangedEventArgs eventArgs) {
        List<ARHumanBody> humanBodies = eventArgs.added;
        humanBodies.AddRange(eventArgs.updated);

        opponent = GetClosestHumanBody(humanBodies);
        
        if (opponent != null) {
            isOpponentVisible = true;
        }
    }

    private ARHumanBody GetClosestHumanBody(List<ARHumanBody> humanBodies) {
        if (humanBodies == null || humanBodies.Count == 0) {
            return null;
        }

        ARHumanBody closestHumanBody = null;
        float closestDistance = float.MaxValue;

        foreach (var humanBody in humanBodies) {
            float distance = Vector3.Distance(Camera.main.transform.position, humanBody.pose.position);
            if (distance < closestDistance) {
                closestDistance = distance;
                closestHumanBody = humanBody;
            }
        }

        return closestHumanBody;
    }

}