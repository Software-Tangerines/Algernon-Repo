using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void OnPlayButtonPressed()
    {
        GameCoordinator.Instance.StartRun(0);
        SceneManager.LoadScene("MVP", LoadSceneMode.Single);
    }
}