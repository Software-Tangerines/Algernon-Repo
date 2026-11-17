using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class PlayerHUD : MonoBehaviour
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

        // Reset cheese when the player dies
        if (wasAlive && !player.IsAlive)
        {
            cheeseCount = 0;
            collectedCheeses.Clear();
            UpdateCheeseText();
        }

        wasAlive = player.IsAlive;

        // Find cheese sprites
        SpriteRenderer[] sprites =
            Object.FindObjectsByType<SpriteRenderer>();

        foreach (SpriteRenderer sprite in sprites)
        {
            if (sprite.gameObject.name != "Cheese")
                continue;

            // Cheese is collected when its renderer is disabled
            if (!sprite.enabled && collectedCheeses.Add(sprite))
            {
                cheeseCount++;
                UpdateCheeseText();

                // Play squeak independently of the walking AudioSource
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