using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Lives on a Screen Space - Overlay canvas.
public class Stage5_GameUI : MonoBehaviour
{
    [SerializeField] Camera cam;

    [Header("Score")]
    [SerializeField] TMP_Text scoreText;

    [Header("Popup")]
    [SerializeField] RectTransform popup;
    [SerializeField] CanvasGroup popupGroup;
    [SerializeField] TMP_Text popupText;
    [SerializeField] float popupRise = 120f;

    [Header("Pull Meter")]
    [SerializeField] CanvasGroup pullMeterGroup;
    [SerializeField] Image pullMeterFill;
    [SerializeField] Gradient pullMeterColors;

    [Header("Perfect Flash")]
    [SerializeField] CanvasGroup flash;

    [Header("Sound")]
    [SerializeField] AudioSource sfxSource;
    [SerializeField] AudioClip popSound;
    [SerializeField] AudioClip perfectSound;

    int score;
    int displayedScore;
    Tween scoreCountTween;
    Sequence popupSequence;
    Tween pullMeterTween;

    void Start()
    {
        scoreText.text = "0";
        popupGroup.alpha = 0f;
        pullMeterGroup.alpha = 0f;
        flash.alpha = 0f;
    }

    public void ShowPullMeter()
    {
        pullMeterTween.Stop();
        pullMeterTween = Tween.Alpha(pullMeterGroup, 1f, 0.15f);
    }

    public void HidePullMeter()
    {
        pullMeterTween.Stop();
        pullMeterTween = Tween.Alpha(pullMeterGroup, pullMeterGroup.alpha, 0f, 0.3f, startDelay: 0.2f);
    }

    public void SetPullMeter(float pullAmount)
    {
        pullMeterFill.fillAmount = pullAmount;
        pullMeterFill.color = pullMeterColors.Evaluate(pullAmount);
    }

    public void ShowHit(int points, string label, Vector3 worldPosition, bool isPerfect)
    {
        AddScore(points);
        PlayPopup($"{label}\n+{points}", worldPosition);

        if (isPerfect)
        {
            Tween.Alpha(flash, 0.35f, 0f, 0.3f, Ease.OutQuad);
            PlaySound(perfectSound);
        }
        else
        {
            PlaySound(popSound);
        }
    }

    void AddScore(int points)
    {
        score += points;

        // Count up instead of jumping, and punch the number so the eye notices it changed.
        scoreCountTween.Stop();
        scoreCountTween = Tween.Custom(displayedScore, score, 0.5f, value =>
        {
            displayedScore = Mathf.RoundToInt(value);
            scoreText.text = displayedScore.ToString();
        }, Ease.OutQuad);

        Tween.StopAll(scoreText.transform);
        scoreText.transform.localScale = Vector3.one;
        Tween.PunchScale(scoreText.transform, Vector3.one * 0.3f, 0.35f);
    }

    // scale 0 -> overshoot -> settle -> float up + fade out.
    void PlayPopup(string message, Vector3 worldPosition)
    {
        popupSequence.Stop();

        Vector3 screenPosition = cam.WorldToScreenPoint(worldPosition);
        popup.position = screenPosition;
        popup.localScale = Vector3.zero;
        popupGroup.alpha = 1f;
        popupText.text = message;

        popupSequence = Sequence.Create()
            .Chain(Tween.Scale(popup, 1f, 0.35f, Ease.OutBack))
            .ChainDelay(0.35f)
            .Chain(Tween.Position(popup, screenPosition + Vector3.up * popupRise, 0.6f, Ease.OutQuad))
            .Group(Tween.Alpha(popupGroup, 0f, 0.6f, Ease.InQuad));
    }

    void PlaySound(AudioClip clip)
    {
        if (clip == null)
            return;

        sfxSource.pitch = Random.Range(0.95f, 1.05f);
        sfxSource.PlayOneShot(clip);
    }
}
