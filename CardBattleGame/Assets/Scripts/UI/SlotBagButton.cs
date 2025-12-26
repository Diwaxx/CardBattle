// SlotBagButton.cs
using UnityEngine;
using UnityEngine.UI;

public class SlotBagButton : MonoBehaviour
{
    [Header("References")]
    public Button button;
    public Image icon;
    public CardSlotHandler slotHandler;

    [Header("Settings")]
    public Sprite bagIcon;
    public Vector2 buttonOffset = new Vector2(0, 40);

    void Start()
    {
        InitializeButton();
    }

    void InitializeButton()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (slotHandler == null)
        {
            slotHandler = GetComponentInParent<CardSlotHandler>();
        }

        // Настраиваем иконку
        if (icon != null && bagIcon != null)
        {
            icon.sprite = bagIcon;
        }

        // Позиционируем кнопку над слотом
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = buttonOffset;
        }

        gameObject.SetActive(false);
    }

    void Update()
    {
        
    }
}