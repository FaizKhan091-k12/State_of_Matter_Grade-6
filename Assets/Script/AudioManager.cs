using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlaySpecificDialogue(AudioClip audioClip)
    {
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(audioClip);
        }
        else if (audioSource.isPlaying)
        {
            audioSource.Stop();
            audioSource.PlayOneShot(audioClip);
        }
        else
        {
            Debug.Log("Error Playing Audio");
        }
      
    }
}
