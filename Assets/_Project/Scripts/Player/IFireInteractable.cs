namespace Scripts.Player
{
    public enum FireInteractionType
    {
        Bounce,
        Continuous,
        Manual
    }

    public interface IFireInteractable
    {
        FireInteractionType InteractionType { get; }
        int DifficultyLevel { get; }
        float InteractionCooldown { get; }
    }
}