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
        public char Element { get; set; }
        public char Type { get; set; }
        public char Speciality { get; set; }

        public Card(string id, string name, double damage)
        {
            Id = id;
            Name = name;
            Damage = damage;

            SetCardType();
            SetCardElement();
            SetCardSpeciality();
        }

        public void SetCardType()
        {
            if (Name.EndsWith("Spell"))
            {
                Type = 'S';
            }
            else
            {
                Type = 'M';
            }
        }

        public void SetCardElement()
        {
            if (Name.StartsWith("Water"))
            {
                Element = 'W';
            }
            else if (Name.StartsWith("Fire"))
            {
                Element = 'F';
            }
            else
            {
                Element = 'N';
            }   
        }

        public void SetCardSpeciality()
        {
            if (Type == 'M')
            {
                Dictionary<string, char> specialities = new()
                {
                    { "Goblin", 'G' },
                    { "Dragon", 'D' },
                    { "Wizzard", 'W' },
                    { "Ork", 'O' },
                    { "Knight", 'N' },
                    { "Kraken", 'K' },
                    { "Elf", 'E' }
                };

                foreach (string s in specialities.Keys)
                {
                    if (Name == s || Name.EndsWith(s))
                    {
                        Speciality = specialities[s];
                    }
                }
            }
            else
            {
                Speciality = '0';
            }
        }

    }
}
