using System.Collections.Generic;
using CardGame.Models;

namespace CardGame.Strategies
{
    public interface IAttackStrategy
    {
        string AttackName { get; }
        string Description { get; }

        void Execute(Card attacker, List<Card> targets);
        List<Card> FindTargets(Card attacker, GameBoard board);
        bool CanExecute(Card attacker);
    }
}