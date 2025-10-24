using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI.ProceduralImage;
public class ResetManager : MonoBehaviour
{
    public MainMenuButtonsBehaviour mainMenuButtonsBehaviour;
    public QuizManager quizManager;

    public ProceduralImage resetImage;
    public float rotSpeed;

    public void ResetCurrentLevel()
    {
        if (mainMenuButtonsBehaviour.buttonclicked[0] == true)
        {
            mainMenuButtonsBehaviour.IntroductionEvent.Invoke();

            quizManager.ImageScale();
        }
        else if (mainMenuButtonsBehaviour.buttonclicked[1] == true)
        {
            mainMenuButtonsBehaviour.CompareEvent.Invoke();
                  quizManager.ImageScale();
        }
        else if (mainMenuButtonsBehaviour.buttonclicked[2] == true)
        {
            mainMenuButtonsBehaviour.StagesEvent.Invoke();
                  quizManager.ImageScale();
        }
        else if (mainMenuButtonsBehaviour.buttonclicked[3] == true)
        {
            mainMenuButtonsBehaviour.QuizEvent.Invoke();
                  quizManager.ImageScale();
        }
        StartCoroutine(RotateButton());
    }


    IEnumerator RotateButton()
    {
        quizManager.ImageScale();
        float duration = 1f / rotSpeed; // rotation time based on speed
        float elapsed = 0f;

        // Cache the initial rotation
        float startRot = resetImage.transform.localEulerAngles.z;
        float targetRot = startRot + 360f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float zRot = Mathf.Lerp(startRot, targetRot, t);
            resetImage.transform.localEulerAngles = new Vector3(0, 0, zRot);
            yield return null;
        }

        // Snap exactly to target (prevents tiny drift)
        resetImage.transform.localEulerAngles = new Vector3(0, 0, targetRot);
    }

}
