using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerSelection : MonoBehaviour {
    public void SelectPlayer1()
    {
        PlayerPrefs.SetInt("SelectedPlayerID", 1);
        LoadMainScene();
    }

    public void SelectPlayer2()
    {
        PlayerPrefs.SetInt("SelectedPlayerID", 2);
        LoadMainScene();
    }

    private void LoadMainScene()
    {
        SceneManager.LoadScene("Main");
    }
}
