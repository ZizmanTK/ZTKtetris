using System.Collections.Generic;
using UnityEngine;

// Screen shake, particles and row flashes. Lives on the camera.
public class GameFeel : MonoBehaviour
{
    public ParticleSystem lineBurst;
    public ParticleSystem holeBurst;
    public Effects[] rowFlashes;
    public float maxShake = 0.8f;

    float trauma;
    Vector3 basePosition;

    void Awake()
    {
        basePosition = transform.localPosition;
    }

    public void Shake(float amount)
    {
        trauma = Mathf.Clamp01(trauma + amount);
    }

    public void LinesCleared(IList<float> rowYs, float centerX)
    {
        for (int i = 0; i < rowYs.Count; i++)
        {
            if (i < rowFlashes.Length) rowFlashes[i].PlayAtPosition(rowYs[i]);
            Emit(lineBurst, new Vector3(centerX, rowYs[i], 0f), 40);
        }
        Shake(0.2f + 0.15f * rowYs.Count);
    }

    public void HolesCreated(IList<Vector3> positions)
    {
        foreach (var p in positions)
            Emit(holeBurst, p, 6);
        Shake(Mathf.Min(0.1f + 0.03f * positions.Count, 0.4f));
    }

    public void HardDrop(int distance)
    {
        Shake(Mathf.Min(0.05f + distance * 0.008f, 0.3f));
    }

    public void GameOver()
    {
        Shake(1f);
    }

    void LateUpdate()
    {
        if (trauma <= 0f)
        {
            transform.localPosition = basePosition;
            return;
        }
        // Squaring trauma makes small shakes subtle and big ones punchy.
        float shake = maxShake * trauma * trauma;
        float t = Time.unscaledTime * 25f;
        var offset = new Vector3(Mathf.PerlinNoise(t, 0f) - 0.5f, Mathf.PerlinNoise(0f, t) - 0.5f, 0f) * 2f * shake;
        transform.localPosition = basePosition + offset;
        trauma = Mathf.Max(0f, trauma - Time.unscaledDeltaTime * 1.8f);
    }

    static void Emit(ParticleSystem system, Vector3 position, int count)
    {
        if (system == null) return;
        var emit = new ParticleSystem.EmitParams { position = position, applyShapeToPosition = true };
        system.Emit(emit, count);
    }
}
