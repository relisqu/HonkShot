namespace Scripts.Items
{
    public enum StatModType { Add, Mult }

    public class StatModifier
    {
        public StatModType type;
        public float value;
        public int order; // Lower order = applied first

        public StatModifier(StatModType type, float value, int order = 0)
        {
            this.type = type;
            this.value = value;
        }
    }
} 