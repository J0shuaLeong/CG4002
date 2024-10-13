using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    public void SelectPlayer1()
    {
        PlayerPrefs.SetInt("PlayerID", 1);
        PlayerPrefs.Save();  // Ensure the PlayerPrefs are saved
        LoadGameScene();
    }

    public void SelectPlayer2()
    {
        PlayerPrefs.SetInt("PlayerID", 2);
        PlayerPrefs.Save();  // Ensure the PlayerPrefs are saved
        LoadGameScene();
    }

    private void LoadGameScene()
    {
        SceneManager.LoadScene("Main");
    }
}
