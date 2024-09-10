using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARObjectSpawner : MonoBehaviour {

    [SerializeField] private GameObject testPrefab;
    [SerializeField] private ARRaycastManager arRaycastManager;
    [SerializeField] private ARAnchorManager arAnchorManager;


    private void Start() {
        if (arRaycastManager == null)
        {
            arRaycastManager = GetComponent<ARRaycastManager>();
        }
        
        if (arAnchorManager == null)
        {
            arAnchorManager = GetComponent<ARAnchorManager>();
        }
    }


    public void SpawnObject()
    {
        // raycast from the center of the screen
        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        List<ARRaycastHit> hitResults = new List<ARRaycastHit>();

        if (arRaycastManager.Raycast(screenCenter, hitResults))
        {
            Pose hitPose = hitResults[0].pose;
            Instantiate(testPrefab, hitPose.position, hitPose.rotation);
            Debug.Log("prefab intantiated");
        }
    }

}