using UnityEngine;

public class UIAudioManager : MonoBehaviour
{
    public static UIAudioManager Instance;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;
    [SerializeField][Range(0f, 1f)] private float volume = 0.5f;

    private bool canPlaySounds = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.volume = volume;
    }

    void Start()
    {
        // Espera un frame antes de permitir sonidos
        Invoke(nameof(EnableSounds), 0.1f);
    }

    private void EnableSounds()
    {
        canPlaySounds = true;
    }

    public void PlayHover()
    {
        if (canPlaySounds && hoverSound != null)
            audioSource.PlayOneShot(hoverSound, volume);
    }

    public void PlayClick()
    {
        if (clickSound != null)
            audioSource.PlayOneShot(clickSound, volume);
    }
}