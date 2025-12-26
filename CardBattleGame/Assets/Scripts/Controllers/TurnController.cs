using CardGame.Models;
using CardGame.Strategies;
using CardGame.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CardGame.Controllers
{
    public class TurnController : MonoBehaviour
    {
        private List<Card> turnOrder = new List<Card>();
        private int currentTurnIndex = 0;
        private int currentRound = 1;
        public int maxRounds = 30;
        private bool isProcessing = false;
        private bool isWaitingForActionToComplete = false;

        public int CurrentRound => currentRound;
        public bool IsProcessing => isProcessing;

        public System.Action<int> OnRoundChanged;

        void Start()
        {
            Debug.Log("TurnController Started");
        }

        public void StartBattle(int rounds = 30)
        {
            Debug.Log(" StartBattle called");

            maxRounds = rounds;
            currentRound = 1;
            turnOrder.Clear();
            currentTurnIndex = 0;
            isProcessing = true;
            isWaitingForActionToComplete = false;

            Debug.Log($" Начало битвы - Макс раундов: {maxRounds}");

            OnRoundChanged?.Invoke(currentRound);

            CalculateTurnOrder();
            ProcessNextTurn();
        }

        public void StopBattle()
        {
            isProcessing = false;
            isWaitingForActionToComplete = false;
            turnOrder.Clear();
            Debug.Log(" Битва остановлена");

            CancelInvoke("ProcessNextTurn");
            CancelInvoke("CompleteAction");
        }

        public bool EndRound()
        {
            Debug.Log($" Конец раунда {currentRound}");

            currentRound++;
            currentTurnIndex = 0;

            OnRoundChanged?.Invoke(currentRound);

            Debug.Log($" Начинается раунд {currentRound}");

            if (currentRound > maxRounds)
            {
                Debug.Log($" Достигнут лимит раундов ({maxRounds})!");
                EndBattle();
                return false;
            }

            if (CheckBattleEnd())
            {
                EndBattle();
                return false;
            }

            CalculateTurnOrder();

            Invoke("ProcessNextTurn", 1f);
            return true;
        }

        public void ResetRounds()
        {
            currentRound = 1;
            Debug.Log(" Сброс счетчика раундов");
        }
        private void CalculateTurnOrder()
        {
            Debug.Log(" Расчет порядка ходов...");

            var allCards = GameController.Instance.Board.GetAllCards(true)
                                .Concat(GameController.Instance.Board.GetAllCards(false))
                                .Where(card => card != null && card.IsAlive && card.enabled)
                                .ToList();

            Debug.Log($" Найдено {allCards.Count} живых карт");

            if (allCards.Count == 0)
            {
                Debug.LogError("Нет живых карт!");
                return;
            }

            turnOrder = allCards.OrderByDescending(card => card.stats.speed)
                               .ThenBy(card => Random.Range(0, 100))
                               .ToList();

            Debug.Log($" Порядок ходов на раунд {currentRound}:");
            for (int i = 0; i < turnOrder.Count; i++)
            {
                var card = turnOrder[i];
                Debug.Log($"   {i + 1}. {card.cardName} (Скорость: {card.stats.speed}, Тип: {card.cardType})");
            }
        }
        private void ProcessNextTurn()
        {
            if (!isProcessing || isWaitingForActionToComplete)
            {
                return;
            }

            while (currentTurnIndex < turnOrder.Count &&
                   (turnOrder[currentTurnIndex] == null ||
                    !turnOrder[currentTurnIndex].IsAlive ||
                    !turnOrder[currentTurnIndex].enabled))
            {
                Debug.Log($" Пропуск мертвой карты: {turnOrder[currentTurnIndex]?.cardName}");
                currentTurnIndex++;
            }

            if (currentTurnIndex >= turnOrder.Count)
            {
                Debug.Log(" Конец раунда достигнут");
                EndRound();
                return;
            }

            var currentCard = turnOrder[currentTurnIndex];
            if (currentCard == null)
            {
                currentTurnIndex++;
                Invoke("ProcessNextTurn", 0.1f);
                return;
            }

            Debug.Log($"\n=== РАУНД {currentRound} | ХОД {currentTurnIndex + 1}/{turnOrder.Count}: {currentCard.cardName} ===");

            bool isHealer = IsCardHealer(currentCard);

            if (isHealer)
            {
                ProcessHealerTurn(currentCard);
            }
            else
            {
                ProcessAttackerTurn(currentCard);
            }
        }

        private bool IsCardHealer(Card card)
        {
            if (card.cardType == CardType.Heal)
                return true;
            return false;
        }

        private void ProcessHealerTurn(Card healer)
        {
           var targets = FindTargets(healer, true);
            if (targets != null)
            {
                
                isWaitingForActionToComplete = true;
                ExecuteHeal(healer, targets);
            }
        }

        private void ProcessAttackerTurn(Card attacker)
        {
            var target = FindTargets(attacker, false);

            if (target != null)
            {
                isWaitingForActionToComplete = true;
                ExecuteAttack(attacker, target);
            }
            else
            {
                Debug.Log($" {attacker.cardName} не нашел целей для атаки");
                currentTurnIndex++;
                Invoke("ProcessNextTurn", 0.3f);
            }
        }
        private List<Card> FindTargets(Card card, bool isHealer)
        {
            if (card == null) return null;

            bool isPlayerAttacker = card.position.isPlayerSide;
            bool targetPlayerSide = isHealer ? isPlayerAttacker : !isPlayerAttacker;

            var allTargets = GameController.Instance.Board.GetAllCards(targetPlayerSide)
                                .Where(target => target != null && target.IsAlive && target.enabled)
                                .ToList();

            if (allTargets.Count == 0) return null;
            return allTargets;
        }

        private void ExecuteHeal(Card healer, List<Card> targets)
        {
            if (healer == null || targets == null)
            {
                CompleteAction();
                return;
            }

            var strategyComponent = healer.GetComponent<CardStrategyComponent>();
            var strategy = strategyComponent.GetCurrentStrategy();
            strategy.Execute(healer, targets);

            Invoke("CompleteAction", 1.0f);
        }

        private void ExecuteAttack(Card attacker, List<Card> targets)
        {
            if (attacker == null)
            {
                CompleteAction();
                return;
            }

            SimpleAttackController simpleAttackController = attacker.GetComponent<SimpleAttackController>();
            if (simpleAttackController != null)
            {
                simpleAttackController.ExecuteAttack();
                Invoke("CompleteAction", 0.8f);
            }
            else
            {
                Card target = targets.FirstOrDefault();
                UseBasicAttack(attacker, target);
            }
            
        }

        private void UseBasicAttack(Card attacker, Card target)
        {
            CardShooter shooter = attacker.GetComponent<CardShooter>();
            bool isCrit = DamageCalculator.CalculateCritChance(attacker, target);
            int damage = DamageCalculator.CalculateDamage(attacker, target, isCrit);

            if (shooter != null && shooter.projectilePrefab != null)
            {
                shooter.ShootAt(target, damage, isCrit);
            }
            else
            {
                target.TakeDamage(damage, isCrit, attacker);
            }

            Invoke("CompleteAction", 0.5f);
        }


        private void CompleteAction()
        {
            isWaitingForActionToComplete = false;
            currentTurnIndex++;

            if (CheckBattleEnd())
            {
                EndBattle();
                return;
            }

            Invoke("ProcessNextTurn", 0.3f);
        }

        private bool CheckBattleEnd()
        {
            bool playerHasCards = GameController.Instance.Board.GetAllCards(true)
                                        .Any(card => card != null && card.IsAlive && card.enabled);

            bool enemyHasCards = GameController.Instance.Board.GetAllCards(false)
                                        .Any(card => card != null && card.IsAlive && card.enabled);

            return !playerHasCards || !enemyHasCards;
        }

        private void EndBattle()
        {
            isProcessing = false;
            isWaitingForActionToComplete = false;
            turnOrder.Clear();

            CancelInvoke("ProcessNextTurn");
            CancelInvoke("CompleteAction");

            bool playerWins = GameController.Instance.Board.GetAllCards(true)
                                    .Any(card => card != null && card.IsAlive && card.enabled);

            bool enemyWins = GameController.Instance.Board.GetAllCards(false)
                                    .Any(card => card != null && card.IsAlive && card.enabled);

            if (playerWins && !enemyWins)
            {
                Debug.Log("\n === ПОБЕДА ИГРОКА! ===");
            }
            else if (!playerWins && enemyWins)
            {
                Debug.Log("\n === ПОБЕДА ПРОТИВНИКА! ===");
            }
            else
            {
                Debug.Log("\n== НИЧЬЯ! ===");
            }

            Debug.Log($" Битва завершена на раунде {currentRound}");
        }

        void OnDestroy()
        {
            StopBattle();
        }
    }
}