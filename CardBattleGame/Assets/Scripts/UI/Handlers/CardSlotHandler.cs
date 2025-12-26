// CardSlotHandler.cs
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardSlotHandler : MonoBehaviour, IDropHandler, IPointerExitHandler
{
    [Header("Visual Feedback")]
    public Image slotImage;
    public Color normalColor = Color.white;
    //public Color highlightColor = new Color(0.8f, 0.9f, 1f, 0.5f);
    //public Color occupiedColor = new Color(1f, 0.9f, 0.8f, 0.3f);
    //public Color swapColor = new Color(1f, 0.8f, 0.8f, 0.5f);

    [Header("UI Elements")]
    public Button bagButton;
    public GameObject slotIndicator; 

    [Header("Slot Type")]
    public bool isOccupied = false;
    public string allowedType = "Hero";

    [Header("Bag Reference")]
    public BagManager bagManager;

    private CardDragHandler currentCard;


    private bool isSelected = false;

    void Start()
    {
        if (slotImage == null)
        {
            slotImage = GetComponent<Image>();
        }

        if (bagButton != null)
        {
            bagButton.onClick.AddListener(OnBagButtonClicked);
            
        }
        if (slotIndicator != null)
        {
            slotIndicator.SetActive(false);
        }
        // UpdateVisual();
        UpdateOccupiedStatus();
    }

    void OnBagButtonClicked()
    {
        if (bagManager != null)
        {
            // Открываем сумку для этого слота
            bagManager.OpenBagForSlot(this);
            SetSelected(true);
        }
        else
        {
            Debug.LogWarning("BagManager not found!");
        }
    }
    // Установить выбранное состояние
    public void SetSelected(bool selected)
    {
        isSelected = selected;

        // Показать/скрыть индикатор
        if (slotIndicator != null)
        {
            slotIndicator.SetActive(selected);
        }

        // Обновить визуал
       // UpdateVisual();

        // Если сброшено выделение, скрыть кнопку сумки через время
        if (!selected)
        {
            StartCoroutine(HideBagButtonDelayed());
        }
        IEnumerator HideBagButtonDelayed()
        {
            yield return new WaitForSeconds(0.5f);
            //UpdateBagButtonVisibility();
        }
    }
    public void ClearSlot()
    {
        if (transform.childCount > 0)
        {
            Destroy(transform.GetChild(0).gameObject);
            UpdateOccupiedStatus();
        }
    }
    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        if (dropped == null) return;

        CardDragHandler cardDrag = dropped.GetComponent<CardDragHandler>();
        if (cardDrag == null) return;

        // Теперь у нас есть доступ к OriginalParent
        Transform cardOriginalParent = cardDrag.OriginalParent;

        // Проверяем, можно ли разместить здесь
        if (CanAcceptCard(dropped))
        {
            // Если слот уже занят, меняем местами
            if (transform.childCount > 0)
            {
                // Получаем текущую карточку в слоте
                Transform currentCardTransform = transform.GetChild(0);
                CardDragHandler currentCardDrag = currentCardTransform.GetComponent<CardDragHandler>();

                // Получаем оригинальный слот перетаскиваемой карточки
                if (cardOriginalParent != null)
                {
                    // Возвращаем текущую карточку в оригинальный слот перетаскиваемой
                    currentCardTransform.SetParent(cardOriginalParent);
                    currentCardTransform.localPosition = Vector3.zero;

                    // Обновляем оригинальный родитель текущей карточки
                    if (currentCardDrag != null)
                    {
                        currentCardDrag.SetNewParent(cardOriginalParent);
                    }

                    // Визуальная обратная связь для свапа
                   // StartCoroutine(ShowSwapFeedback());
                }
            }

            // Помещаем перетаскиваемую карточку в этот слот
            dropped.transform.SetParent(transform);
            dropped.transform.localPosition = Vector3.zero;

            // Обновляем оригинальный родитель перетаскиваемой карточки
            cardDrag.SetNewParent(transform);

            UpdateOccupiedStatus();
        }
        else
        {
            // Если нельзя разместить, возвращаем на место
            cardDrag.ResetToOriginalPosition();
        }
    }

    //private System.Collections.IEnumerator ShowSwapFeedback()
    //{
    //    if (slotImage != null)
    //    {
    //        Color originalColor = slotImage.color;
    //        slotImage.color = swapColor;
    //        yield return new WaitForSeconds(0.3f);
    //        UpdateVisual();
    //    }
    //}

    private bool CanAcceptCard(GameObject card)
    {
        // Проверяем тип карточки
        // Можно добавить дополнительные проверки здесь
        return true;
    }

    //public void OnPointerEnter(PointerEventData eventData)
    //{
    //    if (slotImage != null)
    //    {
    //        slotImage.color = highlightColor;
    //    }
    //}

    public void OnPointerExit(PointerEventData eventData)
    {
       // UpdateVisual();
    }

    public void UpdateOccupiedStatus()
    {
        isOccupied = transform.childCount > 0;

        // Получаем текущую карточку, если есть
        if (isOccupied)
        {
            currentCard = transform.GetChild(0).GetComponent<CardDragHandler>();
        }
        else
        {
            currentCard = null;
        }

       // UpdateVisual();
    }

    //private void UpdateVisual()
    //{
    //    if (slotImage != null)
    //    {
    //        slotImage.color = isOccupied ? occupiedColor : normalColor;
    //    }
    //}

    // Метод для получения текущей карточки в слоте
    public GameObject GetCurrentCard()
    {
        if (transform.childCount > 0)
        {
            return transform.GetChild(0).gameObject;
        }
        return null;
    }
}