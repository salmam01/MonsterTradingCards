using System;

namespace MonsterTradingCardsGame.MTCG_Models.Models
{
    public class SpellCard : Card
    {
        public SpellCard(string id, string name, int damage) : base(id, name, damage)
        {
            _type = 'S';
        }

        public void Attack(Card enemy)
        {

        }
    }
}
