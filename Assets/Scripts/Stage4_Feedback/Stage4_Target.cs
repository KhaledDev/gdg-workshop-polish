using PrimeTween;
using UnityEngine;

// Put this on a pivot at the BOTTOM of the target face, so it tips backward like a hinge.
public class Stage4_Target : MonoBehaviour
{
    [SerializeField] float hitTiltAngle = 25f;
    [SerializeField] float tiltDuration = 0.08f;
    [SerializeField] float settleDuration = 0.8f;

    [Header("Feedback")]
    [SerializeField] Stage4_CameraFeedback cameraFeedback;
    [SerializeField] ParticleSystem impactParticles;
    [SerializeField] AudioSource sfxSource;
    [SerializeField] AudioClip impactSound;
    [SerializeField] float hitStopDuration = 0.06f;

    Quaternion restRotation;
    Sequence hitSequence;
    int hitCount;

    void Awake()
    {
        restRotation = transform.localRotation;
    }

    // IMPACT = Animation + VFX + Camera + Sound + Timing. Each one is small; together they land.
    public void Hit(Vector3 hitPoint)
    {
        hitCount++;
        Debug.Log($"Target hit! Total hits: {hitCount}");

        PlayHitAnimation();

        impactParticles.transform.position = hitPoint;
        impactParticles.Play();

        if (impactSound != null)
        {
            sfxSource.pitch = Random.Range(0.9f, 1.1f);
            sfxSource.PlayOneShot(impactSound);
        }

        cameraFeedback.Impact();
        HitStop();
    }

    void PlayHitAnimation()
    {
        // Snap back fast, then wobble home: fast in, slow out.
        hitSequence.Stop();
        transform.localRotation = restRotation;

        Quaternion tiltedBack = restRotation * Quaternion.Euler(hitTiltAngle, 0f, 0f);
        hitSequence = Sequence.Create()
            .Chain(Tween.LocalRotation(transform, tiltedBack, tiltDuration, Ease.OutQuad))
            .Chain(Tween.LocalRotation(transform, restRotation, settleDuration, Ease.OutElastic));
    }

    // Freeze the whole game for a split second so the brain registers the hit.
    void HitStop()
    {
        Time.timeScale = 0f;
        Tween.Delay(hitStopDuration, () => Time.timeScale = 1f, useUnscaledTime: true);
    }
}
