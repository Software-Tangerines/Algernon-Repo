
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource walkingAudioSource;
    public AudioClip walkingSound;

    [Header("Cheese")]
    public AudioClip cheeseSqueak;

    private void Start()
    {
        if (walkingAudioSource == null || walkingSound == null)
            return;

        walkingAudioSource.clip = walkingSound;
        walkingAudioSource.loop = true;
        walkingAudioSource.playOnAwake = false;
    }

    public void PlayWalkingSound(Vector2 movement)
    {
        if (walkingAudioSource == null || walkingSound == null)
            return;

        if (movement.magnitude > 0.1f)
        {
            if (!walkingAudioSource.isPlaying)
                walkingAudioSource.Play();
        }
        else
        {
            if (walkingAudioSource.isPlaying)
                walkingAudioSource.Stop();
        }
    }

    public void PlayCheeseSound()
    {
        if (walkingAudioSource == null || cheeseSqueak == null)
            return;

        walkingAudioSource.PlayOneShot(cheeseSqueak);
    }
}
