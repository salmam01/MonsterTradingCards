using System;

namespace MonsterTradingCardsGame.Models
{
    public class SpellCard : Card
    {
        public SpellCard(string id, string name, double damage) : base(id, name, damage)
        {
            _type = 'S';
        }
    }
}
