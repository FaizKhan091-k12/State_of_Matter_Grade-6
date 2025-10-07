using UnityEngine;

[RequireComponent(typeof(Outline))]
public class OutlineBehaviour : MonoBehaviour
{
    public float minWidth = 0f;
    public float maxWidth = 5f;
    public float speed = 2f; // cycles per second

    private Outline outline;

    void Awake()
    {
        outline = GetComponent<Outline>();
    }

    void OnEnable()
    {
        if (outline != null)
        {
            outline.OutlineWidth = minWidth;
            outline.enabled = true;
        }
    }

    void Update()
    {
        if (outline == null) return;

        // PingPong between min and max width
        float t = Mathf.PingPong(Time.time * speed, 1f);
        outline.OutlineWidth = Mathf.Lerp(minWidth, maxWidth, t);
    }

    void OnDisable()
    {
        if (outline != null)
        {
            // Reset to min width when effect stops
            outline.OutlineWidth = 0;
            outline.enabled = false;
        }
    }
}
