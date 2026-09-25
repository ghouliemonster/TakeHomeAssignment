using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public Transform jarPosition;
    public JarController jarController;
    public AnimalReactor[] animalReactors; // frog, cricket, and later the third animal

    [Header("Celebration")]
    public int fireflyGoal = 10;
    public GameObject celebrationSparkleBurstPrefab; // the sparkle burst asset
    public AudioClip celebrationSound;
    public float celebrationDuration = 2.0f;

    private AudioSource _audioSource;
    private int _caughtCount;
    private bool _celebrating;

    private void Awake()
    {
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
    }

    // Called by FireflySpawner when a firefly is tapped.
    public void OnFireflyCaught(Firefly firefly)
    {
        FireflyColorType color = firefly.ColorType;

        // Always send the firefly on its way and destroy it — a tap should
        // never leave a firefly stuck on screen. If a celebration is already
        // playing (e.g. it was tapped in the couple seconds before reset),
        // it still flies into the jar and disappears, it just doesn't count
        // toward the next round or wake up the animals.
        firefly.FlyToJarAndDestroy(jarPosition.position, () =>
        {
            if (_celebrating) return;

            _caughtCount++;
            jarController.SetFillLevel(_caughtCount, fireflyGoal);

            foreach (var reactor in animalReactors)
            {
                reactor.NotifyFireflyCaught(color);
            }

            if (_caughtCount >= fireflyGoal)
            {
                StartCoroutine(CelebrateAndReset());
            }
        });
    }

    private IEnumerator Celebrate()
    {
        _celebrating = true;

        if (celebrationSparkleBurstPrefab != null)
        {
            GameObject burst = Instantiate(celebrationSparkleBurstPrefab, jarPosition.position, Quaternion.identity);

            CelebrationBurst burstAnim = burst.GetComponent<CelebrationBurst>();
            if (burstAnim != null)
            {
                // Let it play its own grow/hold/fade sequence, timed to fit
                // within the celebration window, and self-destruct.
                burstAnim.SetLifetime(celebrationDuration);
            }
            else
            {
                // No animation script attached — just a static sprite, so
                // hard-destroy it after the celebration window instead.
                Destroy(burst, celebrationDuration);
            }
        }

        if (celebrationSound != null)
        {
            _audioSource.PlayOneShot(celebrationSound);
        }

        yield return new WaitForSeconds(celebrationDuration);
    }

    private IEnumerator CelebrateAndReset()
    {
        yield return Celebrate();

        jarController.ResetJar();
        _caughtCount = 0;
        _celebrating = false;
    }
}