using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private SessionMachine sessionMachine;
    public void OnPlayButtonPressed()
    {
        sessionMachine.TransitionTo(SessionState.PlayingState);
        SceneManager.LoadScene("MVP", LoadSceneMode.Single);
    }
}