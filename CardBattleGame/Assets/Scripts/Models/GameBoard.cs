using CardGame.Controllers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CardGame.Models
{
    public class GameBoard : MonoBehaviour
    {
        [Header("UI References")]
        public Transform folderHeroP1;
        public Transform folderHeroP2;
        public Transform contentP1;
        public Transform contentP2;

        [Header("Slot Controller")]
        public SlotController slotController;

        // Хранилище карт через слоты
        private Dictionary<BoardPosition, Card> cards = new Dictionary<BoardPosition, Card>();

        public void Initialize()
        {
            cards.Clear();
            slotController.Initialize();
        }

        public bool PlaceCard(Card card, BoardPosition position)
        {
            if (!position.IsValid())
            {
                Debug.LogError($"Invalid position: {position}");
                return false;
            }

            if (cards.ContainsKey(position))
            {
                Debug.LogError($"Position {position} is already occupied by {cards[position].cardName}");
                return false;
            }

            // Размещаем карту через SlotController
            if (slotController != null && slotController.PlaceCardInSlot(card, position))
            {
                cards[position] = card;
                card.SetPosition(position);
                Debug.Log($"Card {card.cardName} placed at {position}");
                return true;
            }

            return false;
        }

        // ⭐ ОСНОВНОЙ МЕТОД: Получить карты конкретного ряда
        public List<Card> GetRowCards(bool isPlayerSide, int row)
        {
            var rowCards = new List<Card>();

            if (slotController != null)
            {
                var slots = slotController.GetSlotsForSide(isPlayerSide);

                if (slots == null)
                {
                    Debug.LogError($"GetSlotsForSide returned null for side: {(isPlayerSide ? "Player" : "Enemy")}");
                    return rowCards;
                }

                foreach (var slot in slots)
                {
                    if (slot.position.row == row &&
                        slot.IsOccupied &&
                        slot.currentCard != null &&
                        slot.currentCard.IsAlive &&
                        slot.currentCard.enabled)
                    {
                        rowCards.Add(slot.currentCard);
                    }
                }
            }
            else
            {
                Debug.LogError("SlotController is null!");
            }
            return rowCards;
        }
        public List<Card> GetFrontRowCards(bool isPlayerSide)
        {
            // Для игрока: передний ряд = 1 (ближе к врагу)
            // Для врага: передний ряд = 0 (ближе к игроку)
            int frontRow = isPlayerSide ? 1 : 0;
            return GetRowCards(isPlayerSide, frontRow);
        }
        public List<Card> GetBackRowCards(bool isPlayerSide)
        {
            // Для игрока: задний ряд = 0 (дальше от врага)
            // Для врага: задний ряд = 1 (дальше от игрока)
            int backRow = isPlayerSide ? 0 : 1;
            return GetRowCards(isPlayerSide, backRow);
        }

        // Получить все карты стороны
        public List<Card> GetAllCards(bool isPlayerSide)
        {
            var allCards = new List<Card>();

            allCards.AddRange(GetFrontRowCards(isPlayerSide));
            allCards.AddRange(GetBackRowCards(isPlayerSide));

            Debug.Log($"GetAllCards({(isPlayerSide ? "Player" : "Enemy")}): found {allCards.Count} cards");
            return allCards;
        }

        public Card GetCardAtPositionSimple(BoardPosition position)
        {
            if (cards.TryGetValue(position, out Card card))
            {
                return card;
            }
            return null;
        }

        public BoardPosition? GetFreePosition(bool isPlayerSide)
        {
            if (slotController != null)
            {
                return slotController.GetFreePosition(isPlayerSide);
            }
            return null;
        }

        public void RemoveCard(Card card)
        {
            // Удаляем из словаря
            BoardPosition? positionToRemove = null;
            foreach (var kvp in cards)
            {
                if (kvp.Value == card)
                {
                    positionToRemove = kvp.Key;
                    break;
                }
            }

            if (positionToRemove.HasValue && cards.ContainsKey(positionToRemove.Value))
            {
                cards.Remove(positionToRemove.Value);
            }

            // Удаляем из слота
            if (slotController != null)
            {
                slotController.RemoveCardFromSlot(card);
            }
        }

      
        public Card GetRandomBackRowCard(bool isPlayerSide)
        {
            var backRowCards = GetBackRowCards(isPlayerSide);
            if (backRowCards.Count > 0)
            {
                return backRowCards[Random.Range(0, backRowCards.Count)];
            }
            return null;
        }

        // Проверить есть ли живые карты в заднем ряду
        public bool HasAliveCardsInBackRow(bool isPlayerSide)
        {
            return GetBackRowCards(isPlayerSide).Count > 0;
        }

        // Получить самую слабую карту в заднем ряду
        public Card GetWeakestBackRowCard(bool isPlayerSide)
        {
            var backRowCards = GetBackRowCards(isPlayerSide);
            if (backRowCards.Count == 0) return null;

            return backRowCards.OrderBy(c => c.stats.health).FirstOrDefault();
        }

        // Получить самую сильную карту в заднем ряду
        public Card GetStrongestBackRowCard(bool isPlayerSide)
        {
            var backRowCards = GetBackRowCards(isPlayerSide);
            if (backRowCards.Count == 0) return null;

            return backRowCards.OrderByDescending(c => c.stats.attack).FirstOrDefault();
        }

     
        public void DebugAllRows()
        {
            Debug.Log("=== 🧪 GAMEBOARD DEBUG ===");

            Debug.Log("PLAYER:");
            Debug.Log($"  Front Row (row 1): {GetRowCards(true, 1).Count} cards");
            Debug.Log($"  Back Row (row 0): {GetRowCards(true, 0).Count} cards");

            Debug.Log("ENEMY:");
            Debug.Log($"  Front Row (row 0): {GetRowCards(false, 0).Count} cards");
            Debug.Log($"  Back Row (row 1): {GetRowCards(false, 1).Count} cards");

            Debug.Log("=== END DEBUG ===");
        }

        public void DebugStrategyTargets(Card attacker, string strategyName)
        {
            if (attacker == null) return;

            bool isPlayerAttacker = attacker.position.isPlayerSide;
            bool targetSide = !isPlayerAttacker;

            Debug.Log($"=== 🎯 {strategyName} DEBUG for {attacker.cardName} ===");
            Debug.Log($"Attacker: {(isPlayerAttacker ? "Player" : "Enemy")} at [{attacker.position.row},{attacker.position.column}]");
            Debug.Log($"Target side: {(targetSide ? "Player" : "Enemy")}");

            int frontRow = isPlayerAttacker ? 0 : 1;
            int backRow = isPlayerAttacker ? 1 : 0;

            var frontTargets = GetRowCards(targetSide, frontRow);
            var backTargets = GetRowCards(targetSide, backRow);

            Debug.Log($"Front row targets (row {frontRow}): {frontTargets.Count}");
            foreach (var card in frontTargets)
            {
                Debug.Log($"  - {card.cardName} (HP: {card.stats.health}) at [{card.position.row},{card.position.column}]");
            }

            Debug.Log($"Back row targets (row {backRow}): {backTargets.Count}");
            foreach (var card in backTargets)
            {
                Debug.Log($"  - {card.cardName} (HP: {card.stats.health}) at [{card.position.row},{card.position.column}]");
            }

            Debug.Log($"=== END DEBUG ===");
        }
    }
}