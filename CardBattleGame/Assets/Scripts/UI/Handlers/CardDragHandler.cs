// CardDragHandler.cs
using UnityEngine;
using UnityEngine.EventSystems;

public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("References")]
    private CanvasGroup canvasGroup;

    [Header("Drag State")]
    private Vector3 originalPosition;
    private int originalSiblingIndex;

    // Делаем public для доступа из других скриптов
    public Transform OriginalParent { get; private set; }
    public bool IsDragging { get; private set; }

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        OriginalParent = transform.parent;
        originalPosition = transform.position;
        originalSiblingIndex = transform.GetSiblingIndex();
        IsDragging = true;

        // Делаем карточку прозрачной и неблокирующей лучи
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;

        // Перемещаем в корень Canvas, чтобы была поверх других элементов
        transform.SetParent(transform.root);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Следим за позицией курсора/тача
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        IsDragging = false;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // Если не сбросили на слот, возвращаем обратно
        if (transform.parent == transform.root)
        {
            ResetToOriginalPosition();
        }
    }

    public void ResetToOriginalPosition()
    {
        transform.SetParent(OriginalParent);
        transform.position = originalPosition;
        transform.SetSiblingIndex(originalSiblingIndex);
    }

    // Метод для принудительной установки нового родителя
    public void SetNewParent(Transform newParent)
    {
        OriginalParent = newParent;
    }
}