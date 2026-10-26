
using UnityEngine;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    public PlayerManager player;
    public TMP_Text healthText;

    private void Update()
    {
        if (player == null || healthText == null)
            return;

        healthText.text =
            "Health: " + player.data.health +
            " / " + player.data.maxHealth;
    }
}
