using CardGame.Controllers;
using TMPro;
using UnityEngine;

namespace CardGame.Views
{
    public class UIManager : MonoBehaviour
    {
        [Header("Battle UI")]
        public TextMeshProUGUI roundText;

        private void Update()
        {
            if (roundText != null)
            {
                if (GameController.Instance != null)
                {
                    int currentRound = GameController.Instance.GetCurrentRound();
                    roundText.text = "Раунд: " + currentRound + "/30";
                }
                else
                {
                    roundText.text = "Раунд: 1/30";
                }
            }
        }
    }
}