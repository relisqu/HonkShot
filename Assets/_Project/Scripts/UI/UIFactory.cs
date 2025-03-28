using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Scripts.UI
{
    public class UIFactory
    {
        public const string UITextParticlePath = "Prefabs/UI/UIParticles/UITextParticle";
        public const string UIFireParticlePath = "Prefabs/UI/UIParticles/FireUITextParticle";
        public const string ExplosionPath = "Prefabs/VFX/Explosion";

        public GameObject CreateExplosionParticle(Vector3 position)
        {
            var gameObject = Resources.Load<GameObject>(ExplosionPath);
            var particle = Object.Instantiate(gameObject, position, Quaternion.identity);
            particle.transform.position = position;
            return particle;
        }

        public UITextParticle CreateUITextParticle(Vector3 position)
        {
            var gameObject = Resources.Load<GameObject>(UITextParticlePath);
            var particle = Object.Instantiate(gameObject, position, Quaternion.identity);
            particle.transform.position = position;
            return particle.GetComponent<UITextParticle>();
        }

        public UITextParticle CreateUIFireParticle(Vector3 position)
        {
            var gameObject = Resources.Load<GameObject>(UIFireParticlePath);
            var particle = Object.Instantiate(gameObject, position, Quaternion.identity);
            particle.transform.position = position;
            return particle.GetComponent<UITextParticle>();
        }
    }
}