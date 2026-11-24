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

            if (!IsUsable(animator)) return;

            animator.SetBool(IsWalkingBool, false);
            ResetToIdle(animator, idleStateName, AttackTrigger);
        }

        public static void ResetToIdle(Animator animator, string idleStateName, params int[] triggersToReset)
        {
            if (!IsUsable(animator)) return;

            foreach (int trigger in triggersToReset)
                animator.ResetTrigger(trigger);

            if (string.IsNullOrEmpty(idleStateName)) return;
            int idleHash = Animator.StringToHash(idleStateName);
            if (animator.HasState(0, idleHash))
                animator.Play(idleHash, 0, 0f);
        }

        private static bool IsUsable(Animator animator)
        {
            return animator && animator.isActiveAndEnabled && animator.runtimeAnimatorController;
        }
    }
}
