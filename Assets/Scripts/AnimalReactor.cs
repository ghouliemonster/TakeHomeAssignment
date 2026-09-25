using UnityEngine;
using System.Collections;

// Generic "reacts to one firefly color" component. Put this on the frog
// (targetColor = Pink or Yellow, your call) and the cricket (the other),
// and it's the whole third-animal mechanic too if there's time — just add
// another GameObject with this script set to Green.
[RequireComponent(typeof(AudioSource))]
public class AnimalReactor : MonoBehaviour
{
    [Header("Which firefly color wakes this animal up")]
    public FireflyColorType targetColor;

    [Header("Reaction")]
    public AudioClip reactionSound; // ribbit / chirp
    public Animator animator;       // optional — leave null if using the scale-pulse fallback
    public string reactionTrigger = "React";

    [Header("Fallback animation (used if no Animator is wired up)")]
    public float pulseScale = 1.25f;
    public float pulseDuration = 0.35f;

    private AudioSource _audioSource;
    private Vector3 _baseScale;
    private Coroutine _pulseRoutine;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _baseScale = transform.localScale;
    }

    // GameManager calls this on every reactor whenever a firefly is caught;
    // each reactor decides for itself whether the color matches.
    public void NotifyFireflyCaught(FireflyColorType caughtColor)
    {
        if (caughtColor != targetColor) return;
        React();
    }

    private void React()
    {
        if (reactionSound != null)
        {
            _audioSource.PlayOneShot(reactionSound);
        }

        if (animator != null)
        {
            animator.SetTrigger(reactionTrigger);
        }
        else
        {
            if (_pulseRoutine != null) StopCoroutine(_pulseRoutine);
            _pulseRoutine = StartCoroutine(ScalePulse());
        }
    }

    private IEnumerator ScalePulse()
    {
        float t = 0f;
        Vector3 up = _baseScale * pulseScale;

        while (t < 1f)
        {
            t += Time.deltaTime / (pulseDuration * 0.5f);
            transform.localScale = Vector3.Lerp(_baseScale, up, Mathf.Sin(Mathf.Clamp01(t) * Mathf.PI * 0.5f));
            yield return null;
        }

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / (pulseDuration * 0.5f);
            transform.localScale = Vector3.Lerp(up, _baseScale, t);
            yield return null;
        }

        transform.localScale = _baseScale;
    }
}