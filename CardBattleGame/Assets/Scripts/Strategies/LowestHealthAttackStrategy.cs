using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CardGame.Models;
using CardGame.Controllers;
using CardGame.Utils;

namespace CardGame.Strategies
{
    public class LowestHealthAttackStrategy : IAttackStrategy
    {
        public string AttackName => "Добивание";
        public string Description => "Атакует врага с наименьшим здоровьем";

        public void Execute(Card attacker, List<Card> targets)
        {
            if (targets == null || targets.Count == 0)
            {
                Debug.Log("Нет целей для добивания!");
                return;
            }

            // Атакуем первую цель (самую слабую)
            var target = targets[0];
            if (target != null && target.IsAlive)
            {
                bool isCrit = DamageCalculator.CalculateCritChance(attacker, target);
                int damage = DamageCalculator.CalculateDamage(attacker, target, isCrit);

                // Бонус к урону против слабых целей
                if (target.stats.health < target.stats.maxHealth * 0.3f)
                {
                    damage = (int)(damage * 1.3f); // +30% урона к слабым
                    Debug.Log($"💀 Добивание! Урон: {damage}");
                }

                CardShooter shooter = attacker.GetComponent<CardShooter>();
                if (shooter != null && shooter.projectilePrefab != null)
                {
                    shooter.ShootAt(target, damage, isCrit);
                }
                else
                {
                    target.TakeDamage(damage, isCrit);
                }
            }
        }

        public List<Card> FindTargets(Card attacker, GameBoard board)
        {
            if (board == null) return new List<Card>();

            bool isPlayerAttacking = attacker.position.isPlayerSide;
            var allTargets = board.GetAllCards(!isPlayerAttacking)
                .Where(t => t != null && t.IsAlive)
                .ToList();

            if (allTargets.Count == 0) return new List<Card>();

            // Находим цель с наименьшим здоровьем
            var weakestTarget = allTargets
                .OrderBy(t => t.stats.health)
                .FirstOrDefault();

            return new List<Card> { weakestTarget };
        }

        public bool CanExecute(Card attacker)
        {
            return attacker != null && attacker.IsAlive;
        }

    
    }
}