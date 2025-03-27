using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class CameraController : MonoBehaviour
{
    //Initial transform values
    [SerializeField] private Transform target; 
    private Transform initialCameraTransform;
    private Vector3 initialTargetPosition;

    // Movement variables
    public float panSpeed = 20f;
    public float zoomSpeed = 20f;
    public float rotateSpeed = 100f;

    // Zoom variables
    [SerializeField] private float minZoom = 5f; // Adjusted minZoom
    [SerializeField] private float maxZoom = 50f; // Adjusted maxZoom
    private float currentZoom;

    // Rotation variables
    private Vector3 lastMousePosition;
    private bool isRotating = false;

    // Pivot Point Variables
    public LayerMask modelLayer; // Layer for the 3D model
    public GameObject pivotMarker; // Prefab for the pivot marker
    private Vector3 currentPivotPoint;
    private bool isPivotChanged = false;
    private Vector3 pivotOffset;
    public bool enablePivotChange = false;

    // Mini-Map Variables
    public Camera miniMapCamera;
    public RawImage miniMapRawImage;
    public LayerMask miniMapLayer;
    private Bounds modelBounds;
    public float miniMapPadding = 5f; // Padding around the model in the mini-map
    public float transitionSpeed = 5f; // Speed for smooth camera transition

    private bool isTransitioning = false;
    private Vector3 targetPosition;
    public bool enableMinimap = false;


    void Start()
    {
        //Save initial transform
        initialCameraTransform = transform;
        initialTargetPosition = target.transform.position;
        currentZoom = Vector3.Distance(transform.position, target.position);
        currentPivotPoint = target.position;

        //Lock Camera view to the center
        transform.LookAt(target); 

        // Ensure the pivot marker is initially at the target position
        if (pivotMarker != null)
        {
            pivotMarker.transform.position = currentPivotPoint;
        }
        
        // Mini-Map Setup
        SetupMiniMap();
    }
    void Update()
    {
        HandleMouseInput();

        if (isTransitioning)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * transitionSpeed);
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                isTransitioning = false;
                transform.position = targetPosition;
            }
            transform.LookAt(currentPivotPoint);
        }
    }

    private void HandleMouseInput()
    {
        // Pan
        if (Input.GetMouseButton(2)) // Middle mouse button
        {
            PanCamera();
        }

        // Zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            ZoomCamera(scroll);
        }

        // Rotate
        if (Input.GetMouseButtonDown(1)) // Right mouse button
        {
            isRotating = true;
            lastMousePosition = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(1))
        {
            isRotating = false;
        }

        if (isRotating)
        {
            RotateCamera();
        }

        // Change Pivot Point
        if (Input.GetMouseButtonDown(0) && enablePivotChange)
        {
            
            ChangePivotPoint();
        }

        //Mini-Map click
        if (Input.GetMouseButtonDown(0) && !enablePivotChange && enableMinimap )
        {
            if(RectTransformUtility.RectangleContainsScreenPoint(miniMapRawImage.rectTransform, Input.mousePosition))
            {
                MoveCameraFromMiniMapClick();
            }
        }

    }

    private void PanCamera()
    {
        Vector3 pan = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0);
        pan *= Time.deltaTime * panSpeed;

        // Calculate the right and up vectors relative to the camera's current rotation
        Vector3 right = transform.right;
        Vector3 up = transform.up;

        // Move the target (and consequently the camera)
        // target.position -= right * pan.x + up * pan.y;
        transform.position -= right * pan.x + up * pan.y;
    }

    private void ZoomCamera(float scroll)
    {
        currentZoom -= scroll * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

        // New Zoom Logic:
        Vector3 zoomDirection = transform.forward; // Zoom along the camera's forward (Z) axis
        transform.position += zoomDirection * scroll * zoomSpeed;

        // Calculate the new distance from the target
        float newDistance = Vector3.Distance(transform.position, currentPivotPoint);

        // Clamp the distance to the min/max zoom values
        if (newDistance < minZoom)
        {
            transform.position = currentPivotPoint + zoomDirection * minZoom;
        }
        else if (newDistance > maxZoom)
        {
            transform.position = currentPivotPoint + zoomDirection * maxZoom;
        }
    }

    private void RotateCamera()
    {
        Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
        lastMousePosition = Input.mousePosition;

        // Calculate the rotation angles
        float rotationX = mouseDelta.x * rotateSpeed * Time.deltaTime;
        float rotationY = -mouseDelta.y * rotateSpeed * Time.deltaTime;

        // Create a rotation quaternion based on the calculated angles
        Quaternion rotation = Quaternion.Euler(rotationY, rotationX, 0);

        // Calculate the new camera position relative to the pivot point
        Vector3 relativePosition = transform.position - currentPivotPoint;
        Vector3 rotatedRelativePosition = rotation * relativePosition;
        Vector3 newPosition = currentPivotPoint + rotatedRelativePosition;

        // Apply the new position and rotation
        transform.position = newPosition;
        transform.LookAt(currentPivotPoint);
    }

    //reset camera position, rotation, lookAtPoint and orthograpic size.
    public void ResetPosition()
    {
        transform.position = initialCameraTransform.position;
        transform.rotation = initialCameraTransform.rotation;
        transform.LookAt(initialTargetPosition);
        target.position = initialTargetPosition;
        currentPivotPoint = initialTargetPosition;
        currentZoom = Vector3.Distance(transform.position, target.position);

        // Reset the pivot marker's position
        if (pivotMarker != null)
        {
            pivotMarker.transform.position = currentPivotPoint;
        }

        // Disable pivot change after reset
        enablePivotChange = false;
    }

    public void ChangePivotPoint()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, modelLayer))
        {
            // Calculate the offset from the old pivot to the new pivot
            pivotOffset = hit.point - currentPivotPoint;

            // Update the current pivot point
            currentPivotPoint = hit.point;

            // Move the pivot marker to the new position
            if (pivotMarker != null)
            {
                pivotMarker.transform.position = currentPivotPoint;
            }

            // Ensure the camera is still looking at the new pivot point
            transform.LookAt(currentPivotPoint);

            isPivotChanged = true;
        }
    }

    public void TogglePivotChange(bool enable)
    {
        enablePivotChange = enable;
    }

    //Mini-Map Functions
    private void SetupMiniMap()
    {
         // Calculate model bounds
        Renderer[] renderers = FindObjectsOfType<Renderer>();
        if (renderers.Length > 0)
        {
            modelBounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                modelBounds.Encapsulate(renderers[i].bounds);
            }
        }
        else
        {
            Debug.LogError("No renderers found in the scene to calculate model bounds.");
            return;
        }

        // Position and configure the mini-map camera
        miniMapCamera.transform.position = new Vector3(modelBounds.center.x, modelBounds.max.y + 10f, modelBounds.center.z);
        miniMapCamera.transform.LookAt(modelBounds.center);
        miniMapCamera.orthographic = true;

        // Adjust the orthographic size to fit the model with padding
        float maxBound = Mathf.Max(modelBounds.extents.x, modelBounds.extents.z);
        miniMapCamera.orthographicSize = maxBound + miniMapPadding;
        // miniMapCamera.cullingMask = miniMapLayer;
    }

    private void MoveCameraFromMiniMapClick()
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(miniMapRawImage.rectTransform, Input.mousePosition, null, out localPoint);

        // Normalize the local point to a 0-1 range
        Vector2 normalizedPoint = Rect.PointToNormalized(miniMapRawImage.rectTransform.rect, localPoint);

        // Convert the normalized point to world space
        float worldX = Mathf.Lerp(modelBounds.min.x, modelBounds.max.x, normalizedPoint.x);
        float worldZ = Mathf.Lerp(modelBounds.min.z, modelBounds.max.z, normalizedPoint.y);

        // Set the target position for the main camera
        targetPosition = new Vector3(worldX, transform.position.y, worldZ);

        // Ensure the camera stays within the model bounds
        targetPosition.x = Mathf.Clamp(targetPosition.x, modelBounds.min.x, modelBounds.max.x);
        targetPosition.z = Mathf.Clamp(targetPosition.z, modelBounds.min.z, modelBounds.max.z);

        // Smooth Transition
        isTransitioning = true;
    }
    public void ToggleMiniMap(bool enable)
    {
        enableMinimap = enable;
    }
}
