using UnityEngine;
using CardGame.Strategies;

namespace CardGame.Models
{
    public class TankCard : Card
    {
        private IAttackStrategy _basicAttack;

        public override IAttackStrategy BasicAttack
        {
            get
            {
                if (_basicAttack == null)
                    _basicAttack = new BasicAttackStrategy();
                return _basicAttack;
            }
        }

        public override IAttackStrategy SpecialAttack
        {
            get { return BasicAttack; } 
        }

        protected override void Awake()
        {
            base.Awake();
            cardName = "Воин";

            if (stats.health == 0) 
            {
                stats = new CardStats(120, 18, 4, 0.15f, 0.05f, 8, 15);
            }
        }
    }
}