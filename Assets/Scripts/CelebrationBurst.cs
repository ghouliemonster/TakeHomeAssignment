using UnityEngine;
using System.Collections;

// Attach to the sparkle burst sprite prefab. Handles its own grow-in/fade-out
// so it reads as a little "pop" of delight rather than a static image that
// just appears and vanishes. Self-destructs when done — GameManager doesn't
// need to manage its lifetime directly.
[RequireComponent(typeof(SpriteRenderer))]
public class CelebrationBurst : MonoBehaviour, ICelebrationEffect
{
    [Header("Timing")]
    public float growDuration = 0.25f;
    public float holdDuration = 1.0f;
    public float fadeOutDuration = 0.4f;

    [Header("Scale")]
    public float startScale = 0.4f;
    public float overshootScale = 1.15f;
    public float restScale = 1.0f;

    private SpriteRenderer _renderer;

    // Optional: called right after Instantiate if you want to sync its total
    // lifetime to GameManager's celebrationDuration instead of the defaults above.
    public void SetLifetime(float totalDuration)
    {
        float minimum = growDuration + fadeOutDuration;
        if (totalDuration <= minimum)
        {
            holdDuration = 0f;
            return;
        }
        holdDuration = totalDuration - minimum;
    }

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        transform.localScale = Vector3.one * startScale;
        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        // Soft pop: overshoot slightly past rest scale, then settle — reads as
        // a little bounce of delight without being fast or jarring.
        yield return ScaleTo(overshootScale, growDuration * 0.7f);
        yield return ScaleTo(restScale, growDuration * 0.3f);

        yield return new WaitForSeconds(holdDuration);

        yield return FadeOut(fadeOutDuration);

        Destroy(gameObject);
    }

    private IEnumerator ScaleTo(float target, float duration)
    {
        float start = transform.localScale.x;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float eased = 1f - Mathf.Pow(1f - Mathf.Clamp01(t / duration), 2f);
            float scale = Mathf.Lerp(start, target, eased);
            transform.localScale = Vector3.one * scale;
            yield return null;
        }
        transform.localScale = Vector3.one * target;
    }

    private IEnumerator FadeOut(float duration)
    {
        Color start = _renderer.color;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(start.a, 0f, t / duration);
            _renderer.color = new Color(start.r, start.g, start.b, a);
            yield return null;
        }
    }
}