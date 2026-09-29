using PrimeTween;
using UnityEngine;

// Put this on a pivot at the BOTTOM of the target face, so it tips backward like a hinge.
public class Stage3_Target : MonoBehaviour
{
    [SerializeField] float hitTiltAngle = 25f;
    [SerializeField] float tiltDuration = 0.08f;
    [SerializeField] float settleDuration = 0.8f;

    Quaternion restRotation;
    Sequence hitSequence;
    int hitCount;

    void Awake()
    {
        restRotation = transform.localRotation;
    }

    public void Hit()
    {
        hitCount++;
        Debug.Log($"Target hit! Total hits: {hitCount}");

        // Snap back fast, then wobble home: fast in, slow out.
        hitSequence.Stop();
        transform.localRotation = restRotation;

        Quaternion tiltedBack = restRotation * Quaternion.Euler(hitTiltAngle, 0f, 0f);
        hitSequence = Sequence.Create()
            .Chain(Tween.LocalRotation(transform, tiltedBack, tiltDuration, Ease.OutQuad))
            .Chain(Tween.LocalRotation(transform, restRotation, settleDuration, Ease.OutElastic));
    }
}
