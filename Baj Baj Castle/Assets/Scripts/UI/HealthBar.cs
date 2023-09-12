using CreatureBehavior;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [UsedImplicitly]
    public class HealthBar : MonoBehaviour
    {
        public CreatureBehavior_Old.Player Player;
        private Slider slider;

        [UsedImplicitly]
        private void Awake()
        {
            Player = GameObject.FindGameObjectWithTag("Player").GetComponent<CreatureBehavior_Old.Player>();
            slider = GetComponent<Slider>();
            slider.maxValue = Player.MaxHealth;
            slider.value = Player.Health;
        }

        [UsedImplicitly]
        private void Update()
        {
            slider.value = Player.Health;
        }
    }
}