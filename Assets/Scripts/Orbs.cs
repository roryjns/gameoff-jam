using UnityEngine;
using System.Collections.Generic;

public class Orbs : MonoBehaviour
{
    ParticleSystem ps;
    readonly List<ParticleSystem.Particle> particles = new();

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    private void Start()
    {
        if (PlayerController.Instance) ps.trigger.SetCollider(0, PlayerController.Instance.GetComponent<Collider2D>());
    }

    public void SetOrbCount(int count)
    {
        ParticleSystem.Burst burst = ps.emission.GetBurst(0);
        burst.count = new ParticleSystem.MinMaxCurve(count);
        ps.emission.SetBurst(0, burst);
    }

    private void OnParticleTrigger()
    {
        int triggeredParticles = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, particles);

        for (int i = 0; i < triggeredParticles; i++)
        {
            ParticleSystem.Particle p = particles[i];
            p.remainingLifetime = 0;
            particles[i] = p;
            GameManager.Instance.OrbCollected();
        }

        ps.SetTriggerParticles(ParticleSystemTriggerEventType.Enter, particles);
    }
}