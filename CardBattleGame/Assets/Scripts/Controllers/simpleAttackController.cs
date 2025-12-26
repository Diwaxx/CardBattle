using System.Collections.Generic;
using UnityEngine;
using CardGame.Models;
using CardGame.Strategies;

namespace CardGame.Controllers
{
    public class SimpleAttackController : MonoBehaviour
    {
        private Card card;
        private CardStrategyComponent strategyComponent;
        private IAttackStrategy currentStrategy;

        void Start()
        {
            card = GetComponent<Card>();
            strategyComponent = GetComponent<CardStrategyComponent>();

            if (strategyComponent == null)
            {
                // Создаем компонент стратегий если его нет
                strategyComponent = gameObject.AddComponent<CardStrategyComponent>();
                Debug.Log($"➕ Добавлен CardStrategyComponent для {card.cardName}");
            }

            currentStrategy = strategyComponent.GetCurrentStrategy();
            Debug.Log($"🎯 SimpleAttackController для {card.cardName} готов");
        }

        public void ExecuteAttack()
        {
            if (currentStrategy == null || card == null)
            {
                currentStrategy = StrategyFactory.GetBasicStrategy();
            }

            var board = GameController.Instance?.Board;
            if (board == null) return;

            var targets = currentStrategy.FindTargets(card, board);

            if (targets.Count > 0)
            {
                Debug.Log($"⚔ {card.cardName} использует {currentStrategy.AttackName}");
                currentStrategy.Execute(card, targets);

                // Уведомляем компонент о использовании стратегии
                strategyComponent.UseStrategy();

                // Обновляем текущую стратегию
                currentStrategy = strategyComponent.GetCurrentStrategy();
            }
            else
            {
                Debug.LogWarning($"🎯 {card.cardName}: нет целей для {currentStrategy.AttackName}");
            }
        }

        public void ExecuteAttackWithTarget(Card target)
        {
            if (target == null || !target.IsAlive)
            {
                Debug.LogWarning($"🎯 {card.cardName}: цель недействительна");
                return;
            }

            if (currentStrategy == null)
            {
                currentStrategy = strategyComponent?.GetCurrentStrategy() ?? StrategyFactory.GetBasicStrategy();
            }

            // Создаем список с одной целью
            List<Card> targets = new List<Card> { target };

            Debug.Log($"⚔ {card.cardName} атакует {currentStrategy.AttackName} -> {target.cardName}");
            currentStrategy.Execute(card, targets);

            strategyComponent.UseStrategy();
            currentStrategy = strategyComponent.GetCurrentStrategy();
        }

        public void SetStrategy(IAttackStrategy newStrategy)
        {
            currentStrategy = newStrategy;
            Debug.Log($"🔄 {card.cardName} сменил стратегию на: {currentStrategy?.AttackName ?? "Basic"}");
        }

        public string GetCurrentStrategyName()
        {
            return currentStrategy?.AttackName ?? "Базовая атака";
        }
    }
}