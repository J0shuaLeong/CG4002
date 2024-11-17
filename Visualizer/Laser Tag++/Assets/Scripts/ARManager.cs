using Google.XR.Cardboard;
using UnityEngine;

public class ARManager : MonoBehaviour {
    private Google.XR.Cardboard.XRLoader cardboardLoader;
    private void Start() {
        //AR Lifecycle Methods 
        cardboardLoader = ScriptableObject.CreateInstance<Google.XR.Cardboard.XRLoader>();
        InitializeGoogleCardboardXR();
    }

    public void InitializeGoogleCardboardXR() {
        Debug.Log("Initializing Google Cardboard XR");
        // Initialize the Google Cardboard XR if on Android / iOS 
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) {
            // set screen brightness to max 
            Screen.brightness = 1;

            Application.targetFrameRate = 120;

            cardboardLoader.Initialize();
            cardboardLoader.Start();
            Debug.Log("Google Cardboard XR initialized");
        }
        else {
            Debug.Log("Google Cardboard XR not initialized. Platform not supported.");
        }
    }

    public void DeinitializeGoogleCardboardXR() {
        Debug.Log("Deinitializing Google Cardboard XR");

        // Stop the Google Cardboard XR if on Android / iOS 
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) {
            // set screen brightness to default 
            Screen.brightness = 0.5f;

            Application.targetFrameRate = 30;

            cardboardLoader.Stop();
            cardboardLoader.Deinitialize();
            Debug.Log("Google Cardboard XR deinitialized");
        }
    }

    // Deinitialize Google Cardboard XR if the close button is pressed 
    public void Update() {
        if (Google.XR.Cardboard.Api.IsCloseButtonPressed) {
            DeinitializeGoogleCardboardXR();
        }
    }
}