using UnityEngine;
using UnityEngine.UI;
using CardGame.Models;

namespace CardGame.Controllers
{
    public class SimpleProjectile : MonoBehaviour
    {
        public float speed = 400f;

        private Card target;
        private RectTransform rectTransform;

        void Awake()
        {
            Debug.Log($" [SimpleProjectile2D] Awake() called on {gameObject.name}");
        }

        void Start()
        {
            Debug.Log($" [SimpleProjectile2D] Start() called on {gameObject.name}");

            rectTransform = GetComponent<RectTransform>();

            Image image = GetComponent<Image>();

            // Размер
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(30, 30);
                Debug.Log($" [SimpleProjectile2D] Size set: {rectTransform.sizeDelta}");
            }

            Debug.Log($" [SimpleProjectile2D] Current target: {target?.cardName ?? "NULL"}");
        }

        public void Setup(Card attacker, Card targetCard, int damage, bool isCritical)
        {

            Debug.Log($"   Attacker: {attacker?.cardName ?? "NULL"}");
            Debug.Log($"   Target: {targetCard?.cardName ?? "NULL"}");

            target = targetCard;

            if (target == null) return;

            Debug.Log($" [SimpleProjectile2D] Setup complete. Will move to: {target.cardName}");
        }

        void Update()
        {
            

            if (target == null)
            {
                Destroy(gameObject);
                return;
            }

            try
            {
                // Движение
                Vector2 targetPos = target.transform.position;
                Vector2 currentPos = transform.position;

                transform.position = Vector2.MoveTowards(
                    currentPos,
                    targetPos,
                    speed * Time.deltaTime
                );

                // Проверка дистанции
                float distance = Vector2.Distance(transform.position, targetPos);
                if (distance < 15f)
                {
                    Debug.Log($" [SimpleProjectile] Hit target! Distance: {distance}");
                    Destroy(gameObject);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"  Error in Update: {e.Message}");
                Destroy(gameObject);
            }
        }

        void OnDestroy()
        {
            Debug.Log($" [SimpleProjectile] Destroyed: {gameObject.name}");
        }
    }
}