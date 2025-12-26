using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CardGame.Models;
using CardGame.Controllers;

namespace CardGame.Strategies
{
    public class BackRowAttackStrategy : IAttackStrategy
    {
        public string AttackName => "Тыловая атака";
        public string Description => "Атакует врага в заднем ряду";

        public void Execute(Card attacker, List<Card> targets)
        {
            if (targets == null || targets.Count == 0)
            {
                Debug.Log("Нет целей для тыловой атаки!");
                return;
            }

            // Атакуем первую цель
            var target = targets[0];
            if (target != null && target.IsAlive)
            {
                bool isCrit = CalculateCrit(attacker, target);
                int damage = isCrit ? attacker.stats.attack * 2 : attacker.stats.attack;

                // Бонус к урону при атаке тыла
                damage = (int)(damage * 1.2f); // +20% урона к тылу

                Debug.Log($"🎯 Тыловая атака! Урон: {damage}");

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
            var backRow = board.GetBackRowCards(!isPlayerAttacking);

            // Возвращаем случайную цель из заднего ряда
            if (backRow.Count > 0)
            {
                var target = backRow[Random.Range(0, backRow.Count)];
                return new List<Card> { target };
            }

            return new List<Card>();
        }

        public bool CanExecute(Card attacker)
        {
            return attacker != null && attacker.IsAlive;
        }

        private bool CalculateCrit(Card attacker, Card target)
        {
            float critChance = attacker.stats.critChance - (target.stats.resilience * 0.01f);
            critChance = Mathf.Clamp(critChance, 0f, 0.8f);
            return Random.value < critChance;
        }
    }
}