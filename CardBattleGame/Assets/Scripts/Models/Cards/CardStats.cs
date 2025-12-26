using System;
using UnityEngine;

namespace CardGame.Models
{
    [Serializable]
    public class CardStats
    {
        [Header("Основные характеристики")]
        public int health = 100;
        public int maxHealth = 100;
        public int attack = 20;
        public int speed = 5;

        [Header("Дополнительные характеристики")]
        [Range(0, 1)] public float critChance = 0.1f;
        [Range(0, 1)] public float dodgeChance = 0.05f;
        public int armor = 5;
        public int resilience = 10; // Стойкость , сопротивление к криту

        public CardStats() { }

        public CardStats(int health, int attack, int speed, float critChance = 0.1f,
                        float dodgeChance = 0.05f, int armor = 0, int resilience = 0)
        {
            this.health = health;
            this.maxHealth = health;
            this.attack = attack;
            this.speed = speed;
            this.critChance = critChance;
            this.dodgeChance = dodgeChance;
            this.armor = armor;
            this.resilience = resilience;
        }
    }
}