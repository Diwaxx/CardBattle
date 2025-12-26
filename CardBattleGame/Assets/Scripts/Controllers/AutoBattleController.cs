using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CardGame.Models;

namespace CardGame.Controllers
{
    public class AutoBattleController : MonoBehaviour
    {
        private TurnController turnController;
        private bool isBattleActive = false;

        public bool IsBattleActive => isBattleActive;

        private void Start()
        {
            EnsureTurnControllerExists();
        }

        private void EnsureTurnControllerExists()
        {
            turnController = FindFirstObjectByType<TurnController>();
            if (turnController == null)
            {
                Debug.Log("TurnController не найден");
                GameObject turnControllerObj = new GameObject("TurnController");
                turnController = turnControllerObj.AddComponent<TurnController>();
                turnController.maxRounds = 30;
            }
        }

        public void StartAutoBattle(int rounds = 30)
        {
            // Убеждаемся, что TurnController существует
            EnsureTurnControllerExists();


            if (!turnController.IsProcessing && !isBattleActive)
            {
                Debug.Log($"=== ЗАПУСК АВТО-БИТВЫ ===");
                Debug.Log($"Максимум раундов: {rounds}");

                isBattleActive = true;
                turnController.maxRounds = rounds;

                // Запускаем TurnController
                turnController.StartBattle(rounds);

                Debug.Log($" РАУНД {turnController.CurrentRound} начался!");

                turnController.OnRoundChanged += HandleRoundChanged;
            }
            else
            {
                Debug.LogWarning(" Битва уже активна!");
            }
        }

        private void HandleRoundChanged(int round)
        {
            Debug.Log($" СМЕНА РАУНДА: {round}");

            // Проверяем лимит раундов
            if (round >= turnController.maxRounds)
            {
                Debug.Log($" Достигнут лимит раундов ({turnController.maxRounds})!");
                StopAutoBattle();
            }
        }

        public void StopAutoBattle()
        {
            Debug.Log("=== ОСТАНОВКА АВТО-БИТВЫ ===");

            // Отписываемся от событий
            if (turnController != null)
            {
                turnController.OnRoundChanged -= HandleRoundChanged;
                turnController.StopBattle();
            }

            isBattleActive = false;
        }
        void Update()
        {
          
        }

        void OnDestroy()
        {
            if (turnController != null)
            {
                turnController.OnRoundChanged -= HandleRoundChanged;
            }
        }
    }
}