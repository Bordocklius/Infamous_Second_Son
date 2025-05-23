using UnityEngine;

public class StartScreenScript : MonoBehaviour
{
    public void OnStartButtonClick()
    {
        // Load sandbox scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("Sandbox");
    }
}
