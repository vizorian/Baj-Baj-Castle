using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Player Player;
    private Slider slider;
    private void Awake()
    {
        Player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        slider = GetComponent<Slider>();
        slider.maxValue = Player.MaxHealth;
        slider.value = Player.Health;
    }

    private void Update()
    {
        slider.value = Player.Health;
    }
}
