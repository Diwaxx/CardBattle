using UnityEngine;

namespace CardGame.Models
{
    public class SlotInfo : MonoBehaviour
    {
        public BoardPosition position;
        public Card currentCard;

        public bool IsOccupied => currentCard != null;

        public void PlaceCard(Card card)
        {
            currentCard = card;
            if (card != null)
            {
                card.transform.SetParent(transform, false);
                card.SetPosition(position);
            }
        }

        public void RemoveCard()
        {
            currentCard = null;
        }
    }
}