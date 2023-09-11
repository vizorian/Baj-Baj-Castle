namespace Actor_Behaviour;

public class Actor : MonoBehaviour
{
    private readonly List<Collider2D> _hits = new();

    // Attributes
    public float MaxHealth;
    public float Health;
    public int Defense;
    public int Strength;
    public int Agility;
    public int Intelligence;
    public int Luck;
    public int Resistance;

    public ActorType ActorType;
    public Sprite SideSprite;
    public Sprite FrontSprite;
    public Sprite BackSprite;
    public ActorHand Hand;
    public GameObject HandPrefab;
    public float InteractionRange;
    public float MovementSpeed;
    public float ReachRange;
    public bool IsActive = false;

    private protected GameObject InteractionObject;
    private protected BoxCollider2D BoxCollider;
    private protected ContactFilter2D ContactFilter;
    private protected Vector3 KnockbackDirection;
    private protected Vector3 MoveDelta;
    private protected Rigidbody2D RigidBody;
    private protected SpriteRenderer SpriteRenderer;
    private protected GameObject Target;

    private protected virtual void Awake()
    {
        // Atribute usage 
        MovementSpeed += Agility * 0.01f;
        MaxHealth += Strength;
        Resistance += (int)(Strength + Intelligence * 0.5f);
        Defense += (int)(Intelligence * 0.2f + Agility * 0.01f);
        Health = MaxHealth;


        RigidBody = GetComponent<Rigidbody2D>();
        BoxCollider = GetComponent<BoxCollider2D>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
        ContactFilter = new ContactFilter2D();

        if (HandPrefab == null) return;
        Hand = Instantiate(HandPrefab, gameObject.transform.position, Quaternion.identity, gameObject.transform)
            .GetComponent<ActorHand>();
        Hand.Init(ReachRange);
        Hand.UpdateCenterPosition(transform.position);
    }

    private protected virtual void FixedUpdate()
    {
        // Handling collisions
        BoxCollider.OverlapCollider(ContactFilter, _hits);
        for (var i = 0; i < _hits.Count; i++)
        {
            OnCollide(_hits[i]);
            _hits[i] = null;
        }
    }

    private protected virtual void OnCollide(Collider2D otherCollider)
    {
    }

    // Used to receive and process damage
    // Called by weapons on collision
    [UsedImplicitly]
    private protected virtual void TakeDamage(DamageData damageData)
    {
        var knockback = damageData.Knockback;
        var damage = damageData.Amount;

        // Adjust damage based on if the weapon is flipped or not
        if (damageData.Source.Hand != null)
        {
            if (damageData.Type == DamageType.Piercing && damageData.Source.Hand.IsItemTurned)
                damageData.Type = DamageType.Slashing;
            else if (damageData.Type == DamageType.Slashing && damageData.Source.Hand.IsItemTurned)
                damageData.Type = DamageType.Piercing;
        }

        // Damage types
        if (damageData.Type == DamageType.Piercing)
            damage -= Defense / 4;
        else if (damageData.Type == DamageType.Bludgeoning)
            damage -= Defense / 2;
        else if (damageData.Type == DamageType.Slashing) damage -= Defense;

        if (damage < 1)
            damage = 1;

        if (damageData.IsCritical)
        {
            damage *= 2;
            knockback *= 2;
            FloatingText.Create(damage.ToString(), Color.yellow, transform.position, 1.2f, 0.5f, 0.2f);
        }
        else
        {
            FloatingText.Create(damage.ToString(), Color.red, transform.position, 1f, 0.5f, 0.2f);
        }

        CalculateKnockback(damageData.Source.transform.position, knockback);

        // Apply damage
        Health -= damage;
        if (Health <= 0) Die();
    }

    // Used to receive and process healing
    public virtual void Heal(float amount)
    {
        if (Health + amount > MaxHealth)
            Health = MaxHealth;
        else
            Health += amount;

        FloatingText.Create(amount.ToString(), Color.green, transform.position, 1f, 0.5f, 0.2f);
    }

    // Used to calculate knocback direction and strength
    private void CalculateKnockback(Vector3 from, float distance)
    {
        KnockbackDirection = transform.position - from;
        KnockbackDirection.Normalize();
        KnockbackDirection *= distance;
    }

    // Actor death logic
    private protected virtual void Die()
    {
        Destroy(gameObject);
    }

    // Actor movement logic
    private protected virtual void Move()
    {
        // Knockback processing
        if (Resistance >= 100)
        {
            KnockbackDirection = Vector3.zero;
        }
        else
        {
            var realResistance = Resistance / 100f;
            if (realResistance == 0) realResistance = 0.01f;
            KnockbackDirection = Vector3.Lerp(KnockbackDirection, Vector3.zero, realResistance);
        }

        MoveDelta += KnockbackDirection;

        // Final movement
        RigidBody.velocity = MoveDelta;
    }

    // Movement calculation
    private protected virtual void CalculateMovement()
    {
        MoveDelta = Target.transform.position - transform.position;
        MoveDelta.Normalize();
        MoveDelta *= MovementSpeed;
    }

    // Actor looking logic
    private protected virtual void LookAt(Vector3 lookTarget, ActorType actorType)
    {
        // Calculating position difference between the target and actor
        var posDif = lookTarget - transform.position;

        // Calculating the angle of the target relative to the actor
        var z = Mathf.Atan2(posDif.y, posDif.x) * Mathf.Rad2Deg;
        if (z < 0) z = 180 + (180 - Mathf.Abs(z));

        // Updating sprite to look at target
        if (z >= 45 && z < 135)
        {
            SpriteRenderer.sprite = BackSprite;
        }
        else if (z >= 135 && z < 225)
        {
            SpriteRenderer.sprite = SideSprite;
            SpriteRenderer.flipX = true;
        }
        else if (z >= 225 && z < 315)
        {
            SpriteRenderer.sprite = FrontSprite;
        }
        else
        {
            SpriteRenderer.sprite = SideSprite;
            SpriteRenderer.flipX = false;
        }
    }
}