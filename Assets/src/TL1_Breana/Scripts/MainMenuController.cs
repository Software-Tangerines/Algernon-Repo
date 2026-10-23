using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public void OnPlayButtonPressed()
    {
        GameCoordinator.Instance.StartRun(0);
    }
}
