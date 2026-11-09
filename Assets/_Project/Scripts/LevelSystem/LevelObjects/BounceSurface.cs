namespace Scripts.LevelSystem.LevelObjects
{
    /// <summary>
    /// Which bank of bounce sounds an entity reaches for when it hits a <see cref="BounceObject"/>.
    /// </summary>
    public enum BounceSurface
    {
        /// <summary>Resolved once on Awake: anything carrying an EnemyHealth counts as Enemy, everything else as Obstacle.</summary>
        Auto,
        Obstacle,
        Enemy
    }
}
