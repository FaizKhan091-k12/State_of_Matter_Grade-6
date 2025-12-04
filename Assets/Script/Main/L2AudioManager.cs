using UnityEngine;
using UnityEngine.Events;

public class L2Audiomanager : MonoBehaviour
{
    public TypewriterTMP typewriterTMP;
    public static L2Audiomanager Instance;
    public AudioSource audioSource;

    public AudioClip[] audioclips;

    public UnityEvent Dragcompleted;
    public int audioIndex;

    void Awake()
    {
        Instance = this;

        audioSource = GetComponent<AudioSource>();


        typewriterTMP.TypeText("Attach all the objects to the parachute. Run the simulation with both small and large parachutes and observe how each object falls with air resistance on and off.", 15f);



    }

    public void PlaySpecificAudio(int clipsIndex)
    {
        StopAudioSource();
        if (audioSource == null) return;
        audioSource.PlayOneShot(audioclips[clipsIndex]);
        audioIndex = clipsIndex;
        typewriterTMP.TypeText("Click on Data sheet and see time of fall.", 15f);

    }

    public void StopAudioSource()
    {
        if (audioSource != null)
            audioSource.Stop();
    }

    public void gotoQuizOne()
    {
        if (audioSource == null) return;
        PlaySpecificAudio(1);
        typewriterTMP.TypeText("From this experiment, you understand air resistance and shape of an object both are important factors for understanding objects under a fall. Let’s go the quiz and see what you have understood from the simulation.", 15f);

    }

        public void gotoQuizTwo()
    {
        if (audioSource == null) return;
        PlaySpecificAudio(1);
        typewriterTMP.TypeText("Let’s go the quiz and see what you have understood from the simulation.", 15f);

    }



}
