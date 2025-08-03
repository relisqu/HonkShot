namespace Scripts.Items.StatSystems
{
    public interface IStatModifier<T>
    {
        int Id { get; }
        T Value { get; }
        int Order { get; }

        public void SetValue(T newValue);
        public void SetOrder(int order);
        public void SetId(int id);
    }
}