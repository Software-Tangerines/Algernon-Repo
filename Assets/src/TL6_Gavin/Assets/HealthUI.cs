using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    public Image healthImage;
    public Sprite[] heartSprites;

    public void UpdateHealth(int currentHealth)
    {
        int spriteIndex = 10 - Mathf.Clamp(currentHealth / 10, 0, 10);

        if (heartSprites == null || heartSprites.Length <= spriteIndex)
            return;

        healthImage.sprite = heartSprites[spriteIndex];
    }
}