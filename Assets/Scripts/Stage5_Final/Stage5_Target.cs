using PrimeTween;
using UnityEngine;

public enum TargetZone { Outer, Middle, Bullseye }

// Put this on a pivot at the BOTTOM of the target face, so it tips backward like a hinge.
public class Stage5_Target : MonoBehaviour
{
    [SerializeField] float maxHitTiltAngle = 35f;
    [SerializeField] float tiltDuration = 0.08f;
    [SerializeField] float settleDuration = 0.8f;

    [Header("Zones")]
    [SerializeField] Transform faceCenter;
    [SerializeField] float bullseyeRadius = 0.3f;
    [SerializeField] float middleRadius = 0.86f;

    [Header("Feedback")]
    [SerializeField] Stage5_CameraFeedback cameraFeedback;
    [SerializeField] Stage5_GameUI gameUI;
    [SerializeField] ParticleSystem impactParticles;
    [SerializeField] ParticleSystem bullseyeConfetti;
    [SerializeField] int maxImpactParticles = 40;
    [SerializeField] AudioSource sfxSource;
    [SerializeField] AudioClip impactSound;
    [SerializeField] float maxHitStop = 0.1f;

    Quaternion restRotation;
    Sequence hitSequence;

    void Awake()
    {
        restRotation = transform.localRotation;
    }

    public void Hit(Vector3 hitPoint)
    {
        TargetZone zone = GetZone(hitPoint);

        // Feedback intensity should match the importance of the action.
        int points = 100;
        string label = "Nice";
        float intensity = 0.3f;

        if (zone == TargetZone.Middle)
        {
            points = 250;
            label = "Great!";
            intensity = 0.6f;
        }
        else if (zone == TargetZone.Bullseye)
        {
            points = 500;
            label = "PERFECT!";
            intensity = 1f;
        }

        // Every layer below scales with the same intensity value.
        PlayHitAnimation(intensity);

        impactParticles.transform.position = hitPoint;
        impactParticles.Emit(Mathf.RoundToInt(maxImpactParticles * intensity));

        PlaySound(impactSound, Mathf.Lerp(1.1f, 0.85f, intensity));
        cameraFeedback.Impact(intensity);
        HitStop(maxHitStop * intensity);

        if (zone == TargetZone.Bullseye)
            bullseyeConfetti.Play();

        gameUI.ShowHit(points, label, hitPoint, zone == TargetZone.Bullseye);
    }

    // Distance from the middle of the face, ignoring depth.
    TargetZone GetZone(Vector3 hitPoint)
    {
        Vector3 offset = hitPoint - faceCenter.position;
        offset.z = 0f;
        float distance = offset.magnitude;

        if (distance <= bullseyeRadius) return TargetZone.Bullseye;
        if (distance <= middleRadius) return TargetZone.Middle;
        return TargetZone.Outer;
    }

    void PlayHitAnimation(float intensity)
    {
        // Snap back fast, then wobble home: fast in, slow out.
        hitSequence.Stop();
        transform.localRotation = restRotation;

        Quaternion tiltedBack = restRotation * Quaternion.Euler(maxHitTiltAngle * intensity, 0f, 0f);
        hitSequence = Sequence.Create()
            .Chain(Tween.LocalRotation(transform, tiltedBack, tiltDuration, Ease.OutQuad))
            .Chain(Tween.LocalRotation(transform, restRotation, settleDuration, Ease.OutElastic));
    }

    // Freeze the whole game for a split second so the brain registers the hit.
    void HitStop(float duration)
    {
        Time.timeScale = 0f;
        Tween.Delay(duration, () => Time.timeScale = 1f, useUnscaledTime: true);
    }

    void PlaySound(AudioClip clip, float pitch)
    {
        if (clip == null)
            return;

        sfxSource.pitch = pitch * Random.Range(0.95f, 1.05f);
        sfxSource.PlayOneShot(clip);
    }
}
