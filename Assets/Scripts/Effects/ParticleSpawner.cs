using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ParticleSpawner : MonoBehaviour
{
    public class ParticlePool
    {
        CustomParticle particle;
        ObjectPool<CustomParticle> pool;

        float maxWait = 0;

        public ParticlePool(CustomParticle particle)
        {
            this.particle = particle;
            pool = new(Create, (particle) => particle.gameObject.SetActive(true), (particle) => particle.gameObject.SetActive(false));
        }

        CustomParticle Create()
        {
            CustomParticle newParticle = Instantiate(particle);
            newParticle.SetSource(this);

            if (maxWait == 0)
                maxWait = newParticle.CalculateMaxWait();

            return newParticle;
        }

        public CustomParticle SpawnParticle(Vector2 position)
        {
            var particle = pool.Get();
            particle.Play(position);

            if(!particle.Looping)
                Instance.StartCoroutine(Release(particle));
            return particle;
        }

        IEnumerator Release(CustomParticle particle)
        {
            yield return new WaitForSeconds(maxWait);
            pool.Release(particle);
        }

        public void ManualRelease(CustomParticle particle)
        {
            pool.Release(particle);
        }
    }

    static ParticleSpawner instance;
    public static ParticleSpawner Instance
    {
        get
        {
            if (instance == null)
                throw new System.Exception("Couldn't find 'manager'. Make sure that manager (can be found in '/Assets' folder) is in the scene!");

            return instance;
        }
    }
    [SerializeField] CustomParticle[] allParticles;
    Dictionary<string, ParticlePool> particles = new();

    void Start()
    {
        instance = this;


        foreach (CustomParticle particle in allParticles)
        {
            particles.Add(particle.particleName, new(particle));
        }
    }

    public CustomParticle SpawnParticle(string name, Vector2 position)
    {
        return particles[name].SpawnParticle(position);
    }
}
