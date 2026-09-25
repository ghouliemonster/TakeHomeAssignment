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
        if (_celebrating) return; // let the celebration play out before counting more

        FireflyColorType color = firefly.ColorType;

        firefly.FlyToJarAndDestroy(jarPosition.position, () =>
        {
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
            Destroy(burst, celebrationDuration);
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