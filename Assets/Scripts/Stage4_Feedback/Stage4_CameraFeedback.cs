using PrimeTween;
using UnityEngine;

// Put this on the CameraRig (the camera's parent).
// The rig does the pull/recoil movement, the camera child does the shaking, so the two never fight.
public class Stage4_CameraFeedback : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] float pullBackDistance = 0.4f;
    [SerializeField] float releaseFovKick = 4f;
    [SerializeField] float impactShakeStrength = 1.5f;

    Vector3 restPosition;
    float baseFov;
    Tween recoilTween;
    Sequence fovSequence;

    void Awake()
    {
        restPosition = transform.localPosition;
        baseFov = cam.fieldOfView;
    }

    // The camera leans back with the player as they pull.
    public void Pull(float curvedPull)
    {
        recoilTween.Stop();
        transform.localPosition = restPosition + Vector3.back * pullBackDistance * curvedPull;
    }

    public void Release(float pullAmount)
    {
        // OutBack = spring forward past the rest position, then settle. That's the recoil.
        recoilTween = Tween.LocalPosition(transform, restPosition, 0.4f, Ease.OutBack);

        // Quick FOV punch out, slower return: sells the burst of speed.
        fovSequence.Stop();
        fovSequence = Sequence.Create()
            .Chain(Tween.CameraFieldOfView(cam, baseFov + releaseFovKick * pullAmount, 0.07f, Ease.OutQuad))
            .Chain(Tween.CameraFieldOfView(cam, baseFov, 0.35f, Ease.InOutSine));
    }

    public void Impact()
    {
        Tween.ShakeCamera(cam, impactShakeStrength, 0.4f);
    }
}
