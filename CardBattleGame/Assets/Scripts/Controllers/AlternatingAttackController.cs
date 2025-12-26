using System.Collections.Generic;
using UnityEngine;
using CardGame.Models;
using CardGame.Strategies;

namespace CardGame.Controllers
{
    public class AlternatingAttackController : MonoBehaviour
    {
        [System.Serializable]
        public class AttackCycle
        {
            public IAttackStrategy strategy;
            public string strategyName;
            public int usesBeforeSwitch = 1; // Сколько раз использовать перед сменой
            [HideInInspector] public int currentUses = 0;
        }

        [Header("Attack Cycle")]
        public List<AttackCycle> attackCycle = new List<AttackCycle>();

        [Header("Settings")]
        public bool useRandomAfterCycle = false; // После цикла использовать случайную стратегию
        public int currentCycleIndex = 0;

        private Card card;
        private IAttackStrategy currentStrategy;

        void Start()
        {
            card = GetComponent<Card>();

            // Инициализируем цикл если пустой
            if (attackCycle.Count == 0)
            {
                InitializeDefaultCycle();
            }

            // Устанавливаем начальную стратегию
            SelectNextStrategy();

            Debug.Log($"🔄 {card.cardName} alternating attack controller initialized");
        }

        private void InitializeDefaultCycle()
        {
            // Базовый цикл: 1 базовая атака, 1 AoE
            attackCycle.Add(new AttackCycle
            {
                strategy = new BasicAttackStrategy(),
                strategyName = "Базовая атака",
                usesBeforeSwitch = 1
            });

            attackCycle.Add(new AttackCycle
            {
                strategy = new BackRowAoEAttackStrategy(),
                strategyName = "Обстрел тыла",
                usesBeforeSwitch = 1
            });
        }

        public void SelectNextStrategy()
        {
            if (attackCycle.Count == 0)
            {
                currentStrategy = new BasicAttackStrategy();
                return;
            }

            // Получаем текущий цикл
            var currentCycle = attackCycle[currentCycleIndex];

            // Увеличиваем счетчик использования
            currentCycle.currentUses++;

            // Если достигли лимита использования - переходим к следующей стратегии
            if (currentCycle.currentUses >= currentCycle.usesBeforeSwitch)
            {
                currentCycle.currentUses = 0;
                currentCycleIndex = (currentCycleIndex + 1) % attackCycle.Count;

                // Если включен случайный выбор после цикла
                if (useRandomAfterCycle && currentCycleIndex == 0)
                {
                    currentCycleIndex = Random.Range(0, attackCycle.Count);
                }

                Debug.Log($"🔄 {card.cardName} переключил стратегию на: {attackCycle[currentCycleIndex].strategyName}");
            }

            currentStrategy = attackCycle[currentCycleIndex].strategy;
        }

        public List<Card> FindTargets()
        {
            if (currentStrategy == null)
            {
                SelectNextStrategy();
            }

            var board = GameController.Instance?.Board;
            if (board == null) return new List<Card>();

            return currentStrategy.FindTargets(card, board);
        }

        public void ExecuteAttack()
        {
            if (currentStrategy == null || card == null)
            {
                SelectNextStrategy();
            }

            var targets = FindTargets();

            if (targets.Count > 0)
            {
                Debug.Log($"🎯 {card.cardName} использует {currentStrategy.AttackName}");
                currentStrategy.Execute(card, targets);

                // После выполнения атаки готовим следующую стратегию
                SelectNextStrategy();
            }
            else
            {
                Debug.LogWarning($"🎯 {card.cardName} не нашел целей для {currentStrategy.AttackName}");
            }
        }

        public void AddToCycle(IAttackStrategy strategy, string name, int uses = 1)
        {
            attackCycle.Add(new AttackCycle
            {
                strategy = strategy,
                strategyName = name,
                usesBeforeSwitch = uses
            });
        }

        public void SetCycle(List<AttackCycle> newCycle)
        {
            attackCycle = newCycle;
            currentCycleIndex = 0;
            SelectNextStrategy();
        }

        public string GetCurrentStrategyName()
        {
            return currentStrategy?.AttackName ?? "Базовая атака";
        }

        public int GetCyclePosition()
        {
            return currentCycleIndex;
        }

        public int GetUsesRemaining()
        {
            if (currentCycleIndex < attackCycle.Count)
            {
                var cycle = attackCycle[currentCycleIndex];
                return cycle.usesBeforeSwitch - cycle.currentUses;
            }
            return 0;
        }
    }
}