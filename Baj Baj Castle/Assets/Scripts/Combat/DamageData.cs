public class DamageData
{
    public float Amount;
    public DamageType Type;
    public float Knockback;
    public Actor Source;
    public bool IsCritical;

    public DamageData(float amount, DamageType type, float knockback, Actor source, bool isCritical = false)
    {
        Amount = amount;
        Type = type;
        Knockback = knockback;
        Source = source;
        IsCritical = isCritical;
    }
}
