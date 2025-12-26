using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CardGame.Models;
using System.Collections;

namespace CardGame.Views
{
    public class SimpleCardView : MonoBehaviour
    {

        [Header("Attack Animation")]
        [SerializeField] private GameObject attackIndicator;
        [SerializeField] private float attackIndicatorDuration = 0.5f;

        [Header("References")]
        public TextMeshProUGUI dodgeText;
        public Slider hpBar;
        public TextMeshProUGUI takenDamageText;
        public TextMeshProUGUI healAmountText;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI attackText;
        public TextMeshProUGUI speedText;
        public TextMeshProUGUI healthText;
        public Image backgroundImage;

        [Header("TMP Sprite Asset")]
        public TMP_SpriteAsset numbersSpriteAsset; 

        [Header("Grave Settings")]
        public GameObject gravePrefab;

        private Card cardModel;
        private bool isDead = false;

        private void Start()
        {
            // Устанавливаем sprite asset для всех текстовых полей
            SetSpriteAssetForTextFields();
        }


        private void SetSpriteAssetForTextFields()
        {
            if (numbersSpriteAsset == null) return;

            TextMeshProUGUI[] allTextFields = {
                attackText, speedText, healthText, takenDamageText, healAmountText
            };

            foreach (var textField in allTextFields)
            {
                if (textField != null)
                {
                    textField.spriteAsset = numbersSpriteAsset;
                }
            }
        }

        public void Initialize(Card card, bool isPlayerCard = true)
        {
            cardModel = card;

            if (takenDamageText != null)
            {
                takenDamageText.gameObject.SetActive(false);
                takenDamageText.color = Color.red;
            }

            if (healAmountText != null)
            {
                healAmountText.gameObject.SetActive(false);
                healAmountText.color = Color.green;
            }

            UpdateCardDisplay(isPlayerCard);

            cardModel.OnDamageTaken += OnDamageTaken;
            cardModel.OnHealed += OnHealed;
            cardModel.OnCardDeath += OnDeath;
            cardModel.OnDodge += OnDodge;
        }

        private void UpdateCardDisplay(bool isPlayerCard)
        {
            if (isDead) return;

            if (nameText != null) nameText.text = cardModel.cardName;

            // Используем спрайтовые цифры через TMP
            if (attackText != null)
                attackText.text = "ATK: " + GetSpriteNumberText(cardModel.stats.attack);

            if (speedText != null)
                speedText.text = "SPD: " + GetSpriteNumberText(cardModel.stats.speed);

            if (hpBar != null)
            {
                hpBar.minValue = 0;
                hpBar.maxValue = cardModel.stats.maxHealth;
                hpBar.value = cardModel.stats.health;
            }

            if (healthText != null)
            {
                string currentHealth = GetSpriteNumberText(cardModel.stats.health);
                string maxHealth = GetSpriteNumberText(cardModel.stats.maxHealth);
                healthText.text = currentHealth + "/" + maxHealth;
            }
        }

        // Метод для преобразования чисел в спрайтовый текст
        private string GetSpriteNumberText(int number)
        {
            return GetSpriteNumberText(number.ToString());
        }

        private string GetSpriteNumberText(string text)
        {
            string result = "";
            foreach (char c in text)
            {
                if (char.IsDigit(c))
                {
                    result += $"<sprite name=\"{c}\">";
                }
                else
                {
                    result += c;
                }
            }
            return result;
        }

        private void OnDamageTaken(Card card, int damage)
        {
            if (isDead) return;

            UpdateCardDisplay(true);

            if (takenDamageText != null)
            {
               
                string damageText = GetSpriteNumberText(damage);
                takenDamageText.text = "<sprite name=\"-\">" + damageText;
                takenDamageText.gameObject.SetActive(true);
                CancelInvoke(nameof(HideDamageText));
                Invoke(nameof(HideDamageText), 1.5f);
            }

            StartCoroutine(DamageFlash());
        }
        private void OnDodge(Card card)
        {
            if (dodgeText != null)
            {
                dodgeText.text = "Уклонение!";
                dodgeText.gameObject.SetActive(true);
                CancelInvoke("HideDodgeText");
                Invoke("HideDodgeText", 1.5f);
            }

        }
        private void OnHealed(Card card, int amount)
        {
            if (isDead) return;

            UpdateCardDisplay(true);

            if (healAmountText != null)
            {
                string healText = GetSpriteNumberText(amount);
                healAmountText.text = "+" + healText;
                healAmountText.gameObject.SetActive(true);
                CancelInvoke(nameof(HideHealText));
                Invoke(nameof(HideHealText), 1.5f);
            }

            StartCoroutine(HealPulse());
        }

        private void OnDeath(Card card)
        {
            if (isDead) return;

            StartCoroutine(DelayedGraveReplacement());
        }

        private IEnumerator DelayedGraveReplacement()
        {
            // Ждем чтобы увидеть урон
            yield return new WaitForSeconds(0.5f);
            isDead = true;

            
            ReplaceWithGrave();

            if (cardModel != null)
            {
                cardModel.enabled = false;
            }

            Debug.Log($" Card replaced with grave: {cardModel.cardName}");
        }

        private void ReplaceWithGrave()
        {
            Transform parent = transform.parent;
            int siblingIndex = transform.GetSiblingIndex();

            GameObject grave = Instantiate(gravePrefab, parent);
            grave.name = "Grave";

            grave.transform.SetSiblingIndex(siblingIndex);

            RectTransform graveRect = grave.GetComponent<RectTransform>();
            RectTransform cardRect = GetComponent<RectTransform>();

            if (graveRect != null && cardRect != null)
            {
                graveRect.anchorMin = cardRect.anchorMin;
                graveRect.anchorMax = cardRect.anchorMax;
                graveRect.pivot = cardRect.pivot;
                graveRect.anchoredPosition = cardRect.anchoredPosition;
                graveRect.sizeDelta = cardRect.sizeDelta;
                graveRect.localScale = cardRect.localScale;
                graveRect.localRotation = cardRect.localRotation;
            }

            Destroy(gameObject);

            Debug.Log("Grave placed at same position as card, sibling index: " + siblingIndex);
        }

        private IEnumerator DamageFlash()
        {
            if (backgroundImage != null && !isDead)
            {
                Color originalColor = backgroundImage.color;
                backgroundImage.color = Color.red;
                yield return new WaitForSeconds(0.2f);
                if (!isDead) backgroundImage.color = originalColor;
            }
        }

        private IEnumerator HealPulse()
        {
            if (backgroundImage != null && !isDead)
            {
                Color originalColor = backgroundImage.color;
                backgroundImage.color = Color.green;
                yield return new WaitForSeconds(0.3f);
                if (!isDead) backgroundImage.color = originalColor;
            }
        }

        private void HideDamageText()
        {
            if (takenDamageText != null && !isDead)
            {
                takenDamageText.gameObject.SetActive(false);
            }
        }

        private void HideHealText()
        {
            if (healAmountText != null && !isDead)
            {
                healAmountText.gameObject.SetActive(false);
            }
        }
        private void HideDodgeText()
        {
            if (dodgeText != null && !isDead)
            {
                dodgeText.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (cardModel != null)
            {
                cardModel.OnDamageTaken -= OnDamageTaken;
                cardModel.OnHealed -= OnHealed;
                cardModel.OnCardDeath -= OnDeath;
                cardModel.OnDodge -= OnDodge;
            }
        }
    }
}