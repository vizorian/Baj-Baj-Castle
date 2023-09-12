using GraphicsBehavior;
using UnityEngine;

namespace CreatureBehavior
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class Creature : MonoBehaviour
    {
        // TODO figure out fields
        private readonly EntityGraphics _graphics;

        private Rigidbody2D _rigidbody;
        private BoxCollider2D _collider;
        private SpriteRenderer _renderer;

        public Creature(EntityGraphics graphics)
        {
            _graphics = graphics;
        }

        public void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _collider = GetComponent<BoxCollider2D>();
            _renderer = GetComponent<SpriteRenderer>();
        }
    }
}