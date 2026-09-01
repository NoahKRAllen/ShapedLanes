using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class TopDownCameraPan : MonoBehaviour
{
    [Header("Ground Reference")]
    [Tooltip("World Y height of the ground plane used to convert screen drags into world-space pan distance. " +
             "Should match the Y of the surface the units will walk on.")]
    [SerializeField]
    private float groundHeight = 0f;

    [Header("Pan limits")] 
    [SerializeField]
    private bool useBounds = true;
    [Tooltip("Minimum corner of map that camera is able to pan to.")]
    [SerializeField] 
    private Vector2 boundsMin = new Vector2(-50f, -50f);
    [Tooltip("Maximum corner of map that camera is able to pan to.")]
    [SerializeField]
    private Vector2 boundsMax = new Vector2(50f, 50f);

    [Header("Feel")]
    [Tooltip("1  = the point under the cursor/finger stays exactly under the cursor/finger while dragging")]
    [SerializeField]
    private float panSpeedMultiplier = 1f;

    [Tooltip("Drags shorter than this many pixels are ignored")] [SerializeField]
    private float dragDeadZonePixels = 4f;

    private Camera _camera;
    private Plane _groundPlane;
    private bool _isDragging;
    private bool _dragConfirmed;
    private Vector2 _dragStartScreenPos;
    private Vector3 _dragAnchorWorldPos;
    
    void Awake()
    {
        _camera = GetComponentInChildren<Camera>();
        
        if (_camera == null)
        {
            Debug.LogError($"{nameof(TopDownCameraPan)} requires a Camera in its children.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        _groundPlane = new Plane(Vector3.up, new Vector3(0f, groundHeight, 0f));
    }

    void Update()
    {
        if (TryGetPointerDown(out Vector2 screenPos))
        {
            BeginDrag(screenPos);
        }
        else if (TryGetPointerHeld(out screenPos))
        {
            ContinueDrag(screenPos);
        }
        else if (_isDragging)
        {
            EndDrag();
        }
    }

    private bool TryGetPointerDown(out Vector2 screenPos)
    {
        screenPos = default;
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return false;
            }

            screenPos = Mouse.current.position.ReadValue();
            return true;
        }

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            int touchId = Touchscreen.current.primaryTouch.touchId.ReadValue();
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touchId))
            {
                return false;
            }

            screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
            return true;
        }

        return false;
    }

    private bool TryGetPointerHeld(out Vector2 screenPos)
    {
        screenPos = default;
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            screenPos = Mouse.current.position.ReadValue();
            return true;
        }

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
            return true;
        }

        return false;
    }
    private void BeginDrag(Vector2 screenPos)
    {
        _isDragging = true;
        _dragConfirmed = false;
        _dragStartScreenPos = screenPos;

        if (ScreenPointToGround(screenPos, out Vector3 worldPos))
        {
            _dragAnchorWorldPos = worldPos;
        }
    }

    private void ContinueDrag(Vector2 screenPos)
    {
        if (!_isDragging)
        {
            return;
        }

        if (!_dragConfirmed)
        {
            //Requires a minimum movement before treating this as a pan
            if (Vector2.Distance(screenPos, _dragStartScreenPos) <= dragDeadZonePixels)
            {
                return;
            }
            _dragConfirmed = true;
        }

        if (!ScreenPointToGround(screenPos, out Vector3 currentWorldPos))
        {
            return;
        }
        
        Vector3 worldDelta = _dragAnchorWorldPos - currentWorldPos;
        worldDelta.y = 0f;
        
        Vector3 newPosition = transform.position + worldDelta * panSpeedMultiplier;

        if (useBounds)
        {
            newPosition.x = Mathf.Clamp(newPosition.x, boundsMin.x, boundsMax.x);
            newPosition.z = Mathf.Clamp(newPosition.z, boundsMin.y, boundsMax.y);
        }

        transform.position = newPosition;

        //Re-anchor at the camera's new position so the same screen point keeps tracking the same world point
        //on next frame instead of drifting
        if (ScreenPointToGround(screenPos, out Vector3 reAnchoredWorldPos))
        {
            _dragAnchorWorldPos = reAnchoredWorldPos;
        }
    }

    private void EndDrag()
    {
        _isDragging = false;
        _dragConfirmed = false;
    }
    private bool ScreenPointToGround(Vector2 screenPos, out Vector3 worldPos)
    {
        Ray ray = _camera.ScreenPointToRay(screenPos);
        if (_groundPlane.Raycast(ray, out float distance))
        {
            worldPos = ray.GetPoint(distance);
            return true;
        }

        worldPos = default;
        return false;
    }
}
