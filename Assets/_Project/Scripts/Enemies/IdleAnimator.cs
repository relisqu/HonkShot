using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Scripts.Enemies
{
    public class IdleAnimator : MonoBehaviour
    {
        [SerializeField] private Animator _animator;

        private IEnumerator Start()
        {
            _animator.speed = 0;
            yield return new WaitForSeconds(Random.Range(0f,1f));
            _animator.speed = 1;
        }
    }
}