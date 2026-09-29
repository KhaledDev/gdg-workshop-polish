using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Stage1_Projectile : MonoBehaviour
{
    [SerializeField] float maxFlightTime = 4f;
    [SerializeField] float resetDelayAfterHit = 1f;

    public bool IsFlying { get; private set; }

    Rigidbody rb;
    Vector3 startPosition;
    Quaternion startRotation;
    bool hasHit;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;
        startRotation = transform.rotation;

        // Kinematic = physics ignores it, so the slingshot can move it freely.
        rb.isKinematic = true;
    }

    public void Launch(Vector3 force)
    {
        IsFlying = true;
        hasHit = false;

        rb.isKinematic = false;
        rb.AddForce(force, ForceMode.Impulse);

        // Missed? Reset anyway after a while.
        Invoke(nameof(ResetProjectile), maxFlightTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!IsFlying || hasHit)
            return;

        Stage1_Target target = collision.collider.GetComponentInParent<Stage1_Target>();
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
        IsFlying = false;
    }
}
