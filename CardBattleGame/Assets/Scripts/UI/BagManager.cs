// BagManager.cs
using CardGame.Views;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class BagManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject bagPanel;
    public Transform heroesContainer;
    public Button closeButton;
    public TMP_FontAsset font;
    public GameObject menu;

    [Header("Hero Prefabs")]
    public List<GameObject> heroPrefabs; // Префабы героев

    [Header("Grid References")]
    public GridManager gridManager;

    private CardSlotHandler currentSelectedSlot;
    private Dictionary<string, GameObject> heroPrefabDictionary = new Dictionary<string, GameObject>();

    void Start()
    {
        InitializeBag();
    }

    void InitializeBag()
    {
        foreach (GameObject heroPrefab in heroPrefabs)
        {
            if (heroPrefab != null)
            {
                heroPrefabDictionary[heroPrefab.name] = heroPrefab;
            }
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseBag);
        }

        // Создаем кнопки героев в сумке
        PopulateBag();
    }

    void PopulateBag()
    {
        
        foreach (Transform child in heroesContainer)
        {
            Destroy(child.gameObject);
        }

        // Создаем кнопки для каждого героя
        foreach (GameObject heroPrefab in heroPrefabs)
        {
            if (heroPrefab == null) continue;

            // Создаем кнопку героя
            GameObject heroButton = new GameObject("HeroButton_" + heroPrefab.name);
            heroButton.transform.SetParent(heroesContainer, false);

            RectTransform rectTransform = heroButton.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200, 300); 

            // Добавляем компоненты
            Image image = heroButton.AddComponent<Image>();
            Button button = heroButton.AddComponent<Button>();

            
            var prefabImage = heroPrefab.GetComponent<SimpleCardView>();
            if (prefabImage != null)
            {
                image.sprite = prefabImage.backgroundImage.sprite;
            }

            // Добавляем обработчик нажатия
            string heroName = heroPrefab.name;
            button.onClick.AddListener(() => OnHeroSelected(heroName));

            // Добавляем текст с именем героя
            GameObject textObj = new GameObject("HeroName");
            textObj.transform.SetParent(heroButton.transform, false);

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.font = font;
            text.text = heroPrefab.name;
            text.color = Color.black;
            
            text.alignment = TextAlignmentOptions.Top;

            text.rectTransform.sizeDelta = new Vector2(200, -100);
        }
    }

    // Открыть сумку для конкретного слота
    public void OpenBagForSlot(CardSlotHandler slot)
    {
        currentSelectedSlot = slot;
        menu.SetActive(false);
        bagPanel.SetActive(true);

        

    }

    public void CloseBag()
    {
        bagPanel.SetActive(false);
        menu.SetActive(true);
        currentSelectedSlot = null;
    }

    // Выбор героя в сумке
    public void OnHeroSelected(string heroName)
    {
        if (currentSelectedSlot == null)
        {
            Debug.LogWarning("No slot selected!");
            return;
        }

        if (heroPrefabDictionary.ContainsKey(heroName))
        {
            PlaceHeroInSlot(heroPrefabDictionary[heroName], currentSelectedSlot);
            CloseBag();
        }
        else
        {
            Debug.LogWarning($"Hero prefab not found: {heroName}");
        }
    }
    void PlaceHeroInSlot(GameObject heroPrefab, CardSlotHandler slot)
    {
        // Если слот уже занят, очищаем его
        if (slot.transform.childCount > 0)
        {
            GameObject currentHero = slot.transform.GetChild(0).gameObject;
            Destroy(currentHero);
        }

        GameObject newHero = Instantiate(heroPrefab, slot.transform);
        newHero.name = heroPrefab.name;
        newHero.transform.localPosition = Vector3.zero;
        CardDragHandler dragHandler = newHero.AddComponent<CardDragHandler>();

        CanvasGroup canvasGroup = newHero.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = newHero.AddComponent<CanvasGroup>();
        }

        slot.UpdateOccupiedStatus();

        
        if (gridManager != null)
        {
            gridManager.OnLayoutChanged();
        }

        Debug.Log($"Hero {heroPrefab.name} placed in slot {slot.name}");
    }

    // Получить список доступных героев
    public List<string> GetAvailableHeroes()
    {
        List<string> availableHeroes = new List<string>();
        foreach (GameObject prefab in heroPrefabs)
        {
            if (prefab != null)
            {
                availableHeroes.Add(prefab.name);
            }
        }
        return availableHeroes;
    }
}