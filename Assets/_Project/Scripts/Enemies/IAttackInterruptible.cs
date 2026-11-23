namespace Scripts.Enemies
{
    // Implemented by enemies whose attack wind-up can be cancelled by a hit (see EnemyHitStun).
    public interface IAttackInterruptible
    {
        bool IsWindingUp { get; }

        // Cancel the pending attack, reset its warning visuals and stop moving.
        void InterruptAttack();

        // Called when the stun ends; restart normal behaviour.
        void ResumeAfterStun();
    }
}
