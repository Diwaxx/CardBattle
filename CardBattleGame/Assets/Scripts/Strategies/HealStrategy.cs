// HealStrategy.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CardGame.Models;
using CardGame.Controllers;
using CardGame.Utils;

namespace CardGame.Strategies
{
    public class HealStrategy : IAttackStrategy
    {
        public string AttackName => "Исцеление";
        public string Description => "Лечит союзников с наименьшим здоровьем";
        public int baseHealAmount = 20;
        public int maxTargets = 2;
        public bool canHealSelf = true;
        public float healInterval = 0.3f;

        public void Execute(Card healer, List<Card> targets)
        {
            if (targets == null || targets.Count == 0)
            {
                Debug.Log($" {healer.cardName}: Нет целей для лечения");
                UseFallbackAttack(healer);
                return;
            }

            Debug.Log($" {healer.cardName} использует {AttackName}!");
            healer.StartCoroutine(ExecuteHealSequence(healer, targets));
        }

        private System.Collections.IEnumerator ExecuteHealSequence(Card healer, List<Card> targets)
        {
            // Фильтруем цели
            var validTargets = targets
                .Where(t => t != null && t.IsAlive)
                .Where(t => canHealSelf || t != healer) // Проверяем можно ли лечить себя
                .OrderBy(t => t.stats.health / (float)t.stats.maxHealth) // Самые раненые первые
                .Take(maxTargets)
                .ToList();

            Debug.Log($" Лечение {validTargets.Count} целей:");

            foreach (var target in validTargets)
            {
                int healAmount = CalculateHealAmount(healer, target);
                int missingHealth = target.stats.maxHealth - target.stats.health;
                int actualHeal = Mathf.Min(healAmount, missingHealth);

                if (actualHeal > 0)
                {
                    float healthPercentBefore = (target.stats.health / (float)target.stats.maxHealth) * 100;
                    target.Heal(actualHeal, healer);
                    float healthPercentAfter = ((target.stats.health + actualHeal) / (float)target.stats.maxHealth) * 100;

                    Debug.Log($"  {target.cardName}: +{actualHeal} HP ({healthPercentBefore:F0}% → {healthPercentAfter:F0}%)");
                }

                yield return new WaitForSeconds(healInterval);
            }

            Debug.Log($" {AttackName} завершено");
        }

        private void UseFallbackAttack(Card healer)
        {
            Debug.Log($" {healer.cardName}: некому лечить, использую базовую атаку");

            var basicStrategy = new BasicAttackStrategy();
            var board = GameController.Instance?.Board;

            if (board != null)
            {
                var enemies = basicStrategy.FindTargets(healer, board);
                if (enemies.Count > 0)
                {
                    basicStrategy.Execute(healer, enemies);
                }
            }
        }

        public List<Card> FindTargets(Card healer, GameBoard board)
        {
            if (board == null || !CanExecute(healer))
                return new List<Card>();

            bool isPlayerHealer = healer.position.isPlayerSide;

            // Получаем всех союзников
            var allAllies = board.GetAllCards(isPlayerHealer)
                                 .Where(ally => ally != null && ally.IsAlive && ally.enabled)
                                 .ToList();

            if (allAllies.Count == 0) return new List<Card>();

            // Фильтруем тех, кому нужно лечение
            var woundedAllies = allAllies
                .Where(ally => ally.stats.health < ally.stats.maxHealth)
                .OrderBy(ally => ally.stats.health / (float)ally.stats.maxHealth)
                .Take(maxTargets * 2) // Берем больше на случай если некоторые на полном HP
                .ToList();

            return woundedAllies;
        }

        public bool CanExecute(Card healer)
        {
            return healer != null && healer.IsAlive && !healer.isStunned;
        }

        private int CalculateHealAmount(Card healer, Card target)
        {
            return Mathf.Max(1, baseHealAmount);
        }
    }
}