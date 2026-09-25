using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FireflySpawner : MonoBehaviour
{
    [Header("Setup")]
    public Firefly fireflyPrefab;
    public GameManager gameManager;

    [Header("Pacing (calm, not a swarm)")]
    public int maxOnScreen = 6;
    public float minSpawnInterval = 1.2f;
    public float maxSpawnInterval = 2.5f;

    [Header("Spawn area (world units, inset from screen edges)")]
    public float edgeInset = 1.0f;
    [Tooltip("Keep the bottom clear so the jar and any animal sprites aren't crowded.")]
    public float bottomInset = 2.5f;

    private readonly List<Firefly> _active = new List<Firefly>();
    private Rect _bounds;

    private void Start()
    {
        CalculateBounds();
        StartCoroutine(SpawnLoop());
    }

    private void CalculateBounds()
    {
        Camera cam = Camera.main;
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        _bounds = new Rect(
            cam.transform.position.x - halfWidth + edgeInset,
            cam.transform.position.y - halfHeight + bottomInset,
            (halfWidth * 2f) - (edgeInset * 2f),
            (halfHeight * 2f) - edgeInset - bottomInset
        );
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            _active.RemoveAll(f => f == null);

            if (_active.Count < maxOnScreen)
            {
                SpawnOne();
            }

            yield return new WaitForSeconds(Random.Range(minSpawnInterval, maxSpawnInterval));
        }
    }

    private void SpawnOne()
    {
        Vector3 spawnPos = new Vector3(
            Random.Range(_bounds.xMin, _bounds.xMax),
            Random.Range(_bounds.yMin, _bounds.yMax),
            0f
        );

        Firefly firefly = Instantiate(fireflyPrefab, spawnPos, Quaternion.identity);
        firefly.ClampToBounds(_bounds);
        firefly.OnCaught += HandleCaught;
        _active.Add(firefly);
    }

    private void HandleCaught(Firefly firefly)
    {
        firefly.OnCaught -= HandleCaught;
        _active.Remove(firefly);
        gameManager.OnFireflyCaught(firefly);
    }
}