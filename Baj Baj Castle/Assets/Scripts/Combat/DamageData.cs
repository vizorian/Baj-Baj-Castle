namespace Combat;

public class DamageData
{
    public float Amount;
    public bool IsCritical = false;
    public float Knockback;
    public Actor Source;
    public DamageType Type;

    public DamageData(float amount, DamageType type, float knockback, Actor source)
    {
        Amount = amount;
        Type = type;
        Knockback = knockback;
        Source = source;
    }
}