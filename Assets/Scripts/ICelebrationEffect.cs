// Lets GameManager treat any celebration effect prefab the same way,
// regardless of whether it's a procedurally-animated static sprite
// (CelebrationBurst) or an Animator-driven sprite-sheet explosion
// (FireworkExplosion). Implement this on whatever effect script you attach
// to the prefab, and GameManager will find it automatically.
public interface ICelebrationEffect
{
    // Called once, right after Instantiate, with GameManager's overall
    // celebrationDuration. Effects that want to size themselves to fit that
    // window can use it; effects that play at a fixed natural speed (like a
    // frame-by-frame explosion) can just ignore it.
    void SetLifetime(float totalDuration);
}