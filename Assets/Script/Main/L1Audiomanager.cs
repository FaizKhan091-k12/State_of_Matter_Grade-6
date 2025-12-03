using UnityEngine;
using UnityEngine.Events;

public class L1Audiomanager : MonoBehaviour
{
  public TypewriterTMP typewriterTMP;
  public static L1Audiomanager Instance;
  public AudioSource audioSource;

  public AudioClip[] audioclips;

  public UnityEvent Dragcompleted;
  public int audioIndex;
  public GameObject canvas;

  void Awake()
  {
    Instance = this;
    audioSource = GetComponent<AudioSource>();

    if (!canvas.activeInHierarchy)
    {
      PlaySpecificAudio(1);
      typewriterTMP.TypeText("Let’s start the simulation with keeping shape constant. Select the first shape ‘Cube’ to test.", 15f);
    }


  }


  public void PlaySpecificAudio(int clipsIndex)
  {
    StopAudioSource();
    audioSource.PlayOneShot(audioclips[clipsIndex]);
    audioIndex = clipsIndex;

    if (audioIndex == 0)
    {

    }
    if (audioIndex == 1)
    {
      typewriterTMP.TypeText("Let’s start the simulation with keeping air resistance constant. Select the first shape ‘Cube’ to test.", 15f);

    }
    if (audioIndex == 2)
    {
      typewriterTMP.TypeText("Drag the cube and attach it to the parachute. Keep the air resistance ‘OFF’.", 15f);
    }
    if (audioIndex == 3)
    {
      typewriterTMP.TypeText("Click Simulate and notice the time taken by object to fall. ", 15f);
    }
    if (audioIndex == 4)
    {
      typewriterTMP.TypeText("Reset and drag the cube back to the parachute. This time change the size of the parachute. ", 15f);
    }
    if (audioIndex == 5)
    {
      typewriterTMP.TypeText("Click simulate while keeping the air resistance ‘OFF’ and notice time of fall again", 15f);
    }
    if (audioIndex == 6)
    {
      typewriterTMP.TypeText("Now Reset. And drop the cube with small parachute with Air Resistance ‘ON’.", 15f);
    }
    if (audioIndex == 7)
    {
      typewriterTMP.TypeText("Click reset and finally drop the cube with the larger parachute and Air Resistance ‘ON’. Notice the time of fall. ", 15f);
    }
    if (audioIndex == 8)
    {
      typewriterTMP.TypeText("Click on Data to see the time of fall for all the cases. ", 15f);
    }
    if (audioIndex == 9)
    {
      typewriterTMP.TypeText("The data shows that the time of fall is same when air resistance is absence and when air resistance is present the larger parachute experiences more resistance which increases the time of fall. now let's close the data sheet.", 15f);
    }
    if (audioIndex == 10)
    {
      typewriterTMP.TypeText("Want to test more shapes with the same air resistance? Go ahead! Or click ‘Keeping Air Resistance Constant’ to continue to the next stage. ", 15f);
    }
  }

  public void StopAudioSource()
  {
    audioSource.Stop();
  }


}
