using CardGame.Controllers;
using CardGame.Strategies;
using System;
using UnityEngine;

namespace CardGame.Models
{
    public abstract class Card : MonoBehaviour
    {
        public string cardId;
        public string cardName;
        public CardType cardType;
        public CardStats stats;
        public BoardPosition position;

        public event Action<Card> OnCardDeath;
        public event Action<Card, int> OnDamageTaken;
        public event Action<Card, int> OnHealed;
        public event Action<Card> OnDodge;
        
        public bool isStunned = false; // Оглушена ли карта
        public int attacksThisTurn = 0;
        public int maxAttacksPerTurn = 1;
        public bool IsAlive => stats.health > 0;

        public abstract IAttackStrategy BasicAttack { get; }
        public abstract IAttackStrategy SpecialAttack { get; }

        public event Action<Card> OnStunned;
        protected virtual void Awake()
        {
            if (string.IsNullOrEmpty(cardId))
                cardId = Guid.NewGuid().ToString();

            if (stats.maxHealth == 0)
                stats.maxHealth = stats.health;
        }

        public virtual void TakeDamage(int damage, bool isCrit = false, Card damageSource = null)
        {
            if (UnityEngine.Random.value < stats.dodgeChance)
            {
                Debug.Log(cardName + " dodged attack");
                OnDodge?.Invoke(this);
                return;
            }

            int finalDamage = CalculateFinalDamage(damage, isCrit);
            stats.health = Mathf.Max(0, stats.health - finalDamage);

            Debug.Log(cardName + " takes " + finalDamage + " damage" + (isCrit ? " CRIT" : "") +
                      ". HP: " + stats.health + (damageSource != null ? $" from {damageSource.cardName}" : ""));

            OnDamageTaken?.Invoke(this, finalDamage);

            if (!IsAlive)
            {
                Debug.Log(cardName + " dies");
                OnCardDeath?.Invoke(this);
            }
        }

    
        
        public virtual void StartTurn()
        {
            attacksThisTurn = 0;
        }

        public virtual void EndTurn()
        {
            
        }

        public void Stun(int duration)
        {
            isStunned = true;
            OnStunned?.Invoke(this);
            Debug.Log($"{cardName} is stunned for {duration} turns");

            //  добавить корутину для снятия оглушения 
            //StartCoroutine(RemoveStun(duration)); 
        }
        private int CalculateFinalDamage(int baseDamage, bool isCrit)
        {
            int damage = isCrit ? baseDamage * 2 : baseDamage;
            damage = Mathf.Max(1, damage - stats.armor);
            return damage;
        }
        public virtual void Heal(int amount, Card healer)
        {
            int oldHealth = stats.health;
            stats.health = Mathf.Min(stats.maxHealth, stats.health + amount);
            int actualHeal = stats.health - oldHealth;

            if (actualHeal > 0)
            {
                Debug.Log($"{cardName} получает лечение: +{actualHeal} HP от {healer.cardName}");
                OnHealed?.Invoke(this, actualHeal);
            }
        }
        public virtual void Heal(int amount)
        {
            Heal(amount, null);
        }



        public virtual void SetPosition(BoardPosition newPosition)
        {
            position = newPosition;
        }
    }

    public enum CardType
    {
        Warrior,
        Archer,
        Mage,
        Assasin,
        Heal
    }
}