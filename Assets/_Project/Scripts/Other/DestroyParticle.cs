using System;
using UnityEngine;

namespace Scripts.Other
{
    public class DestroyParticle : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _particleSystem;

        private void OnParticleSystemStopped()
        {
            Destroy(gameObject);
        }
    }
}