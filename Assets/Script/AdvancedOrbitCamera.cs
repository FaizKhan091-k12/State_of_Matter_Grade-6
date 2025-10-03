using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class AdvancedOrbitCamera : MonoBehaviour
{
    public static AdvancedOrbitCamera instance;

    [Header("Camera Settings")]
    [SerializeField] private Camera orbitCamera;

    [Header("Initial View")]
    [SerializeField] public float defaultZoom = 10f;
    [SerializeField] public float defaultHorizontalRotation = 0f;
    [SerializeField] public float defaultVerticalRotation = 20f;

    [Header("Target Settings")]
    [SerializeField] public Transform target;
    [SerializeField] private Vector3 cameraTargetOffset = Vector3.zero;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private bool invertY = false;

    [SerializeField] public bool restrictVerticalRotation = true;
    [SerializeField] public float minVerticalAngle = -20f;
    [SerializeField] public float maxVerticalAngle = 80f;

    [SerializeField] public bool restrictHorizontalRotation = false;
    [SerializeField] public float minHorizontalAngle = -360f;
    [SerializeField] public float maxHorizontalAngle = 360f;

    [SerializeField] private bool requireRightClick = false;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] public float minZoom = 3f;
    [SerializeField] public float maxZoom = 20f;

    [Header("Smoothing")]
    [SerializeField] private float smoothTime = 0.15f;

    [Header("Target Height Mapping")]
    [SerializeField] public float minTargetY = 0f;
    [SerializeField] public float maxTargetY = 5f;
    [SerializeField] public bool useTargetYPosition = false;
    [SerializeField] public float minTargetX = 0f;
    [SerializeField] public float maxTargetX = 5f;
    [SerializeField] public bool useTargetXPosition = false;

    public bool canOrbit = true;

    private float targetX, targetY, currentX, currentY;
    private float rotationVelocityX, rotationVelocityY;
    private float targetDistance, currentDistance, zoomVelocity;

    [Header("Reset Default On Events")]
    public bool resetValues;
    public float setdefaultZoom, setdefaultHorizontalRotation, setdefaultVerticalRotation;

    [Header("Recoil Shake")]
    public bool enableRecoilShake = false;
    public float shakeDuration = 0.5f;
    public float shakeMagnitude = 0.2f;

    private float shakeTimer = 0f;
    private Vector3 recoilOffset = Vector3.zero;

    [SerializeField] private Button launchButton;
    [SerializeField] private float zoomOutSpeed;
    [SerializeField] private bool dynamiczoomOut;

    [SerializeField] private Button zoomInButton;
    [SerializeField] private Button zoomOutButton;
    [SerializeField] private float zoomStep = 1f;
    [SerializeField] private float zoomHoldSpeed = 2f;

    private bool isHoldingZoomIn = false;
    private bool isHoldingZoomOut = false;


    private void Awake()
    {
        instance = this;
        if (orbitCamera == null)
            orbitCamera = Camera.main;
    }

    private void Start()
    {
        if (!target)
            target = new GameObject("CameraTarget").transform;


        if (zoomInButton != null && zoomOutButton != null)
        {
            zoomInButton.onClick.AddListener(ZoomInOnce);
            zoomOutButton.onClick.AddListener(ZoomOutOnce);

            // Optional: EventTrigger for hold detection
            AddHoldEvents(zoomInButton, () => isHoldingZoomIn = true, () => isHoldingZoomIn = false);
            AddHoldEvents(zoomOutButton, () => isHoldingZoomOut = true, () => isHoldingZoomOut = false);
        }


        if (launchButton != null)
        {

            launchButton.onClick.AddListener(LuanchManager);
        }
        ApplyInitialView();
    }

    public void StartRecoilShake()
    {
        enableRecoilShake = true;
        shakeTimer = shakeDuration;
    }

    public void LuanchManager()
    {
        if (!dynamiczoomOut) return;
        StartCoroutine(LaunchButton());
    }

    IEnumerator LaunchButton()
    {
        float t = 0f;
        yield return new WaitForEndOfFrame();
        while (t < 1)
        {
            t += Time.deltaTime * zoomOutSpeed;
            defaultZoom = Mathf.Lerp(minZoom, maxZoom, t);
            defaultVerticalRotation = Mathf.Lerp(0, 20, t);
            defaultHorizontalRotation = 0;
            ApplyInitialView();
            yield return null;
        }
    }

    public void ResetValues()
    {
        if (resetValues)
        {
            FollowSpecificTarget();
            resetValues = false;
        }
    }

    public void FollowSpecificTarget()
    {
        setdefaultZoom = Mathf.Clamp(setdefaultZoom, minZoom, maxZoom);
        if (restrictVerticalRotation)
            setdefaultVerticalRotation = Mathf.Clamp(setdefaultVerticalRotation, minVerticalAngle, maxVerticalAngle);
        if (restrictHorizontalRotation)
            setdefaultHorizontalRotation = Mathf.Clamp(setdefaultHorizontalRotation, minHorizontalAngle, maxHorizontalAngle);

        targetDistance = setdefaultZoom;
        targetX = setdefaultHorizontalRotation;
        targetY = setdefaultVerticalRotation;
    }

    public void ResetTagetValues()
    {
        float setdefaultVerticalRotation1 = 0;
        float setdefaultHorizontalRotation1 = 0;
        float setdefaultZoom = Mathf.Clamp(2.4f, minZoom, maxZoom);
        if (restrictVerticalRotation)
            setdefaultVerticalRotation1 = Mathf.Clamp(0, minVerticalAngle, maxVerticalAngle);
        if (restrictHorizontalRotation)
            setdefaultHorizontalRotation1 = Mathf.Clamp(0, minHorizontalAngle, maxHorizontalAngle);

        targetDistance = setdefaultZoom;
        targetX = setdefaultHorizontalRotation1;
        targetY = setdefaultVerticalRotation1;
    }

    private void ApplyInitialView()
    {
        defaultZoom = Mathf.Clamp(defaultZoom, minZoom, maxZoom);
        if (restrictVerticalRotation)
            defaultVerticalRotation = Mathf.Clamp(defaultVerticalRotation, minVerticalAngle, maxVerticalAngle);
        if (restrictHorizontalRotation)
            defaultHorizontalRotation = Mathf.Clamp(defaultHorizontalRotation, minHorizontalAngle, maxHorizontalAngle);

        targetDistance = currentDistance = defaultZoom;
        targetX = currentX = defaultHorizontalRotation;
        targetY = currentY = defaultVerticalRotation;
    }

    private void OnValidate()
    {
        if (!Application.isPlaying) return;
        ApplyInitialView();
    }

    public void OrbitControlsWorkingState(bool CanOrbit)
    {
        this.canOrbit = CanOrbit;
    }

    
    private void LateUpdate()
    {
        if (!canOrbit || orbitCamera == null) return;

        HandleInput();
        ApplySmoothCamera();

        if (enableRecoilShake)
        {
            shakeTimer -= Time.deltaTime;
            float recoilZ = Mathf.Lerp(shakeMagnitude, 0f, 1 - (shakeTimer / shakeDuration));
            Vector2 jitter = Random.insideUnitCircle * (shakeMagnitude * 0.2f);
            recoilOffset = orbitCamera.transform.TransformDirection(new Vector3(jitter.x, jitter.y, -recoilZ));
            if (shakeTimer <= 0f)
            {
                enableRecoilShake = false;
                recoilOffset = Vector3.zero;
            }
        }
        else recoilOffset = Vector3.zero;

    #if UNITY_EDITOR
        if (Application.isPlaying)
        {
            defaultHorizontalRotation = currentX;
            defaultVerticalRotation = currentY;
            defaultZoom = currentDistance;
        }
    #endif
    }

    private void HandleInput()
    {
        bool isPointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        // Mouse support
    #if UNITY_EDITOR || UNITY_STANDALONE || (!UNITY_ANDROID && !UNITY_IOS && UNITY_WEBGL)
        bool isRotating = !requireRightClick || Input.GetMouseButton(1);
        if (!isPointerOverUI && isRotating && Input.GetMouseButton(0))
        {
            targetX += Input.GetAxis("Mouse X") * rotationSpeed;
            float yDelta = Input.GetAxis("Mouse Y") * rotationSpeed;
            targetY += invertY ? yDelta : -yDelta;
        }

        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            targetDistance -= scroll * zoomSpeed;
        }
    #endif

        // Touch support
    #if UNITY_ANDROID || UNITY_IOS 
        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            Vector2 delta = Input.GetTouch(0).deltaPosition;
            targetX += delta.x * rotationSpeed * 0.02f;
            targetY += (invertY ? delta.y : -delta.y) * rotationSpeed * 0.02f;
        }
        else if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            Vector2 prevPos0 = t0.position - t0.deltaPosition;
            Vector2 prevPos1 = t1.position - t1.deltaPosition;
            float prevMag = Vector2.Distance(prevPos0, prevPos1);
            float currMag = Vector2.Distance(t0.position, t1.position);
            float deltaMag = currMag - prevMag;

            targetDistance -= deltaMag * zoomSpeed * 0.01f;
        }
    #endif

        targetDistance = Mathf.Clamp(targetDistance, minZoom, maxZoom);
        if (restrictVerticalRotation)
            targetY = Mathf.Clamp(targetY, minVerticalAngle, maxVerticalAngle);
        if (restrictHorizontalRotation)
            targetX = Mathf.Clamp(targetX, minHorizontalAngle, maxHorizontalAngle);
    }

    private void ApplySmoothCamera()
    {
        currentX = Mathf.SmoothDamp(currentX, targetX, ref rotationVelocityX, smoothTime);
        currentY = Mathf.SmoothDamp(currentY, targetY, ref rotationVelocityY, smoothTime);
        currentDistance = Mathf.SmoothDamp(currentDistance, targetDistance, ref zoomVelocity, smoothTime);

        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 direction = rotation * Vector3.forward;

        float mappedY = useTargetYPosition ? Mathf.Lerp(minTargetY, maxTargetY, Mathf.InverseLerp(minZoom, maxZoom, currentDistance)) : target.position.y;
        float mappedX = useTargetXPosition ? Mathf.Lerp(minTargetX, maxTargetX, Mathf.InverseLerp(minZoom, maxZoom, currentDistance)) : target.position.x;

        Vector3 updatedTargetPos = new Vector3(mappedX, mappedY, target.position.z);
        target.position = updatedTargetPos;

        orbitCamera.transform.position = updatedTargetPos + cameraTargetOffset - direction * currentDistance + recoilOffset;
        orbitCamera.transform.LookAt(updatedTargetPos + cameraTargetOffset);
    }
    void ZoomInOnce() => targetDistance -= zoomStep;
    void ZoomOutOnce() => targetDistance += zoomStep;

    private void Update()
    {
        if (isHoldingZoomIn)
            targetDistance -= zoomHoldSpeed * Time.deltaTime;

        if (isHoldingZoomOut)
            targetDistance += zoomHoldSpeed * Time.deltaTime;
    }
    private void AddHoldEvents(Button button, UnityEngine.Events.UnityAction onDown, UnityEngine.Events.UnityAction onUp)
    {
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null) trigger = button.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry press = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        press.callback.AddListener((_) => onDown());

        EventTrigger.Entry release = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        release.callback.AddListener((_) => onUp());

        trigger.triggers.Add(press);
        trigger.triggers.Add(release);
    }



#if UNITY_EDITOR
    public float CurrentUpRotation => currentY;
    public float CurrentSideRotation => currentX;
    public float CurrentZoom => currentDistance;
#endif
}
