// DamageCalculator.cs
using CardGame.Models;
using UnityEngine;

namespace CardGame.Utils
{
    public static class DamageCalculator
    {
        // Расчет шанса крита
        public static bool CalculateCritChance(Card attacker, Card target)
        {
            if (attacker == null || target == null) return false;

            float critChance = attacker.stats.critChance - (target.stats.resilience * 0.01f);
            critChance = Mathf.Clamp(critChance, 0f, 0.8f); // Макс 80%
            return Random.value < critChance;
        }

        // Расчет урона с учетом крита
        public static int CalculateDamage(Card attacker, Card target, bool isCrit = false, float multiplier = 1.0f)
        {
            if (attacker == null || target == null) return 0;

            int baseDamage = isCrit ? attacker.stats.attack * 2 : attacker.stats.attack;
            int finalDamage = (int)(baseDamage * multiplier);

            // Учет брони
            finalDamage = Mathf.Max(1, finalDamage - target.stats.armor);

            return finalDamage;
        }

        // Расчет урона для AoE (с множителем)
        public static int CalculateAoEDamage(Card attacker, Card target, float aoeMultiplier = 0.7f)
        {
            bool isCrit = CalculateCritChance(attacker, target);
            return CalculateDamage(attacker, target, isCrit, aoeMultiplier);
        }

        // Проверка валидности цели
        public static bool IsValidTarget(Card card)
        {
            return card != null && card.IsAlive && card.enabled;
        }
    }
}