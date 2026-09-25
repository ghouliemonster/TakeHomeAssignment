using UnityEngine;
using System;

// Drop this on the firefly prefab, alongside a SpriteRenderer and a
// CircleCollider2D sized generously (big touch target for small hands).
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
public class Firefly : MonoBehaviour
{
    [Header("Sprites (assign the 3 color-variant PNGs)")]
    public Sprite yellowSprite;
    public Sprite pinkSprite;
    public Sprite greenSprite;

    [Header("Color weighting (out of 100, does not need to sum exactly)")]
    [Range(0, 100)] public int yellowWeight = 60;
    [Range(0, 100)] public int pinkWeight = 20;
    [Range(0, 100)] public int greenWeight = 20;

    [Header("Wander drift")]
    public float wanderSpeed = 0.6f;      // how fast the sine drift oscillates
    public float wanderAmplitude = 1.2f;  // how far it drifts per axis
    public float driftSpeed = 0.5f;       // slow overall glide across the screen

    [Header("Catch behaviour")]
    public float catchFlightDuration = 0.6f;
    public AudioClip catchBlip; // optional soft "pop" when tapped, distinct from the celebration chime

    public FireflyColorType ColorType { get; private set; }

    private SpriteRenderer _renderer;
    private CircleCollider2D _collider;
    private Vector2 _driftDirection;
    private float _phaseX;
    private float _phaseY;
    private bool _caught;
    private Vector3 _spawnCenter;
    private AudioSource _audioSource;

    public event Action<Firefly> OnCaught;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<CircleCollider2D>();
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null && catchBlip != null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }
    }

    private void Start()
    {
        // Random phase offsets so fireflies don't all wander in sync.
        _phaseX = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
        _phaseY = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
        _driftDirection = UnityEngine.Random.insideUnitCircle.normalized;
        _spawnCenter = transform.position;

        AssignRandomColor();
    }

    private void Update()
    {
        if (_caught) return;

        // Slow overall glide, plus a soft sine wobble on top — calm, floaty,
        // never sharp or fast. This is the "no frantic motion" rule from the brief.
        _spawnCenter += (Vector3)(_driftDirection * driftSpeed * Time.deltaTime);

        float offsetX = Mathf.Sin(Time.time * wanderSpeed + _phaseX) * wanderAmplitude;
        float offsetY = Mathf.Cos(Time.time * wanderSpeed * 0.8f + _phaseY) * wanderAmplitude;

        transform.position = _spawnCenter + new Vector3(offsetX, offsetY, 0f);

        // Occasionally nudge the drift direction so it wanders, not travels in a straight line.
        if (UnityEngine.Random.value < 0.002f)
        {
            _driftDirection = Vector2.Lerp(_driftDirection, UnityEngine.Random.insideUnitCircle.normalized, 0.3f);
        }
    }

    private void AssignRandomColor()
    {
        int total = Mathf.Max(1, yellowWeight + pinkWeight + greenWeight);
        int roll = UnityEngine.Random.Range(0, total);

        if (roll < yellowWeight)
        {
            ColorType = FireflyColorType.Yellow;
            _renderer.sprite = yellowSprite;
        }
        else if (roll < yellowWeight + pinkWeight)
        {
            ColorType = FireflyColorType.Pink;
            _renderer.sprite = pinkSprite;
        }
        else
        {
            ColorType = FireflyColorType.Green;
            _renderer.sprite = greenSprite;
        }
    }

    // Hooked to bounds by the spawner so fireflies stay on-screen.
    public void ClampToBounds(Rect bounds)
    {
        Vector3 clamped = _spawnCenter;
        clamped.x = Mathf.Clamp(clamped.x, bounds.xMin, bounds.xMax);
        clamped.y = Mathf.Clamp(clamped.y, bounds.yMin, bounds.yMax);
        _spawnCenter = clamped;
    }

    // Mouse click AND touch tap both land here by default in Unity
    // (touch is translated to mouse events unless Input.simulateMouseWithTouches
    // is turned off — leave it on for this project).
    private void OnMouseDown()
    {
        if (_caught) return;
        Catch();
    }

    private void Catch()
    {
        _caught = true;
        _collider.enabled = false;

        if (catchBlip != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(catchBlip);
        }

        OnCaught?.Invoke(this);
    }

    // Called by GameManager once it knows where the jar is.
    public void FlyToJarAndDestroy(Vector3 jarPosition, Action onArrived)
    {
        StartCoroutine(FlyRoutine(jarPosition, onArrived));
    }

    private System.Collections.IEnumerator FlyRoutine(Vector3 target, Action onArrived)
    {
        Vector3 start = transform.position;
        float t = 0f;
        Vector3 startScale = transform.localScale;

        while (t < 1f)
        {
            t += Time.deltaTime / catchFlightDuration;
            // Ease-out so the motion is gentle, not a hard snap.
            float eased = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f);
            transform.position = Vector3.Lerp(start, target, eased);
            transform.localScale = Vector3.Lerp(startScale, startScale * 0.2f, eased);
            yield return null;
        }

        onArrived?.Invoke();
        Destroy(gameObject);
    }
}