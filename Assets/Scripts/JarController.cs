using UnityEngine;
using System.Collections;

// Attach to the jar GameObject, with two child SpriteRenderers stacked on
// top of each other (same position/scale): rendererA and rendererB, assigned
// in the inspector. They crossfade into each other so stage changes are a
// soft blend, not a hard cut — keeps it in line with the "calm, never
// abrupt" rule.
//
// Maps your 4 fill-stage sprites (empty / half-full / glowing full / glowing
// full with sparkles) across the 10-catch goal:
//   0 catches      -> Empty
//   1-4 catches    -> HalfFull
//   5-9 catches    -> GlowingFull
//   10 catches     -> GlowingFullWithSparkles (celebration state)
public class JarController : MonoBehaviour
{
    [Header("Fill-stage sprites (from the reference sheet, redrawn at full res)")]
    public Sprite emptySprite;
    public Sprite halfFullSprite;
    public Sprite glowingFullSprite;
    public Sprite glowingFullWithSparklesSprite;

    [Header("Crossfade")]
    public SpriteRenderer rendererA;
    public SpriteRenderer rendererB;
    public float crossfadeDuration = 0.5f;

    private SpriteRenderer _front;
    private SpriteRenderer _back;
    private Coroutine _fadeRoutine;
    private Sprite _currentSprite;

    private void Awake()
    {
        _front = rendererA;
        _back = rendererB;

        _currentSprite = emptySprite;
        _front.sprite = _currentSprite;
        _front.color = new Color(1f, 1f, 1f, 1f);
        _back.color = new Color(1f, 1f, 1f, 0f);
    }

    public void SetFillLevel(int caught, int max)
    {
        Sprite target = GetSpriteForCount(caught, max);
        if (target == _currentSprite) return;

        _currentSprite = target;
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(Crossfade(target));
    }

    private Sprite GetSpriteForCount(int caught, int max)
    {
        if (caught <= 0) return emptySprite;
        if (caught >= max) return glowingFullWithSparklesSprite;

        float t = (float)caught / max;
        if (t < 0.5f) return halfFullSprite;
        return glowingFullSprite;
    }

    private IEnumerator Crossfade(Sprite target)
    {
        _back.sprite = target;
        _back.color = new Color(1f, 1f, 1f, 0f);

        float elapsed = 0f;
        while (elapsed < crossfadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / crossfadeDuration;
            _front.color = new Color(1f, 1f, 1f, 1f - t);
            _back.color = new Color(1f, 1f, 1f, t);
            yield return null;
        }

        // Swap roles so _front is always the fully-opaque, currently-shown sprite.
        var temp = _front;
        _front = _back;
        _back = temp;

        _front.color = new Color(1f, 1f, 1f, 1f);
        _back.color = new Color(1f, 1f, 1f, 0f);
    }

    // Called by GameManager once the celebration finishes.
    public void ResetJar()
    {
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _currentSprite = emptySprite;
        _front.sprite = emptySprite;
        _front.color = new Color(1f, 1f, 1f, 1f);
        _back.color = new Color(1f, 1f, 1f, 0f);
    }
}