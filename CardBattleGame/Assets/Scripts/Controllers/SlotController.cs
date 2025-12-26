using UnityEngine;
using System.Collections.Generic;
using CardGame.Models;

namespace CardGame.Controllers
{
    public class SlotController : MonoBehaviour
    {
        [Header("Slot Prefab")]
        public GameObject slotPrefab;

        [Header("Layout Settings")]
        public float slotWidth = 150f;
        public float slotHeight = 200f;
        public float horizontalSpacing = 15f;
        public float verticalSpacing = 15f;
        public Vector2 startPosition = new Vector2(50f, 20f);

        private Dictionary<BoardPosition, SlotInfo> allSlots = new Dictionary<BoardPosition, SlotInfo>();

        public void Initialize()
        {
            CreateAllSlots();
            Debug.Log($"SlotController initialized with {allSlots.Count} slots");
        }

        private void CreateAllSlots()
        {
            allSlots.Clear();

            // Создаем слоты для игрока (2 ряда × 3 колонки)
            CreateSlotsForSide(true);
            // Создаем слоты для противника (2 ряда × 3 колонки)
            CreateSlotsForSide(false);
        }

        private void CreateSlotsForSide(bool isPlayerSide)
        {
            Transform parent = isPlayerSide ? GameController.Instance.Board.contentP1 : GameController.Instance.Board.contentP2;
            if (parent == null)
            {
                Debug.LogError($"Parent not found for {(isPlayerSide ? "Player" : "Enemy")} side");
                return;
            }

            // Очищаем старые слоты
            foreach (Transform child in parent)
            {
                Destroy(child.gameObject);
            }

            // Создаем слоты для 2 рядов × 3 колонки
            for (int row = 0; row < 2; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    CreateSlot(parent, row, col, isPlayerSide);
                }
            }
        }

        private void CreateSlot(Transform parent, int row, int col, bool isPlayerSide)
        {
            if (slotPrefab == null)
            {
                Debug.LogError("Slot prefab is not assigned!");
                return;
            }

            var slotObject = Instantiate(slotPrefab, parent);
            var position = new BoardPosition(row, col, isPlayerSide);

            slotObject.name = $"Slot_{(isPlayerSide ? "P" : "E")}_{row}_{col}";

            // Настраиваем RectTransform
            RectTransform rect = slotObject.GetComponent<RectTransform>();

            // Правильный расчет позиции
            float xPos = startPosition.x + col * (slotWidth + horizontalSpacing);
            float yPos = startPosition.y - row * (slotHeight + verticalSpacing);

            rect.anchoredPosition = new Vector2(xPos, yPos);
            rect.sizeDelta = new Vector2(slotWidth, slotHeight);
            rect.localScale = Vector3.one;

            // Добавляем SlotInfo компонент
            var slotInfo = slotObject.AddComponent<SlotInfo>();
            slotInfo.position = position;

            // Сохраняем в словарь
            allSlots[position] = slotInfo;

            Debug.Log($"Created slot: {slotObject.name} at ({xPos:F0}, {yPos:F0})");
        }
        public SlotInfo GetSlot(BoardPosition position)
        {
            if (allSlots.ContainsKey(position))
            {
                return allSlots[position];
            }
            return null;
        }

        public bool PlaceCardInSlot(Card card, BoardPosition position)
        {
            var slot = GetSlot(position);
            if (slot == null)
            {
                Debug.LogError($"Slot not found for position: {position}");
                return false;
            }

            if (slot.IsOccupied)
            {
                Debug.LogError($"Slot at {position} is already occupied by {slot.currentCard?.cardName}");
                return false;
            }

            // Убедимся что карта в правильном родителе
            Transform slotTransform = slot.transform;
            card.transform.SetParent(slotTransform, false);

            // Правильно настраиваем RectTransform карты
            RectTransform cardRect = card.GetComponent<RectTransform>();
            if (cardRect != null)
            {
                // Растягиваем карту на весь слот
                cardRect.anchorMin = new Vector2(0, 0);
                cardRect.anchorMax = new Vector2(1, 1);
                cardRect.pivot = new Vector2(0.5f, 0.5f);

                // Сбрасываем offsets чтобы заполнить весь родителя
                cardRect.offsetMin = Vector2.zero;
                cardRect.offsetMax = Vector2.zero;

                cardRect.anchoredPosition = Vector2.zero;
                cardRect.localScale = Vector3.one;

                Debug.Log($"Card rect: anchors({cardRect.anchorMin}-{cardRect.anchorMax}), pos({cardRect.anchoredPosition})");
            }

            slot.PlaceCard(card);
            Debug.Log($"Card {card.cardName} placed in slot at {position}");
            return true;
        }

        public void RemoveCardFromSlot(Card card)
        {
            foreach (var slot in allSlots.Values)
            {
                if (slot.currentCard == card)
                {
                    slot.RemoveCard();
                    Debug.Log($"Card {card.cardName} removed from slot at {slot.position}");
                    return;
                }
            }
        }

        public BoardPosition? GetFreePosition(bool isPlayerSide)
        {
            foreach (var slot in allSlots.Values)
            {
                if (slot.position.isPlayerSide == isPlayerSide && !slot.IsOccupied)
                {
                    return slot.position;
                }
            }
            return null;
        }

        public List<SlotInfo> GetSlotsForSide(bool isPlayerSide)
        {
            var slots = new List<SlotInfo>();
            foreach (var slot in allSlots.Values)
            {
                if (slot.position.isPlayerSide == isPlayerSide)
                {
                    slots.Add(slot);
                }
            }
            return slots;
        }


        public List<SlotInfo> GetFrontRowSlots(bool isPlayerSide)
        {
            var slots = new List<SlotInfo>();
            foreach (var slot in allSlots.Values)
            {
                if (slot.position.isPlayerSide == isPlayerSide && slot.position.row == 0)
                {
                    slots.Add(slot);
                }
            }
            return slots;
        }
    }
}