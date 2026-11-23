using DG.Tweening;
using UnityEngine;

namespace Scripts.Enemies
{
    public static class EnemyAttackVisuals
    {
        private static readonly int AttackTrigger = Animator.StringToHash("Attack");
        private static readonly int IsWalkingBool = Animator.StringToHash("IsWalking");

        public static void ResetAfterInterrupt(Animator animator, string idleStateName,
            SpriteRenderer prefireSpriteRenderer, ParticleSystem warningEffect)
        {
            if (prefireSpriteRenderer)
            {
                prefireSpriteRenderer.DOKill();
                prefireSpriteRenderer.color = Color.clear;
            }

            if (warningEffect)
                warningEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            if (!animator || !animator.isActiveAndEnabled || !animator.runtimeAnimatorController) return;

            animator.ResetTrigger(AttackTrigger);
            animator.SetBool(IsWalkingBool, false);

            if (string.IsNullOrEmpty(idleStateName)) return;
            int idleHash = Animator.StringToHash(idleStateName);
            if (animator.HasState(0, idleHash))
                animator.Play(idleHash, 0, 0f);
        }
    }
}
