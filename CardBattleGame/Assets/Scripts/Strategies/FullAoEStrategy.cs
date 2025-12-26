using CardGame.Models;
using CardGame.Strategies;
using CardGame.Utils;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CardGame.Strategies
{
    public class FullAoEStrategy : IAttackStrategy
    {
        public string AttackName => "Массовая Атака";
        public string Description => "Атакует всех врагов";

        public void Execute(Card attacker, List<Card> targets)
        {
           
            var validTargets = targets.Where(DamageCalculator.IsValidTarget).ToList();
            if (validTargets.Count == 0)
            {
                Debug.Log("Нет целей");
                return;
            }
            foreach (var target in targets)
            {
                if (DamageCalculator.IsValidTarget(target))
                {
                    bool isCrit = DamageCalculator.CalculateCritChance(attacker, target);
                    int damage = DamageCalculator.CalculateDamage(attacker, target, isCrit);
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
        }
        public List<Card> FindTargets(Card attacker, GameBoard board)
        {
            if (board == null) return new List<Card>();

            bool isPlayerAttacking = attacker.position.isPlayerSide;
            var allTargets = board.GetAllCards(!isPlayerAttacking)
                .Where(DamageCalculator.IsValidTarget)
                .ToList();
            return allTargets;
        }
        public bool CanExecute(Card attacker)
        {
            return DamageCalculator.IsValidTarget(attacker);
        }

    }
}
