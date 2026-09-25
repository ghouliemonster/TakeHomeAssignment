using UnityEngine;
using System.Collections;

// Attach to the jar GameObject. Expects two child SpriteRenderers:
// - jarOutline: the line-art jar PNG you were given (static, always visible)
// - glowFill: a soft blob/glyph sprite (use the Firefly Glow #FFD166 or
//   Highlights #FFF4E0 color) sized to sit inside the jar's body, alpha 0 at start.
// No fill-state sprites were provided, so the "filling up" look is done with
// this glow's alpha + scale rather than swapping sprites.
public class JarController : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer glowFill;
    public float fillTweenDuration = 0.35f;
    public float maxGlowAlpha = 0.9f;
    public Vector3 minGlowScale = new Vector3(0.3f, 0.3f, 1f);
    public Vector3 maxGlowScale = new Vector3(1f, 1f, 1f);

    private Coroutine _tweenRoutine;

    private void Awake()
    {
        if (glowFill != null)
        {
            SetGlowInstant(0f);
        }
    }

    public void SetFillLevel(int caught, int max)
    {
        float t = Mathf.Clamp01((float)caught / max);
        if (_tweenRoutine != null) StopCoroutine(_tweenRoutine);
        _tweenRoutine = StartCoroutine(TweenGlow(t));
    }

    private IEnumerator TweenGlow(float targetT)
    {
        Color startColor = glowFill.color;
        float startAlpha = startColor.a;
        Vector3 startScale = glowFill.transform.localScale;

        float targetAlpha = targetT * maxGlowAlpha;
        Vector3 targetScale = Vector3.Lerp(minGlowScale, maxGlowScale, targetT);

        float elapsed = 0f;
        while (elapsed < fillTweenDuration)
        {
            elapsed += Time.deltaTime;
            float p = elapsed / fillTweenDuration;
            float a = Mathf.Lerp(startAlpha, targetAlpha, p);
            glowFill.color = new Color(startColor.r, startColor.g, startColor.b, a);
            glowFill.transform.localScale = Vector3.Lerp(startScale, targetScale, p);
            yield return null;
        }

        glowFill.color = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);
        glowFill.transform.localScale = targetScale;
    }

    private void SetGlowInstant(float t)
    {
        Color c = glowFill.color;
        glowFill.color = new Color(c.r, c.g, c.b, t * maxGlowAlpha);
        glowFill.transform.localScale = Vector3.Lerp(minGlowScale, maxGlowScale, t);
    }

    // Called by GameManager once the celebration finishes.
    public void ResetJar()
    {
        SetGlowInstant(0f);
    }
}