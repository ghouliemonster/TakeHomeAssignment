using UnityEngine;

// Attach to the firework explosion prefab alongside an Animator whose
// controller has a single non-looping "Explode" clip built from the 5x5
// sprite sheet. Plays once at its own natural frame rate — it deliberately
// ignores SetLifetime(), since stretching or squashing a frame-by-frame
// explosion to fit an arbitrary duration would just make it look wrong.
[RequireComponent(typeof(Animator))]
public class FireworkExplosion : MonoBehaviour, ICelebrationEffect
{
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        Destroy(gameObject, GetClipLength());
    }

    private float GetClipLength()
    {
        if (_animator == null || _animator.runtimeAnimatorController == null)
        {
            return 1f; // safe fallback if the controller isn't wired up yet
        }

        var clips = _animator.runtimeAnimatorController.animationClips;
        return clips.Length > 0 ? clips[0].length : 1f;
    }

    // Part of ICelebrationEffect. No-op on purpose — see class comment.
    public void SetLifetime(float totalDuration) { }
}