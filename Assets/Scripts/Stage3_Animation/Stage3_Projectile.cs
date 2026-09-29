using PrimeTween;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Stage3_Projectile : MonoBehaviour
{
    [SerializeField] float maxFlightTime = 4f;
    [SerializeField] float resetDelayAfterHit = 1f;

    [Header("Squash & Stretch")]
    [SerializeField] float launchStretch = 0.6f;
    [SerializeField] float stretchDuration = 0.5f;

    public bool IsFlying { get; private set; }

    Rigidbody rb;
    Vector3 startPosition;
    Quaternion startRotation;
    Vector3 restScale;
    bool hasHit;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;
        startRotation = transform.rotation;
        restScale = transform.localScale;

        // Kinematic = physics ignores it, so the slingshot can move it freely.
        rb.isKinematic = true;
    }

    public void Launch(Vector3 force)
    {
        IsFlying = true;
        hasHit = false;

        // Face the travel direction so "stretch along Z" means "stretch along the flight path".
        transform.rotation = Quaternion.LookRotation(force);

        rb.isKinematic = false;
        rb.AddForce(force, ForceMode.Impulse);

        // Squashed before release -> stretched on release -> wobbles back to normal.
        Vector3 stretched = Vector3.Scale(restScale, new Vector3(1f - launchStretch * 0.5f, 1f - launchStretch * 0.5f, 1f + launchStretch));
        Tween.Scale(transform, stretched, restScale, stretchDuration, Ease.OutElastic);

        // Missed? Reset anyway after a while.
        Invoke(nameof(ResetProjectile), maxFlightTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!IsFlying || hasHit)
            return;

        Stage3_Target target = collision.collider.GetComponentInParent<Stage3_Target>();
        if (target == null)
            return;

        hasHit = true;
        target.Hit();

        CancelInvoke(nameof(ResetProjectile));
        Invoke(nameof(ResetProjectile), resetDelayAfterHit);
    }

    void ResetProjectile()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        transform.SetPositionAndRotation(startPosition, startRotation);

        // Pop back in instead of just appearing.
        Tween.StopAll(transform);
        Tween.Scale(transform, Vector3.zero, restScale, 0.35f, Ease.OutBack);

        IsFlying = false;
    }
}
