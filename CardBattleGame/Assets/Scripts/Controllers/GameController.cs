
using CardGame.Models;
using CardGame.Strategies;
using CardGame.Views;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CardGame.Controllers
{
    public class GameController : MonoBehaviour
    {
        [Header("2D Projectile Settings")]
        public GameObject projectilePrefab; 


        [Header("Game References")]
        public GameBoard gameBoard;

        [Header("Card Prefabs")]
        public GameObject warriorCardPrefab;
        public GameObject damagerCardPrefab;
        public GameObject healerCardPrefab;

        [Header("Settings")]
        public bool autoCreateTestCards = true;
        public int autoBattleRounds = 30;

        private AutoBattleController autoBattleController;
        private List<Card> allCards = new List<Card>();

        // Input Actions
        private InputAction autoBattleAction;
        private InputAction testBattleAction;
        private InputAction resetAction;
        private InputAction addCardAction;
        private InputAction healAction;
        private InputAction statusAction;
        private InputAction testProjectile;
        private InputAction testBackRowAttack;
        public static GameController Instance { get; private set; }
        public GameBoard Board => gameBoard;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            StrategyFactory.Initialize();
            SetupInput();
            InitializeGame();
        }
        public int GetCurrentRound()
        {
            var turnController = FindObjectOfType<TurnController>();
            return turnController != null ? turnController.CurrentRound : 1;
        }
        private void SetupInput()
        {
            // Создаем Input Actions программно
            autoBattleAction = new InputAction("AutoBattle", InputActionType.Button, "<Keyboard>/a");
            testBattleAction = new InputAction("TestBattle", InputActionType.Button, "<Keyboard>/space");
            resetAction = new InputAction("Reset", InputActionType.Button, "<Keyboard>/r");
            addCardAction = new InputAction("AddCard", InputActionType.Button, "<Keyboard>/t");
            testProjectile = new InputAction("AddCard", InputActionType.Button, "<Keyboard>/p");
            healAction = new InputAction("Heal", InputActionType.Button, "<Keyboard>/1");
            statusAction = new InputAction("Status", InputActionType.Button, "<Keyboard>/s");
            testBackRowAttack = new InputAction("TestBackRow", InputActionType.Button, "<Keyboard>/e");

            // Упрощенные действия для скорости (только + и -)
            var increaseSpeedAction = new InputAction("IncreaseSpeed", InputActionType.Button, "<Keyboard>/plus");
            var decreaseSpeedAction = new InputAction("DecreaseSpeed", InputActionType.Button, "<Keyboard>/minus");


            increaseSpeedAction.Enable();
            decreaseSpeedAction.Enable();
            // Подписываемся на события
            testProjectile.performed += ctx => TestProjectileManual();
            autoBattleAction.performed += ctx => StartAutoBattle();
            testBattleAction.performed += ctx => TestBattle();
            resetAction.performed += ctx => ResetGame();
            addCardAction.performed += ctx => AddNewRandomCard();
            healAction.performed += ctx => TestHealing();
           
            testBackRowAttack.performed += ctx => TestBackRowImplementation();

            // Включаем действия
            autoBattleAction.Enable();
            testProjectile.Enable();
            testBattleAction.Enable();
            resetAction.Enable();
            addCardAction.Enable();
            healAction.Enable();
            statusAction.Enable();
            testBackRowAttack.Enable();

            Debug.Log("Input System initialized");
        }

        private void InitializeGame()
        {
            Debug.Log("Initializing game");

            if (gameBoard != null)
            {
                gameBoard.Initialize();

                if (autoCreateTestCards)
                {
                    StartCoroutine(SetupTestCardsWithDelay());
                }
            }
            else
            {
                Debug.LogError("GameBoard not assigned");
            }
        }
        private IEnumerator SetupTestCardsWithDelay()
        {
            yield return null;
            SetupTestCards();
        }

        private void SetupTestCards()
        {
            Debug.Log("Creating test cards");

            ClearAllCards();
            //Создание карт врагов
            CreateTestCard(new BoardPosition(0, 0, false), "Enemy_Slow", warriorCardPrefab);
            CreateTestCard(new BoardPosition(1, 1, false), "Enemy_Slow", warriorCardPrefab);
            CreateTestCard(new BoardPosition(0, 2, false), "Enemy_Slow", warriorCardPrefab);
            CreateTestCard(new BoardPosition(1, 2, false), "Enemy_Slow", warriorCardPrefab);
            CreateTestCard(new BoardPosition(0, 1, false), "Enemy_Fast", warriorCardPrefab);
            CreateTestCard(new BoardPosition(1, 0, false), "Enemy_Standard", healerCardPrefab);

            Debug.Log("Test cards created. Controls: A=AutoBattle, Space=Test, R=Reset, T=AddCard, 1=Heal, S=Status");
        }

        // В методе CreateTestCard добавьте вызов:
        private void CreateTestCard(BoardPosition position, string name, GameObject type)
        {
            Debug.Log($"Creating card: {name} at {position}");

            if (warriorCardPrefab == null)
            {
                Debug.LogError("No card prefab assigned");
                return;
            }

            var cardObject = Instantiate(type);
            cardObject.name = name;

            var card = cardObject.GetComponent<Card>();
            if (card == null)
            {
                Debug.LogError("Card component not found");
                Destroy(cardObject);
                return;
            }

            card.cardName = name;
            //ConfigureCardByType(card, name);
            card.enabled = true;

           
            AddShooterToCard(cardObject, name);

            var cardView = cardObject.GetComponent<SimpleCardView>();
            if (cardView != null)
            {
                cardView.Initialize(card, position.isPlayerSide);
            }

            if (gameBoard.PlaceCard(card, position))
            {
                allCards.Add(card);
                Debug.Log($"✓ Card created: {name} at {position}");
            }
            else
            {
                Debug.LogError($"✗ Failed to place card: {name}");
                Destroy(cardObject);
            }
        }
        private void AddShooterToCard(GameObject cardObject, string cardName)
        {
            if (projectilePrefab == null)
            {
                Debug.LogWarning($"⚠ No projectile prefab for {cardName}");
                return;
            }

            // Добавляем CardShooter
            CardShooter shooter = cardObject.AddComponent<CardShooter>();
            shooter.projectilePrefab = projectilePrefab;
            AddSimpleAttackController(cardObject, cardName);
            

            Debug.Log($"✅ Добавлены системы для {cardName}");
        }


        void TestProjectileManual()
        {
            Debug.Log("=== 🧪 MANUAL TEST ===");

            var playerCards = gameBoard.GetAllCards(true);
            var enemyCards = gameBoard.GetAllCards(false);

            if (playerCards.Count > 0 && enemyCards.Count > 0)
            {
                var attacker = playerCards[0];
                var target = enemyCards[0];

                Debug.Log($"Test: {attacker.cardName} -> {target.cardName}");

                // Проверяем CardShooter
                CardShooter shooter = attacker.GetComponent<CardShooter>();
                if (shooter == null)
                {
                    Debug.LogError("No CardShooter! Adding...");
                    shooter = attacker.gameObject.AddComponent<CardShooter>();
                    shooter.projectilePrefab = projectilePrefab;
                }

                shooter.ShootAt(target, 10, false);
            }
        }
        public void StartAutoBattle()
        {
            Debug.Log("Auto Battle started via TurnController");

            if (autoBattleController == null)
            {
                autoBattleController = gameObject.AddComponent<AutoBattleController>();
            }

            if (!autoBattleController.IsBattleActive)
            {
                autoBattleController.StartAutoBattle(autoBattleRounds);
            }
        }

        public void StopAutoBattle()
        {
            if (autoBattleController != null)
            {
                autoBattleController.StopAutoBattle();
            }
        }

        public void TestBattle()
        {
            Debug.Log("Test Battle");

            var playerCards = gameBoard.GetAllCards(true);
            var enemyCards = gameBoard.GetAllCards(false);

            if (playerCards.Count > 0 && enemyCards.Count > 0)
            {
                var attacker = playerCards[0];
                var targets = gameBoard.GetFrontRowCards(false);

                if (targets.Count > 0)
                {
                    Debug.Log(attacker.cardName + " attacks " + targets[0].cardName);
                    attacker.BasicAttack.Execute(attacker, new List<Card> { targets[0] });
                }
            }
        }

        public void TestHealing()
        {
            var playerCards = gameBoard.GetAllCards(true);
            if (playerCards.Count > 0)
            {
                var cardToHeal = playerCards[0];
                if (cardToHeal.IsAlive)
                {
                    cardToHeal.Heal(20);
                }
            }
        }
        void TestBackRowImplementation()
        {
            Debug.Log("\n=== 🧪 BACK ROW IMPLEMENTATION TEST ===");

            if (gameBoard == null)
            {
                Debug.LogError("GameBoard is null!");
                return;
            }

            // Тест 3: Проверка AoE стратегии
            var playerCards = gameBoard.GetAllCards(true);
            if (playerCards.Count > 0)
            {
                var testCard = playerCards[0];


                // Создаем тестовую AoE стратегию
                var aoeStrategy = new BackRowAoEAttackStrategy();
                var targets = aoeStrategy.FindTargets(testCard, gameBoard);
                Debug.Log($"AoE strategy found {targets.Count} targets for {testCard.cardName}");
            }
        }
        public void AddNewRandomCard()
        {
            var freePosition = gameBoard.GetFreePosition(true);
            if (freePosition != null)
            {
                CreateTestCard(freePosition.Value, "New Warrior", warriorCardPrefab);
            }
        }
        private void AddSimpleAttackController(GameObject cardObject, string cardName)
        {
            // Для обычных карт - простой контроллер
            SimpleAttackController controller = cardObject.AddComponent<SimpleAttackController>();
            Debug.Log($"🎯 {cardName} получил простой контроллер атак");
        }
        public void ResetGame()
        {
            Debug.Log("Resetting game");
            ClearAllCards();
            SetupTestCards();
        }

        

        private void ClearAllCards()
        {
            foreach (var card in allCards)
            {
                if (card != null) Destroy(card.gameObject);
            }
            allCards.Clear();
        }

        public int GetAlivePlayerCards()
        {
            var cards = gameBoard.GetAllCards(true);
            return cards.Count(card => card != null && card.IsAlive && card.enabled);
        }

        public int GetAliveEnemyCards()
        {
            var cards = gameBoard.GetAllCards(false);
            return cards.Count(card => card != null && card.IsAlive && card.enabled);
        }

        private void OnDestroy()
        {
            // Отключаем Input Actions при уничтожении
            autoBattleAction?.Disable();
            testBattleAction?.Disable();
            resetAction?.Disable();
            addCardAction?.Disable();
            healAction?.Disable();
            statusAction?.Disable();
            testBackRowAttack?.Disable();
        }
    }
}