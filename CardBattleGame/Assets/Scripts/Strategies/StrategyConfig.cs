// StrategyConfig.cs
using System.Collections.Generic;
using UnityEngine;

namespace CardGame.Strategies
{
    [CreateAssetMenu(fileName = "StrategyConfig", menuName = "Card Game/Strategy Config")]
    public class StrategyConfig : ScriptableObject
    {
        [System.Serializable]
        public class StrategyWeight
        {
            public string strategyName;
            public int weight = 1;
            public int usesBeforeSwitch = 1;
        }

        [Header("Доступные стратегии")]
        public List<StrategyWeight> availableStrategies = new List<StrategyWeight>
        {
            new StrategyWeight { strategyName = "Basic", weight = 10, usesBeforeSwitch = 1 }
        };

        [Header("Настройки стратегий")]

        // Настройки для HealStrategy
        public int healAmount = 20;
        public int healMaxTargets = 2;
        public bool healCanTargetSelf = true;
        public float healInterval = 0.3f;

        // Настройки для BackRowAoEAttackStrategy
        public float aoeDamageMultiplier = 0.7f;
        public float aoeTimeBetweenShots = 0.15f;
        public int aoeMaxTargets = 3;

        // Общие настройки
        public bool canAttackBackRow = false;
        public float baseDamageMultiplier = 1.0f;
    }
}