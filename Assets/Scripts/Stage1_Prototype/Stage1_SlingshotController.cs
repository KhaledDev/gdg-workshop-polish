using UnityEngine;

public class Stage1_SlingshotController : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] Stage1_Projectile projectile;

    [SerializeField] float maxPullDistance = 1.5f;
    [SerializeField] float maxLaunchForce = 20f;
    [SerializeField] float dragSensitivity = 6f;
    [SerializeField] float pullDownAmount = 0.4f;
    [SerializeField] float minPullToLaunch = 0.1f;

    Vector3 restPosition;
    Vector3 dragStartMouse;
    Vector3 pullOffset;
    float pullAmount;
    bool isDragging;

    void Start()
    {
        restPosition = projectile.transform.position;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && IsMouseOverProjectile())
            StartDrag();

        if (!isDragging)
            return;

        UpdateDrag();

        if (Input.GetMouseButtonUp(0))
            Release();
    }

    bool IsMouseOverProjectile()
    {
        if (projectile.IsFlying)
            return false;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject == projectile.gameObject;
    }

    void StartDrag()
    {
        isDragging = true;
        dragStartMouse = Input.mousePosition;
    }

    void UpdateDrag()
    {
        // Mouse movement in "screen heights", so it feels the same at any resolution.
        Vector3 mouseDelta = (Input.mousePosition - dragStartMouse) / Screen.height * dragSensitivity;

        // Dragging the mouse down pulls the projectile back (and a little down). Dragging up does nothing.
        float back = Mathf.Min(mouseDelta.y, 0f);
        Vector3 pull = new Vector3(mouseDelta.x, back * pullDownAmount, back);
        pull = Vector3.ClampMagnitude(pull, maxPullDistance);

        // Convert the pull distance into a reusable 0-1 value.
        pullAmount = pull.magnitude / maxPullDistance;
        pullOffset = pull;

        projectile.transform.position = restPosition + pullOffset;
    }

    void Release()
    {
        isDragging = false;

        if (pullAmount < minPullToLaunch)
        {
            projectile.transform.position = restPosition;
            return;
        }

        // Launch in the opposite direction of the pull. More pull = more force.
        Vector3 launchDirection = -pullOffset.normalized;
        Vector3 launchForce = launchDirection * pullAmount * maxLaunchForce;
        projectile.Launch(launchForce);
    }
}
