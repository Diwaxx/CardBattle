using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CardGame.Models;
using CardGame.Controllers;
using CardGame.Utils;

namespace CardGame.Strategies
{
    public class BasicAttackStrategy : IAttackStrategy
    {
        public string AttackName => "Базовая атака";
        public string Description => "Атакует одну случайную цель в переднем ряду";

        public void Execute(Card attacker, List<Card> targets)
        {
            if (targets == null || targets.Count == 0)
            {
                Debug.Log("Нет целей для атаки!");
                return;
            }

            Debug.Log($"🎯 BasicAttackStrategy.Execute() для {attacker.cardName}");
            Debug.Log($"   Найдено целей: {targets.Count}");

            // Выбираем ОДНУ случайную цель из валидных
            var validTargets = targets.Where(DamageCalculator.IsValidTarget).ToList();

            if (validTargets.Count == 0)
            {
                Debug.Log("Нет живых целей!");
                return;
            }

            Card target = validTargets[Random.Range(0, validTargets.Count)];
            Debug.Log($"✅ Выбрана цель: {target.cardName}");

            bool isCrit = DamageCalculator.CalculateCritChance(attacker, target);
            int damage = DamageCalculator.CalculateDamage(attacker, target, isCrit);

            Debug.Log($"💥 Наношу урон {damage}{(isCrit ? " (КРИТ!)" : "")}");

            // Пытаемся использовать CardShooter
            CardShooter shooter = attacker.GetComponent<CardShooter>();
            if (shooter != null && shooter.projectilePrefab != null)
            {
                shooter.ShootAt(target, damage, isCrit);
            }
            else
            {
                // Без снаряда - сразу наносим урон
                target.TakeDamage(damage, isCrit);
            }
        }

        public List<Card> FindTargets(Card attacker, GameBoard board)
        {
            if (board == null) return new List<Card>();

            bool isPlayerAttacking = attacker.position.isPlayerSide;
            bool targetSide = !isPlayerAttacking;

            Debug.Log($"🎯 {attacker.cardName} ищет цели (Basic)");

            // Определяем ряды противника
            int enemyFrontRow = isPlayerAttacking ? 1 : 0;  // Передний ряд противника
            int enemyBackRow = isPlayerAttacking ? 0 : 1;   // Задний ряд противника

            // 1. Сначала front row противника
            var frontTargets = board.GetRowCards(targetSide, enemyFrontRow)
                                    .Where(DamageCalculator.IsValidTarget)
                                    .ToList();
            Debug.Log($"   Front row противника (row {enemyFrontRow}): {frontTargets.Count} целей");

            if (frontTargets.Count > 0)
            {
                return frontTargets; 
            }

            // 2. Если front row пуст, back row противника
            var backTargets = board.GetRowCards(targetSide, enemyBackRow)
                                   .Where(DamageCalculator.IsValidTarget)
                                   .ToList();
            Debug.Log($"   Back row противника (row {enemyBackRow}): {backTargets.Count} целей");

            return backTargets;
        }

        public bool CanExecute(Card attacker)
        {
            return DamageCalculator.IsValidTarget(attacker);
        }
    }
}