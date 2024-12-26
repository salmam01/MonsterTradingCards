using System;

namespace MonsterTradingCardsGame.MTCG_Models.Models
{
    public class MonsterCard : Card
    {
        public MonsterCard(string id, string name, int damage) : base(id, name, damage)
        {
            _type = 'M';
        }

        public void SetRandomDamage()
        {

        }

        public void Attack(Card enemy)
        {
            /*if(enemy.GetType() == 'M')
            {

            }*/
        }
    }
}
