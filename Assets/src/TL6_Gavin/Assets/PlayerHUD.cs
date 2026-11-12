
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class PlayerHUD : MonoBehaviour, IGameplayVisibility
{
    public PlayerManager player;
    public TMP_Text healthText;
    public TMP_Text cheeseText;

    [Header("Cheese Audio")]
    public AudioClip cheeseSqueak;

    private int cheeseCount = 0;

    private HashSet<SpriteRenderer> collectedCheeses =
        new HashSet<SpriteRenderer>();

    private bool wasAlive = true;

    private void Start()
    {
        UpdateCheeseText();
    }

    public void SetGameplayVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    private void Update()
    {
        if (player == null)
            return;

        // Update health display
        if (healthText != null)
        {
            healthText.text =
                "Health: " + player.data.health +
                " / " + player.data.maxHealth;
        }

        // Reset cheese count when the player dies
        if (wasAlive && !player.IsAlive)
        {
            cheeseCount = 0;
            UpdateCheeseText();
        }

        wasAlive = player.IsAlive;

        // Cheese visuals are spawned by the puzzle system after this HUD starts.
        SpriteRenderer[] sprites = Object.FindObjectsByType<SpriteRenderer>();

        foreach (SpriteRenderer cheese in sprites)
        {
            if (cheese == null ||
                !(cheese.gameObject.name == "Cheese" ||
                  cheese.gameObject.name.StartsWith("Cheese ")))
                continue;

            // A disabled renderer means the cheese was collected
            if (!cheese.enabled && collectedCheeses.Add(cheese))
            {
                cheeseCount++;
                UpdateCheeseText();

                // Play cheese sound independently of walking audio
                if (cheeseSqueak != null)
                {
                    AudioSource.PlayClipAtPoint(
                        cheeseSqueak,
                        player.transform.position,
                        1f
                    );
                }
            }
        }
    }

    private void UpdateCheeseText()
    {
        if (cheeseText != null)
        {
            cheeseText.text = "Cheese: " + cheeseCount;
        }
    }
}
