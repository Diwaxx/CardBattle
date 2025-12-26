using CardGame.Models;
using UnityEngine;

public class CardShooter : MonoBehaviour
{
    [Header("Settings")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 400f;

    public event System.Action OnShootComplete;
    private Card card;

    void Start()
    {
        card = GetComponent<Card>();
    }

    public void ShootAt(Card target, int damage, bool isCritical = false)
    {
        if (projectilePrefab == null || target == null) return;

        Debug.Log($" {card.cardName} shooting at {target.cardName}");

        
        Canvas canvas = FindCanvas();
        if (canvas == null)
        {
            Debug.LogError(" No Canvas found!");
            OnShootComplete?.Invoke();
            return;
        }
        //Создание снаряда
        GameObject projectile = Instantiate(projectilePrefab, canvas.transform);
        projectile.transform.position = transform.position;

        ProjectileController projectileController = projectile.GetComponent<ProjectileController>();
        if (projectileController == null)
        {
            projectileController = projectile.AddComponent<ProjectileController>();
        }

        // Подписываемся на попадание снаряда
        projectileController.OnProjectileHit += () =>
        {
            Debug.Log($" Projectile hit callback for {card.cardName}");
            OnShootComplete?.Invoke();
        };

        projectileController.speed = projectileSpeed;
        projectileController.Initialize(card, target, damage, isCritical);
        Destroy(projectile, 3f);
    }

    private Canvas FindCanvas()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = FindFirstObjectByType<Canvas>();
        }
        return canvas;
    }
}