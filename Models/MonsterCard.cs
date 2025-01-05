using System;

namespace MonsterTradingCardsGame.Models
{
    public class MonsterCard : Card
    {
        public char Speciality { get; set; }

        public MonsterCard(string id, string name, double damage) : base(id, name, damage)
        {
            _type = 'M';
        }
    }
}
