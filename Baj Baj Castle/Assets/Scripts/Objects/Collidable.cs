namespace Objects;

public class Collidable : MonoBehaviour
{
    private protected BoxCollider2D BoxCollider;
    private protected ContactFilter2D ContactFilter;
    private protected List<Collider2D> Hits = new();

    private protected virtual void Awake()
    {
        BoxCollider = GetComponent<BoxCollider2D>();
        ContactFilter = new ContactFilter2D();
    }

    private protected virtual void FixedUpdate()
    {
        // Handling collisions
        BoxCollider.OverlapCollider(ContactFilter, Hits);
        for (var i = 0; i < Hits.Count; i++)
        {
            OnCollide(Hits[i]);
            Hits[i] = null;
        }
    }

    // Handle collision
    private protected virtual void OnCollide(Collider2D otherCollider)
    {
        Debug.Log(otherCollider.name);
    }
}