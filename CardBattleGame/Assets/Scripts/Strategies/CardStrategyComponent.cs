// CardStrategyComponent.cs
using System.Collections.Generic;
using UnityEngine;
using CardGame.Strategies;

namespace CardGame.Models
{
    public class CardStrategyComponent : MonoBehaviour
    {
        [Header("Конфигурация стратегий")]
        public StrategyConfig strategyConfig;

        [Header("Ручная настройка")]
        public List<string> manualStrategies = new List<string>();
        public bool useConfig = true;
        public bool randomSelection = false;

        [Header("Текущее состояние")]
        [SerializeField] private List<IAttackStrategy> availableStrategies = new List<IAttackStrategy>();
        [SerializeField] private int currentStrategyIndex = 0;
        [SerializeField] private int currentUses = 0;

        private Card card;
        private IAttackStrategy currentStrategy;

        void Start()
        {
            card = GetComponent<Card>();
            InitializeStrategies();

            Debug.Log($" CardStrategyComponent для {card.cardName} готов");
            Debug.Log($"   Доступно стратегий: {availableStrategies.Count}");
        }

        public void InitializeStrategies()
        {
            availableStrategies.Clear();

            if (useConfig && strategyConfig != null)
            {
                // Используем конфиг из ScriptableObject
                availableStrategies = StrategyFactory.CreateStrategiesForCard(strategyConfig);
                Debug.Log($" Использую конфиг: {strategyConfig.name}");
            }
            else if (manualStrategies.Count > 0)
            {
                // Используем ручной список
                foreach (var strategyName in manualStrategies)
                {
                    var strategy = StrategyFactory.CreateStrategy(strategyName);
                    if (strategy != null)
                    {
                        availableStrategies.Add(strategy);
                    }
                }
                Debug.Log($" Использую ручной список стратегий");
            }
            else
            {
                // Используем только базовую
                availableStrategies.Add(StrategyFactory.GetBasicStrategy());
                Debug.Log($"Использую только базовую стратегию");
            }

            // Выбираем начальную стратегию
            if (randomSelection && availableStrategies.Count > 1)
            {
                currentStrategyIndex = Random.Range(0, availableStrategies.Count);
            }

            currentStrategy = availableStrategies[currentStrategyIndex];
            currentUses = 0;

            LogCurrentStrategy();
        }

        public IAttackStrategy GetCurrentStrategy()
        {
            if (currentStrategy == null)
            {
                currentStrategy = StrategyFactory.GetBasicStrategy();
            }
            return currentStrategy;
        }

        public void UseStrategy()
        {
            if (availableStrategies.Count == 0) return;

            currentUses++;

            // Проверяем нужно ли сменить стратегию
            if (strategyConfig != null && currentStrategyIndex < availableStrategies.Count)
            {
                var strategyInfo = strategyConfig.availableStrategies[currentStrategyIndex];
                int usesBeforeSwitch = strategyInfo.usesBeforeSwitch;

                if (currentUses >= usesBeforeSwitch)
                {
                    SwitchToNextStrategy();
                }
            }
        }

        public void SwitchToNextStrategy()
        {
            if (availableStrategies.Count <= 1) return;

            currentStrategyIndex = (currentStrategyIndex + 1) % availableStrategies.Count;
            currentStrategy = availableStrategies[currentStrategyIndex];
            currentUses = 0;

            LogCurrentStrategy();
        }

        public void SwitchToStrategy(string strategyName)
        {
            for (int i = 0; i < availableStrategies.Count; i++)
            {
                if (availableStrategies[i].AttackName.Contains(strategyName))
                {
                    currentStrategyIndex = i;
                    currentStrategy = availableStrategies[i];
                    currentUses = 0;
                    LogCurrentStrategy();
                    return;
                }
            }

            Debug.LogWarning($"⚠ Стратегия '{strategyName}' не найдена для {card.cardName}");
        }

        public void AddStrategy(string strategyName)
        {
            var strategy = StrategyFactory.CreateStrategy(strategyName);
            if (strategy != null && !ContainsStrategy(strategyName))
            {
                availableStrategies.Add(strategy);
                Debug.Log($" Добавлена стратегия: {strategyName}");
            }
        }

        public void RemoveStrategy(string strategyName)
        {
            for (int i = availableStrategies.Count - 1; i >= 0; i--)
            {
                if (availableStrategies[i].AttackName.Contains(strategyName))
                {
                    availableStrategies.RemoveAt(i);
                    Debug.Log($" Удалена стратегия: {strategyName}");

                    // Корректируем индекс если нужно
                    if (currentStrategyIndex >= availableStrategies.Count)
                    {
                        currentStrategyIndex = 0;
                        if (availableStrategies.Count > 0)
                        {
                            currentStrategy = availableStrategies[0];
                        }
                    }
                    return;
                }
            }
        }

        public bool ContainsStrategy(string strategyName)
        {
            foreach (var strategy in availableStrategies)
            {
                if (strategy.AttackName.Contains(strategyName))
                {
                    return true;
                }
            }
            return false;
        }

        public List<string> GetAvailableStrategyNames()
        {
            var names = new List<string>();
            foreach (var strategy in availableStrategies)
            {
                names.Add(strategy.AttackName);
            }
            return names;
        }

        private void LogCurrentStrategy()
        {
            if (currentStrategy != null)
            {
                Debug.Log($" {card.cardName} выбрал стратегию: {currentStrategy.AttackName}");
            }
        }

        // Для отладки в редакторе
        void OnValidate()
        {
            if (Application.isPlaying && card != null)
            {
                InitializeStrategies();
            }
        }
    }
}