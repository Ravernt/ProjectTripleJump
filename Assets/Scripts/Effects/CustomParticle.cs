using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CustomParticle : MonoBehaviour
{
    public string particleName;
    ParticleSystem[] particles;
    ParticleSystemRenderer[] renderers;

    ParticleSpawner.ParticlePool source;
    public bool Looping { get; private set; } = false;

    void Awake()
    {
        if (particles == null)
            Set();
    }

    void Set()
    {
        particles = GetComponentsInChildren<ParticleSystem>();

        renderers = new ParticleSystemRenderer[particles.Length];

        for (int i = 0; i < particles.Length; i++)
        {
            renderers[i] = particles[i].GetComponent<ParticleSystemRenderer>();

            if (particles[i].main.loop)
                Looping = true;
        }
    }

    public void SetSource(ParticleSpawner.ParticlePool source)
    {
        this.source = source;
    }

    public void Play(Vector2 position)
    {
        particles[0].transform.position = position;
        particles[0].Play(true);
    }

    public void Continue()
    {
        particles[0].Play(true);
    }

    public void Pause()
    {
        particles[0].Stop(true);
    }

    public void Release()
    {
        source.ManualRelease(this);
    }

    public float CalculateMaxWait()
    {
        Set();

        float maxWait = 0;
        foreach(ParticleSystem particle in particles)
        {
            if(particle.main.duration > maxWait)
                maxWait = particle.main.duration;
        }

        return maxWait;
    }
}
