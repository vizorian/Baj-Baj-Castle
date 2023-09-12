namespace CreatureBehavior
{
    public class Attributes
    {
        private const int DEFAULT_VALUE = 1;

        public int Strength { get; private set; } = DEFAULT_VALUE;
        public int Dexterity { get; private set; } = DEFAULT_VALUE;
        public int Constitution { get; private set; } = DEFAULT_VALUE;
        public int Intelligence { get; private set; } = DEFAULT_VALUE;
        public int Wisdom { get; private set; } = DEFAULT_VALUE;
        public int Charisma { get; private set; } = DEFAULT_VALUE;
    }
}