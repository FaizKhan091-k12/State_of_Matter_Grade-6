using UnityEngine;

[DisallowMultipleComponent]
public class LookAtCamera : MonoBehaviour
{
    [Tooltip("Leave empty to use Camera.main. Assign if you want a specific camera.")]
    public Camera targetCamera;

    [Tooltip("Keep only the Y axis rotation (billboard upright).")]
    public bool onlyRotateY = false;

    void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    void LateUpdate()
    {
        if (targetCamera == null) return;

        if (onlyRotateY)
        {
            // Get direction from object to camera, but flatten on Y
            Vector3 dir = targetCamera.transform.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(dir);
            }
        }
        else
        {
            // Full look at camera
            transform.LookAt(targetCamera.transform);
        }
    }
}
