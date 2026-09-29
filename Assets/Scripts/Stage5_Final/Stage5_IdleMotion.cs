using PrimeTween;
using UnityEngine;

// Small looping motion for props (flag, star, prizes) so the world never looks frozen.
public class Stage5_IdleMotion : MonoBehaviour
{
    [SerializeField] Vector3 swingAngles = new Vector3(0f, 0f, 5f);
    [SerializeField] Vector3 bobOffset = Vector3.zero;
    [SerializeField] float duration = 1.5f;

    void Start()
    {
        // Random delay so props don't all move in perfect sync.
        float delay = Random.Range(0f, duration);

        // cycles: -1 = forever. Yoyo = go there, then come back.
        if (swingAngles != Vector3.zero)
        {
            Quaternion rest = transform.localRotation;
            Tween.LocalRotation(transform, rest * Quaternion.Euler(-swingAngles), rest * Quaternion.Euler(swingAngles),
                duration, Ease.InOutSine, cycles: -1, cycleMode: CycleMode.Yoyo, startDelay: delay);
        }

        if (bobOffset != Vector3.zero)
        {
            Vector3 rest = transform.localPosition;
            Tween.LocalPosition(transform, rest, rest + bobOffset,
                duration, Ease.InOutSine, cycles: -1, cycleMode: CycleMode.Yoyo, startDelay: delay);
        }
    }
}
