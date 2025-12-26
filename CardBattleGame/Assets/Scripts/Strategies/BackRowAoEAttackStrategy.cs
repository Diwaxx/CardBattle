using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CardGame.Models;
using CardGame.Controllers;
using CardGame.Utils;

namespace CardGame.Strategies
{
    public class BackRowAoEAttackStrategy : IAttackStrategy
    {
        public string AttackName => "Обстрел тыла";
        public string Description => "Атакует всех врагов в заднем ряду (AoE)";

        // Настройки баланса
        public float aoeDamageMultiplier = 0.7f;
        public float timeBetweenShots = 0.15f;

        public void Execute(Card attacker, List<Card> targets)
        {
            if (targets == null || targets.Count == 0)
            {
                Debug.Log($" {attacker.cardName}: Нет целей для {AttackName}");
                return;
            }

            Debug.Log($" {attacker.cardName} использует {AttackName} по {targets.Count} целям!");

            // Запускаем AoE атаку
            attacker.StartCoroutine(ExecuteAoEAttack(attacker, targets));
        }

        private System.Collections.IEnumerator ExecuteAoEAttack(Card attacker, List<Card> targets)
        {
            CardShooter shooter = attacker.GetComponent<CardShooter>();
            bool hasShooter = shooter != null && shooter.projectilePrefab != null;

            // Сортируем цели слева направо для лучшего визуала
            var sortedTargets = targets
                .Where(DamageCalculator.IsValidTarget)
                .OrderBy(t => t.position.column)
                .ToList();

            Debug.Log($"AoE атака по {sortedTargets.Count} целям:");

            foreach (var target in sortedTargets)
            {
                int damage = DamageCalculator.CalculateAoEDamage(attacker, target, aoeDamageMultiplier);
                bool isCrit = damage > (int)(attacker.stats.attack * aoeDamageMultiplier); // Определяем был ли крит

                Debug.Log($"{target.cardName}: {damage} урона{(isCrit ? " (КРИТ!)" : "")}");

                if (hasShooter)
                {
                    shooter.ShootAt(target, damage, isCrit);
                }
                else
                {
                    target.TakeDamage(damage, isCrit, attacker);
                }

                // Пауза между выстрелами в AoE
                yield return new WaitForSeconds(timeBetweenShots);
            }

            Debug.Log($"{AttackName} завершена");
        }

        public List<Card> FindTargets(Card attacker, GameBoard board)
        {
            if (board == null || !CanExecute(attacker))
                return new List<Card>();

            bool isPlayerAttacking = attacker.position.isPlayerSide;
            bool targetSide = !isPlayerAttacking;

            Debug.Log($"{attacker.cardName} ищет цели для AoE");

            // 1. Сначала ищем в ЗАДНЕМ ряду противника
            int enemyBackRow = isPlayerAttacking ? 0 : 1;   // Задний ряд противника
            var backRowTargets = board.GetRowCards(targetSide, enemyBackRow)
                                      .Where(DamageCalculator.IsValidTarget)
                                      .ToList();


            if (backRowTargets.Count > 0)
            {
                Debug.Log($"AoE атака по заднему ряду");
                return backRowTargets;
            }

            // 2. Если задний ряд пуст, ищем в ПЕРЕДНЕМ ряду
            int enemyFrontRow = isPlayerAttacking ? 1 : 0;  // Передний ряд противника
            var frontRowTargets = board.GetRowCards(targetSide, enemyFrontRow)
                                       .Where(DamageCalculator.IsValidTarget)
                                       .ToList();

            Debug.Log($"   Передний ряд противника (row {enemyFrontRow}): {frontRowTargets.Count} целей");

            if (frontRowTargets.Count > 0)
            {
                Debug.Log($"Задний ряд пуст, AoE атака по переднему ряду");
                return frontRowTargets;
            }

            Debug.Log($"Нет целей для AoE");
            return new List<Card>();
        }

        public bool CanExecute(Card attacker)
        {
            return DamageCalculator.IsValidTarget(attacker);
        }
    }
}