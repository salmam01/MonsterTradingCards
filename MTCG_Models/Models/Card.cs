using System;
using System.Reflection.PortableExecutable;
using System.Text.Json.Serialization;

namespace MonsterTradingCardsGame.MTCG_Models.Models
{
    public class Card
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double Damage { get; set; }
        protected int _element;
        protected char _type;

        public Card(string id, string name, double damage)
        {
            Id = id;
            Name = name;
            Damage = damage;
        }

        public char GetType() { return _type; }

        public void SetElement() { }

        public void SetType() { }

        public void Attack(Card enemy) { }

    }
}
