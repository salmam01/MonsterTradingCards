﻿using System;

namespace MonsterTradingCardsGame.MTCG_Models.Frontend
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