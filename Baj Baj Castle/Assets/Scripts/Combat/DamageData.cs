public class DamageData
{
    public float Amount;
    public DamageType Type;
    public float Knockback;
    public Actor Source;

    public DamageData(float amount, DamageType type, float knockback, Actor source)
    {
        Amount = amount;
        Type = type;
        Knockback = knockback;
        Source = source;
    }
}
