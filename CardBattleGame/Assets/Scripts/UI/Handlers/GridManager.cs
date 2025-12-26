using CardGame.Controllers;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public List<CardSlotHandler> slots = new List<CardSlotHandler>();
    public List<GameObject> heroes = new List<GameObject>();

    [Header("Free Area")]
    public CardSlotHandler freeSlot; 

    [Header("Save/Load")]
    public bool autoSave = true;

    [Header("Formation Battle")]
    public FormationManager formationManager;
    void Start()
    {
        InitializeGrid();
        LoadHeroesLayout();
        if (formationManager == null)
        {
            formationManager = GetComponent<FormationManager>();
        }
    }
    void InitializeGrid()
    {
        slots.Clear();

        // Находим все слоты в GridHero
        Transform gridHero = transform.Find("GridHero") ?? transform;

        for (int i = 0; i < gridHero.childCount; i++)
        {
            Transform slotTransform = gridHero.GetChild(i);

            // Проверяем, является ли это слотом
            if (slotTransform.name.Contains("CardSlot") || slotTransform.name == "free")
            {
                CardSlotHandler slotHandler = slotTransform.GetComponent<CardSlotHandler>();
                slots.Add(slotHandler);

                if (slotTransform.name == "free" && freeSlot == null)
                {
                    freeSlot = slotHandler;
                }
            }
        }
    }
    public void UpdateAllSlots()
    {
        foreach (CardSlotHandler slot in slots)
        {
            if (slot != null)
            {
                slot.UpdateOccupiedStatus();
            }
        }
    }
    public CardSlotHandler GetSlotByIndex(int index)
    {
        if (index >= 0 && index < slots.Count)
        {
            return slots[index];
        }
        return null;
    }
    public CardSlotHandler FindSlotWithCard(GameObject card)
    {
        foreach (CardSlotHandler slot in slots)
        {
            if (slot.GetCurrentCard() == card)
            {
                return slot;
            }
        }
        return null;
    }

    // Метод для сохранения расположения героев
    public void SaveHeroesLayout()
    {
        List<HeroPosition> positions = new List<HeroPosition>();

        for (int i = 0; i < slots.Count; i++)
        {
            GameObject card = slots[i].GetCurrentCard();
            if (card != null)
            {
                positions.Add(new HeroPosition
                {
                    heroId = card.name,
                    heroInstanceId = card.GetInstanceID(),
                    slotIndex = i
                });
            }
        }

        // Конвертируем в JSON и сохраняем
        HeroLayout layout = new HeroLayout { positions = positions };
        string json = JsonUtility.ToJson(layout);
        PlayerPrefs.SetString("HeroesLayout", json);
        PlayerPrefs.Save();

        Debug.Log("Hero layout saved: " + json);
    }

    // Метод для загрузки расположения
    public void LoadHeroesLayout()
    {
        if (PlayerPrefs.HasKey("HeroesLayout"))
        {
            string json = PlayerPrefs.GetString("HeroesLayout");
            HeroLayout layout = JsonUtility.FromJson<HeroLayout>(json);

            Debug.Log("Loading hero layout: " + json);
            DisableAllDragHandlers();
            ClearAllSlots();

            // Восстанавливаем позиции
            foreach (var pos in layout.positions)
            {
                GameObject hero = FindHeroByName(pos.heroId);
                if (hero != null && pos.slotIndex < slots.Count)
                {
                    // Помещаем героя в слот
                    hero.transform.SetParent(slots[pos.slotIndex].transform);
                    hero.transform.localPosition = Vector3.zero;

                    // Обновляем оригинальный родитель в CardDragHandler
                    CardDragHandler dragHandler = hero.GetComponent<CardDragHandler>();
                    if (dragHandler != null)
                    {
                        dragHandler.SetNewParent(slots[pos.slotIndex].transform);
                    }
                }
            }
            UpdateAllSlots();
            EnableAllDragHandlers();
        }
    }

    private GameObject FindHeroByName(string heroName)
    {
        // Поиск среди всех героев в сцене
        GameObject[] allHeroes = GameObject.FindGameObjectsWithTag("Hero");
        foreach (GameObject hero in allHeroes)
        {
            if (hero.name == heroName)
            {
                return hero;
            }
        }
        return null;
    }

    private void ClearAllSlots()
    {
        foreach (CardSlotHandler slot in slots)
        {
            if (slot.transform.childCount > 0)
            {
                foreach (Transform child in slot.transform)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
    }

    private void DisableAllDragHandlers()
    {
        CardDragHandler[] handlers = FindObjectsByType<CardDragHandler>(FindObjectsSortMode.None);
        foreach (CardDragHandler handler in handlers)
        {
            handler.enabled = false;
        }
    }

    private void EnableAllDragHandlers()
    {
        CardDragHandler[] handlers = FindObjectsByType<CardDragHandler>(FindObjectsSortMode.None);
        foreach (CardDragHandler handler in handlers)
        {
            handler.enabled = true;
        }
    }
    public void OnLayoutChanged()
    {
        if (autoSave)
        {
            SaveHeroesLayout();
        }
         if (formationManager != null)
        {
            formationManager.UpdateFormationStatus();
        }
    }

    [System.Serializable]
    public class HeroPosition
    {
        public string heroId;
        public int heroInstanceId;
        public int slotIndex;
    }

    [System.Serializable]
    public class HeroLayout
    {
        public List<HeroPosition> positions;
    }
}