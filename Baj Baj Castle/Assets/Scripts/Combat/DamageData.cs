public class DamageData
{
    public int Amount;
    public DamageType Type;
    public float Knockback;
    public Actor Source;

    public DamageData(int amount, DamageType type, float knockback, Actor source)
    {
        Amount = amount;
        Type = type;
        Knockback = knockback;
        Source = source;
    }
}
