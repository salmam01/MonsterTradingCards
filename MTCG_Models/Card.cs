using MTCG_Models;
using System;

namespace MTCG_Models
{
    public class Card
    {
        public Card() { }

        protected string name;
        protected char type;
        protected int damage = 10;
        protected int element;

        public string GetName() { return name; }
        public int GetDamage() { return damage; }
        public int GetElement() { return element; } 
        public void Attack(Card enemy) { }

    }
}
