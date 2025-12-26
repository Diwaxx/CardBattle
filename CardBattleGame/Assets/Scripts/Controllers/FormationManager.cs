using CardGame.Models;
using CardGame.Strategies;
using CardGame.Views;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardGame.Controllers
{
    public class FormationManager : MonoBehaviour
    {
        [Header("References")]
        public GridManager gridManager;
        public GameController gameController;

        [Header("Card Mapping")]
        public GameObject warriorCardPrefab;
        public GameObject tankCardPrefab;
        public GameObject healerCardPrefab;
        public GameObject archerCardPrefab;
        public GameObject mageCardPrefab;

        [Header("UI")]
        public UnityEngine.UI.Button startBattleButton;

        private bool isInitialized = false;
        private int lastHeroCount = -1;

        void Start()
        {
            StartCoroutine(InitializeWithDelay());
        }

        private IEnumerator InitializeWithDelay()
        {
            yield return null;
            InitializeFormationManager();
            yield return null;
            UpdateFormationStatus();
            isInitialized = true;
        }

        private void InitializeFormationManager()
        {
            if (gridManager == null)
            {
                gridManager = FindFirstObjectByType<GridManager>();
                if (gridManager == null)
                {
                    Debug.LogError("❌ GridManager не найден в сцене!");
                    return;
                }
            }

            if (gameController == null)
            {
                gameController = GameController.Instance;
                if (gameController == null)
                {
                    Debug.LogError("❌ GameController не найден!");
                }
            }

            if (startBattleButton != null)
            {
                startBattleButton.onClick.RemoveAllListeners();
                startBattleButton.onClick.AddListener(StartBattleFromFormation);
                Debug.Log("✅ Кнопка запуска битвы настроена");
            }

            Debug.Log("✅ FormationManager initialized");
        }

        void Update()
        {
            if (!isInitialized || gridManager == null) return;

            // Проверяем изменения каждые 15 кадров
            if (Time.frameCount % 15 == 0)
            {
                int currentHeroCount = CountHeroesInSlots();

                if (currentHeroCount != lastHeroCount)
                {
                    lastHeroCount = currentHeroCount;
                    UpdateFormationStatus();
                    Debug.Log($"🔄 Обновлен статус: {currentHeroCount} героев");
                }
            }
        }

        private int CountHeroesInSlots()
        {
            if (gridManager == null || gridManager.slots == null) return 0;

            int count = 0;
            foreach (var slot in gridManager.slots)
            {
                if (slot != null && slot.GetCurrentCard() != null)
                {
                    count++;
                }
            }
            return count;
        }

        public void UpdateFormationStatus()
        {
            int heroCount = CountHeroesInSlots();
            bool isValid = heroCount >= 1;
        }

        // ОСНОВНОЙ МЕТОД
        public void StartBattleFromFormation()
        {
            if (!IsFormationValid())
            {
                Debug.LogError("Нельзя начать битву: нет героев в формации!");
                return;
            }

            Debug.Log("=== ЗАПУСК БИТВЫ С ГЕРОЯМИ ИЗ ФОРМАЦИИ ===");

            // Получаем героев 
            var playerFormation = GetPlayerFormationFromGrid();

            if (playerFormation.Count == 0)
            {
                Debug.LogError("❌ Нет героев для битвы!");
                return;
            }

          
            CreatePlayerCardsOnBattlefield(playerFormation);

            if (gameController != null)
            {
                StartCoroutine(DelayedBattleStart());
            }
            else
            {
                Debug.LogError("GameController не найден!");
            }
        }

        private IEnumerator DelayedBattleStart()
        {
            yield return new WaitForSeconds(0.5f);
            gameController.StartAutoBattle();
        }

        // Получение формации игрока из GridManager
        private List<HeroSlotInfo> GetPlayerFormationFromGrid()
        {
            List<HeroSlotInfo> formation = new List<HeroSlotInfo>();

            if (gridManager == null || gridManager.slots == null)
            {
                Debug.LogError("GridManager не инициализирован!");
                return formation;
            }

            Debug.Log($" Поиск героев в {gridManager.slots.Count} слотах...");

            for (int i = 0; i < gridManager.slots.Count; i++)
            {
                var slot = gridManager.slots[i];
                if (slot != null)
                {
                    GameObject hero = slot.GetCurrentCard();
                    if (hero != null)
                    {
                        Debug.Log($"Найден герой в слоте {i}: {hero.name}");

                        // Определяем позицию на поле боя
                        BoardPosition boardPosition = ConvertGridIndexToBoardPosition(i);

                        formation.Add(new HeroSlotInfo
                        {
                            slotIndex = i,
                            hero = hero,
                            heroName = hero.name,
                            boardPosition = boardPosition,
                        });

                        Debug.Log($"   → Позиция: {boardPosition}");
                    }
                }
            }

            Debug.Log($"✅ Найдено {formation.Count} героев для битвы");
            return formation;
        }
        private BoardPosition ConvertGridIndexToBoardPosition(int slotIndex)
        {
            Dictionary<int, (int row, int col)> positionMap = new Dictionary<int, (int, int)>
            {
              
                { 0, (0, 0) }, 
                { 2, (0, 1) }, 
                { 4, (0, 2) }, 
                { 1, (1, 0) }, 
                { 3, (1, 1) }, 
                { 5, (1, 2) }  
            };

            if (positionMap.TryGetValue(slotIndex, out var position))
            {
                return new BoardPosition(position.row, position.col, true);
            }
            return new BoardPosition(0, 0, true);
        }
        private void CreatePlayerCardsOnBattlefield(List<HeroSlotInfo> playerFormation)
        {

            Debug.Log($" Создание {playerFormation.Count} карт игрока на поле боя...");

            ClearExistingPlayerCards();

            // Создаем новые карты
            foreach (var heroInfo in playerFormation)
            {
                CreatePlayerCard(heroInfo.boardPosition, heroInfo.cardType, heroInfo.heroName);
            }

            Debug.Log($" Карты игрока созданы: {playerFormation.Count} героев вступили в битву!");
        }

        private void ClearExistingPlayerCards()
        {
            if (gameController == null || gameController.Board == null) return;

            var playerCards = gameController.Board.GetAllCards(true);
            int removedCount = 0;

            foreach (var card in playerCards)
            {
                if (card != null && card.gameObject != null)
                {
                    Destroy(card.gameObject);
                    removedCount++;
                }
            }

            if (removedCount > 0)
            {
                Debug.Log($"🗑️ Удалено {removedCount} старых карт игрока");
            }
        }

        private void CreatePlayerCard(BoardPosition position, CardType cardType, string name)
        {
            GameObject prefab = GetPrefabByCardName(name);
            if (prefab == null)
            {
                Debug.LogError($"Не найден префаб для типа {cardType}");
                return;
            }

            Debug.Log($"Создание карты: {name} ({cardType}) на позиции {position}");

            // Создаем карту
            var cardObject = Instantiate(prefab);
            cardObject.name = name;
            var cardView = cardObject.GetComponent<SimpleCardView>();
          

            var card = cardObject.GetComponent<Card>();
            if (cardView != null)
            {
                cardView.Initialize(card, position.isPlayerSide);
            }
            if (card != null)
            {
                card.cardName = name;
                card.cardType = cardType;

                // Добавляем компоненты для битвы
                AddBattleComponents(cardObject, cardType);

                // Размещаем на поле
                if (gameController.Board != null)
                {
                    bool placed = gameController.Board.PlaceCard(card, position);
                    if (placed)
                    {
                        Debug.Log($" Карта размещена: {name} на {position}");

             
                    }
                    else
                    {
                        Debug.LogError($" Не удалось разместить карту {name} на позиции {position}");
               
                        Destroy(cardObject);
                    }
                }
                else
                {
                    Debug.LogError(" GameBoard не найден!");
                    Destroy(cardObject);
                }
            }
            else
            {
                Debug.LogError($" Не удалось получить компонент Card у префаба");
                Destroy(cardObject);
            }
        }

        private GameObject GetPrefabByCardName(string name)
        {
           
            return name switch
            {
                "CardPrime" => warriorCardPrefab,
                "CardPrime 1" => healerCardPrefab,
                "CardPrime 2" => mageCardPrefab,

                _ => throw new System.NotImplementedException()
            };
        }


        private void AddBattleComponents(GameObject cardObject, CardType cardType)
        {
            if (cardObject.GetComponent<CardShooter>() == null)
            {
                var shooter = cardObject.AddComponent<CardShooter>();
                if (gameController != null)
                {
                    shooter.projectilePrefab = gameController.projectilePrefab;
                }
            }
            if (cardObject.GetComponent<SimpleAttackController>() == null)
            {
                cardObject.AddComponent<SimpleAttackController>();
            }
        }

        private bool IsFormationValid()
        {
            return CountHeroesInSlots() >= 1;
        }

        // Класс для хранения информации о герое
        public class HeroSlotInfo
        {
            public int slotIndex;
            public GameObject hero;
            public BoardPosition boardPosition;
            public CardType cardType;
            public string heroName;
        }
    }
}