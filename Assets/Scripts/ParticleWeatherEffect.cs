using UnityEngine;

/// <summary>
/// Puts a particle system (rain or snow) behind the IWeatherEffect switch.
/// Add this to the GameObject that holds the ParticleSystem.
/// Deactivate stops AND clears the particles, so old rain or snow disappears at once.
/// </summary>
public sealed class ParticleWeatherEffect : MonoBehaviour, IWeatherEffect
{
    ParticleSystem system;

    // Looked up lazily: Awake does not run while the object is inactive.
    ParticleSystem Particles
    {
        get
        {
            if (system == null) system = GetComponentInChildren<ParticleSystem>(true);
            return system;
        }
    }

    public void Activate()
    {
        gameObject.SetActive(true);

        ParticleSystem ps = Particles;
        if (ps == null)
        {
            Debug.LogWarning("ParticleWeatherEffect on '" + name + "' has no ParticleSystem.");
            return;
        }

        ps.Clear(true);
        ps.Play(true);
    }

    public void Deactivate()
    {
        ParticleSystem ps = Particles;
        if (ps != null && gameObject.activeInHierarchy)
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        gameObject.SetActive(false);
    }
}
