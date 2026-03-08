using System;
using System.Collections;
using UnityEngine;

namespace Scripts.Enemies.Bosses
{
    public class BossIntro : MonoBehaviour
    {
        [SerializeField] private float _introDelay = 2f;

        public bool IsComplete { get; private set; }
        public event Action IntroFinished;

        public void Play()
        {
            StartCoroutine(IntroRoutine());
        }

        private IEnumerator IntroRoutine()
        {
            // Future: trigger intro animation here
            yield return new WaitForSeconds(_introDelay);
            IsComplete = true;
            IntroFinished?.Invoke();
        }
    }
}
