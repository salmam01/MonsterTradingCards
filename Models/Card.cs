using System;
using System.Reflection.PortableExecutable;
using System.Text.Json.Serialization;

namespace MonsterTradingCardsGame.Models
{
    public class Card
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double Damage { get; set; }
        protected char _element;
        protected char _type;

        public Card(string id, string name, double damage)
        {
            Id = id;
            Name = name;
            Damage = damage;
        }

        public void SetElement(char element) 
        {
            _element = element;
        }

        public char GetType() { return _type; }
        
        public char GetElement() { return _element; }
    }
}
