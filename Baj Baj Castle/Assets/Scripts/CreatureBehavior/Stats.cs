namespace CreatureBehavior
{
    public class Stats
    {
        private const int DEFAULT_VALUE = 1;

        public int MaxHealth { get; set; } = DEFAULT_VALUE;
        public int Health { get; set; } = DEFAULT_VALUE;
        public int Shield { get; set; } = DEFAULT_VALUE;
    }
}