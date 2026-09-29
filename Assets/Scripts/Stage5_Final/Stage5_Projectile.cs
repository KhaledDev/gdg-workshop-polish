using PrimeTween;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Stage5_Projectile : MonoBehaviour
{
    [SerializeField] float maxFlightTime = 4f;
    [SerializeField] float resetDelayAfterHit = 1.2f;

    [Header("Squash & Stretch")]
    [SerializeField] float launchStretch = 0.6f;
    [SerializeField] float stretchDuration = 0.5f;

    [Header("Feedback")]
    [SerializeField] TrailRenderer trail;
    [SerializeField] AudioSource sfxSource;
    [SerializeField] AudioClip whooshSound;
    [SerializeField] AudioClip bounceSound;

    public bool IsFlying { get; private set; }
    public float Mass => rb.mass;

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
        trail.emitting = false;
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

        trail.Clear();
        trail.emitting = true;

        PlaySound(whooshSound, 1f);

        // Missed? Reset anyway after a while.
        Invoke(nameof(ResetProjectile), maxFlightTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!IsFlying)
            return;

        Stage5_Target target = collision.collider.GetComponentInParent<Stage5_Target>();

        // Anything that isn't a fresh target hit still makes a sound, louder when it hits harder.
        if (target == null || hasHit)
        {
            float impactSpeed = collision.relativeVelocity.magnitude;
            PlaySound(bounceSound, Mathf.Clamp01(impactSpeed / 15f));
            return;
        }

        hasHit = true;
        target.Hit(collision.GetContact(0).point);

        CancelInvoke(nameof(ResetProjectile));
        Invoke(nameof(ResetProjectile), resetDelayAfterHit);
    }

    // Freeze -> shrink away -> move home -> pop back in -> ready.
    void ResetProjectile()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        trail.emitting = false;

        Tween.StopAll(transform);
        Sequence.Create()
            .Chain(Tween.Scale(transform, 0f, 0.2f, Ease.InBack))
            .ChainCallback(MoveToStart)
            .Chain(Tween.Scale(transform, Vector3.zero, restScale, 0.35f, Ease.OutBack))
            .ChainCallback(() => IsFlying = false);
    }

    void MoveToStart()
    {
        transform.SetPositionAndRotation(startPosition, startRotation);
        trail.Clear();
    }

    void PlaySound(AudioClip clip, float volume)
    {
        if (clip == null)
            return;

        sfxSource.pitch = Random.Range(0.9f, 1.1f);
        sfxSource.PlayOneShot(clip, volume);
    }
}
