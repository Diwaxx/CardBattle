
using System.Collections.Generic;
using UnityEngine;

namespace CardGame.Strategies
{
    public static class StrategyFactory
    {
        private static Dictionary<string, IAttackStrategy> strategyCache = new Dictionary<string, IAttackStrategy>();
        private static StrategyConfig defaultConfig;

        // Инициализация фабрики
        public static void Initialize(StrategyConfig config = null)
        {
            defaultConfig = config;

            // Предзагружаем часто используемые стратегии
            strategyCache["Basic"] = new BasicAttackStrategy();
            strategyCache["BackRowAoE"] = new BackRowAoEAttackStrategy();

            Debug.Log(" StrategyFactory initialized");
        }

        // Создание стратегии по имени
        public static IAttackStrategy CreateStrategy(string strategyName, StrategyConfig config = null)
        {
            if (string.IsNullOrEmpty(strategyName))
            {
                Debug.LogWarning(" Strategy name is empty, using Basic");
                return GetBasicStrategy();
            }

            // Используем кэшированные стратегии если есть
            if (strategyCache.TryGetValue(strategyName, out IAttackStrategy cachedStrategy))
            {
                return cachedStrategy;
            }

            // Создаем новую стратегию
            IAttackStrategy strategy = CreateNewStrategy(strategyName, config);

            if (strategy != null)
            {
                strategyCache[strategyName] = strategy;
            }

            return strategy ?? GetBasicStrategy();
        }

        // Создание стратегии для карты на основе конфига
        public static List<IAttackStrategy> CreateStrategiesForCard(StrategyConfig config)
        {
            var strategies = new List<IAttackStrategy>();

            if (config == null)
            {
                strategies.Add(GetBasicStrategy());
                return strategies;
            }

            foreach (var strategyWeight in config.availableStrategies)
            {
                var strategy = CreateStrategy(strategyWeight.strategyName, config);
                if (strategy != null)
                {
                    strategies.Add(strategy);
                }
            }

            // Если нет стратегий - добавляем базовую
            if (strategies.Count == 0)
            {
                strategies.Add(GetBasicStrategy());
            }

            return strategies;
        }

        // Получить рандомную стратегию по весам
        public static IAttackStrategy GetRandomStrategy(StrategyConfig config)
        {
            if (config == null || config.availableStrategies.Count == 0)
                return GetBasicStrategy();

            // Рассчитываем общий вес
            int totalWeight = 0;
            foreach (var strategy in config.availableStrategies)
            {
                totalWeight += strategy.weight;
            }

            if (totalWeight == 0)
                return GetBasicStrategy();

            // Выбираем стратегию по весу
            int randomValue = Random.Range(0, totalWeight);
            int currentWeight = 0;

            foreach (var strategy in config.availableStrategies)
            {
                currentWeight += strategy.weight;
                if (randomValue < currentWeight)
                {
                    return CreateStrategy(strategy.strategyName, config);
                }
            }

            return GetBasicStrategy();
        }

        // Получить базовую стратегию
        public static IAttackStrategy GetBasicStrategy()
        {
            if (!strategyCache.ContainsKey("Basic"))
            {
                strategyCache["Basic"] = new BasicAttackStrategy();
            }
            return strategyCache["Basic"];
        }

        // Создание новой стратегии
        private static IAttackStrategy CreateNewStrategy(string strategyName, StrategyConfig config = null)
        {
            switch (strategyName.ToLower())
            {
                case "basic":
                    return new BasicAttackStrategy();

                case "heal":
                    var healStrategy = new HealStrategy();
                    if (config != null)
                    {
                        // Применяем настройки из конфига
                        healStrategy.baseHealAmount = config.healAmount;
                        healStrategy.maxTargets = config.healMaxTargets;
                        healStrategy.canHealSelf = config.healCanTargetSelf;
                        healStrategy.healInterval = config.healInterval;
                    }
                    return healStrategy;

                case "backrowaoe":
                    var aoeStrategy = new BackRowAoEAttackStrategy();
                    if (config != null)
                    {
                        aoeStrategy.aoeDamageMultiplier = config.aoeDamageMultiplier;
                        aoeStrategy.timeBetweenShots = config.aoeTimeBetweenShots;
                        // Можно добавить maxTargets если нужно
                    }
                    return aoeStrategy;

                case "backrow":
                    return new BackRowAttackStrategy();

                case "lowesthealth":
                    return new LowestHealthAttackStrategy();
                case "fullaoe":
                    return new FullAoEStrategy();

                default:
                    Debug.LogWarning($"⚠ Unknown strategy: {strategyName}");
                    return null;
            }
        }

        // Очистка кэша
        public static void ClearCache()
        {
            strategyCache.Clear();
            Debug.Log(" StrategyFactory cache cleared");
        }

        // Получить список всех доступных стратегий
        public static List<string> GetAvailableStrategyNames()
        {
            return new List<string>
            {
                "Basic",
                "BackRowAoE",
                "BackRow",
                "LowestHealth"
            };
        }
    }
}