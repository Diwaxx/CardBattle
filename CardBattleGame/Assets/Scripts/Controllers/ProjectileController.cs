using CardGame.Models;
using UnityEngine;
using UnityEngine.UI;
public class ProjectileController : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 400f;

    // Событие при попадании
    public event System.Action OnProjectileHit;

    private Card target;
    private Card attacker;
    private RectTransform rectTransform;
    private Image image;
    private bool isPlayerAttacker;
    private int damage;
    private bool isCritical;
    private bool hasHit = false;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();

        if (image == null)
        {
            image = gameObject.AddComponent<Image>();
            image.color = Color.red;
        }

        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(100, 100);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }

        SetupInitialRotation();
    }

    public void Initialize(Card attackerCard, Card targetCard, int damageAmount, bool isCriticalHit)
    {
        attacker = attackerCard;
        target = targetCard;
        damage = damageAmount;
        isCritical = isCriticalHit;
        isPlayerAttacker = attacker.position.isPlayerSide;

        if (isCritical && image != null)
        {
            image.color = Color.yellow;
        }

        // Поворачиваем для врага
        if (!isPlayerAttacker && rectTransform != null)
        {
            rectTransform.rotation = Quaternion.Euler(0, 0, 180);
        }
    }

    void Update()
    {
        if (target == null || !target.IsAlive || hasHit)
        {
            if (!hasHit && target != null && !target.IsAlive)
            {
                // Цель умерла до попадания
                OnProjectileHit?.Invoke();
                hasHit = true;
            }
            Destroy(gameObject);
            return;
        }

        // Двигаемся к цели
        Vector2 targetPosition = target.transform.position;
        transform.position = Vector2.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime
        );

        // Проверяем попадание
        if (Vector2.Distance(transform.position, targetPosition) < 15f)
        {
            HitTarget();
        }
    }

    private void HitTarget()
    {
        if (hasHit) return;
        hasHit = true;

        Debug.Log($"💥 Projectile hit {target.cardName}");

        // Наносим урон
        if (target != null && target.IsAlive)
        {
            target.TakeDamage(damage, isCritical);
        }

        // Уведомляем о попадании
        OnProjectileHit?.Invoke();

        // Уничтожаем снаряд
        Destroy(gameObject);
    }

    private void SetupInitialRotation()
    {
        // Игрок (снизу) атакует вверх - без поворота
        // Враг (сверху) атакует вниз - поворот 180 градусов

        if (attacker != null)
        {
            if (!attacker.position.isPlayerSide) // Враг сверху
            {
                // Поворачиваем на 180 градусов чтобы смотреть вниз
                rectTransform.rotation = Quaternion.Euler(0, 0, 180);
            }
            else // Игрок снизу
            {
                // Без поворота - смотрит вверх
                rectTransform.rotation = Quaternion.Euler(0, 0, 0);
            }
        }
    }

    void OnDestroy()
    {
        if (!hasHit)
        {
            // Если снаряд уничтожен до попадания, все равно уведомляем
            OnProjectileHit?.Invoke();
        }
    }
}