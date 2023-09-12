using GraphicsBehavior;
using UnityEngine;

namespace CreatureBehavior
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class Creature : MonoBehaviour
    {
        public readonly Stats Stats;
        public readonly Attributes Attributes;
        private readonly EntityGraphics _graphics;

        private Rigidbody2D _rigidbody;
        private BoxCollider2D _collider;
        private SpriteRenderer _renderer;

        public Creature(Stats stats, Attributes attributes, EntityGraphics graphics)
        {
            Stats = stats;
            Attributes = attributes;
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