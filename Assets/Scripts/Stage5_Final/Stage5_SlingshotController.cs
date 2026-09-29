using PrimeTween;
using UnityEngine;

public class Stage5_SlingshotController : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] Stage5_Projectile projectile;

    [SerializeField] float maxPullDistance = 1.5f;
    [SerializeField] float maxLaunchForce = 20f;
    [SerializeField] float dragSensitivity = 6f;
    [SerializeField] float pullDownAmount = 0.4f;
    [SerializeField] float minPullToLaunch = 0.1f;

    [Header("Slingshot Parts")]
    [SerializeField] Transform pouch;
    [SerializeField] Transform frame;
    [SerializeField] Transform bandLeftPivot;
    [SerializeField] Transform bandRightPivot;
    [SerializeField] Transform bandLeftAttach;
    [SerializeField] Transform bandRightAttach;

    [Header("Tension")]
    [SerializeField] AnimationCurve tensionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] float maxFrameBend = 4f;
    [SerializeField] float maxProjectileSquash = 0.3f;
    [SerializeField] float maxTensionShake = 0.03f;

    [Header("Feedback")]
    [SerializeField] Stage5_CameraFeedback cameraFeedback;
    [SerializeField] ParticleSystem releaseBurst;
    [SerializeField] AudioSource sfxSource;
    [SerializeField] AudioSource tensionSource;
    [SerializeField] AudioClip releaseSound;
    [SerializeField] float maxTensionVolume = 0.6f;

    [Header("Final Polish")]
    [SerializeField] Stage5_GameUI gameUI;
    [SerializeField] LineRenderer trajectoryLine;
    [SerializeField] float trajectoryTimeStep = 0.04f;

    Vector3 restPosition;
    Vector3 dragStartMouse;
    Vector3 pullOffset;
    float pullAmount;
    bool isDragging;

    Vector3 pouchOffset;
    Vector3 pouchRestPosition;
    Quaternion frameRestRotation;
    Vector3 projectileRestScale;
    float bandLeftRestLength;
    float bandRightRestLength;

    // Handles to running animations, so we can cancel them if the player grabs again early.
    Tween pouchTween;
    Sequence frameSequence;

    void Start()
    {
        restPosition = projectile.transform.position;
        projectileRestScale = projectile.transform.localScale;

        pouchRestPosition = pouch.position;
        pouchOffset = pouch.position - projectile.transform.position;
        frameRestRotation = frame.localRotation;

        bandLeftRestLength = Vector3.Distance(bandLeftPivot.position, bandLeftAttach.position);
        bandRightRestLength = Vector3.Distance(bandRightPivot.position, bandRightAttach.position);

        trajectoryLine.enabled = false;
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

    // Bands always follow the pouch, including while the release tween is moving it.
    void LateUpdate()
    {
        StretchBand(bandLeftPivot, bandLeftAttach, bandLeftRestLength);
        StretchBand(bandRightPivot, bandRightAttach, bandRightRestLength);
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

        // Stop any release animation that might still be playing.
        pouchTween.Stop();
        frameSequence.Stop();
        Tween.StopAll(projectile.transform);

        tensionSource.volume = 0f;
        tensionSource.Play();

        gameUI.ShowPullMeter();
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

        ShowTension();
        ShowTrajectory();
        gameUI.SetPullMeter(pullAmount);
    }

    void ShowTension()
    {
        // Instead of using the raw 0-1 value, we pass it through a curve.
        float curvedPull = tensionCurve.Evaluate(pullAmount);

        // Frame leans toward the player as the bands pull on it.
        frame.localRotation = frameRestRotation * Quaternion.Euler(-curvedPull * maxFrameBend, 0f, 0f);

        // Projectile squashes along the pull direction (anticipation).
        float squash = curvedPull * maxProjectileSquash;
        projectile.transform.localScale = Vector3.Scale(projectileRestScale, new Vector3(1f + squash * 0.5f, 1f + squash * 0.5f, 1f - squash));

        // At full tension, a tiny tremble says "this is the limit".
        if (pullAmount > 0.98f)
            projectile.transform.position += Random.insideUnitSphere * maxTensionShake;

        pouch.position = projectile.transform.position + pouchOffset;

        // The same curved value now also drives sound and camera.
        tensionSource.volume = curvedPull * maxTensionVolume;
        tensionSource.pitch = Mathf.Lerp(0.8f, 1.3f, curvedPull);
        cameraFeedback.Pull(curvedPull);
    }

    // Preview the first part of the flight so the player can aim, without giving the whole answer away.
    void ShowTrajectory()
    {
        trajectoryLine.enabled = pullAmount >= minPullToLaunch;

        // ForceMode.Impulse means: starting velocity = force / mass.
        Vector3 velocity = CalculateLaunchForce() / projectile.Mass;
        Vector3 start = projectile.transform.position;

        for (int i = 0; i < trajectoryLine.positionCount; i++)
        {
            float t = i * trajectoryTimeStep;
            trajectoryLine.SetPosition(i, start + velocity * t + 0.5f * t * t * Physics.gravity);
        }
    }

    // Launch in the opposite direction of the pull. More pull = more force.
    Vector3 CalculateLaunchForce()
    {
        return -pullOffset.normalized * pullAmount * maxLaunchForce;
    }

    void StretchBand(Transform bandPivot, Transform attachPoint, float restLength)
    {
        Vector3 toAttach = attachPoint.position - bandPivot.position;
        bandPivot.rotation = Quaternion.LookRotation(toAttach);
        bandPivot.localScale = new Vector3(1f, 1f, toAttach.magnitude / restLength);
    }

    void Release()
    {
        isDragging = false;
        tensionSource.Stop();
        trajectoryLine.enabled = false;
        gameUI.HidePullMeter();

        if (pullAmount < minPullToLaunch)
        {
            projectile.transform.position = restPosition;
            projectile.transform.localScale = projectileRestScale;
            pouch.position = pouchRestPosition;
            frame.localRotation = frameRestRotation;
            cameraFeedback.Pull(0f);
            return;
        }

        projectile.Launch(CalculateLaunchForce());

        PlayReleaseAnimation();
        PlayReleaseFeedback();
    }

    void PlayReleaseAnimation()
    {
        // OutElastic = the pouch snaps past its rest point, then wobbles back (overshoot).
        pouchTween = Tween.Position(pouch, pouchRestPosition, 0.45f, Ease.OutElastic);

        // Frame whips forward past rest, then settles.
        Quaternion whipForward = frameRestRotation * Quaternion.Euler(maxFrameBend * 0.6f, 0f, 0f);
        frameSequence = Sequence.Create()
            .Chain(Tween.LocalRotation(frame, whipForward, 0.06f, Ease.OutQuad))
            .Chain(Tween.LocalRotation(frame, frameRestRotation, 0.5f, Ease.OutElastic));
    }

    void PlayReleaseFeedback()
    {
        releaseBurst.transform.position = pouchRestPosition;
        releaseBurst.Play();

        cameraFeedback.Release(pullAmount);

        // Stronger pulls sound a little lower and heavier.
        PlaySound(releaseSound, Mathf.Lerp(1.15f, 0.9f, pullAmount));
    }

    void PlaySound(AudioClip clip, float pitch)
    {
        if (clip == null)
            return;

        sfxSource.pitch = pitch * Random.Range(0.95f, 1.05f);
        sfxSource.PlayOneShot(clip);
    }
}
