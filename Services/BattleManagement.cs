using MonsterTradingCardsGame.Database;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Server;
using MonsterTradingCardsGame.Services.Authentication;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace MonsterTradingCardsGame.Services
{
    public class BattleManagement
    {
        private List<User> _users;
        private string _battleLog;
        private int _maxRounds = 100;

        public BattleManagement(List<User> readyUsers)
        {
            _users = readyUsers;
        }

        public List<User> ProcessBattle()
        {
            try
            {
                _battleLog = "Battle Log\n";
                int battleResult = StartBattle();
                _battleLog += "Battle has ended.\n Battle Result: ";

                switch(battleResult)
                {
                    case 0:
                        _battleLog += "Draw.";
                        return new List<User>();

                    case 1:
                        _battleLog += $"{_users[0].Username}" +" won!\n";
                        //  update user stats
                        break;

                    case 2:
                        _battleLog += $"{_users[1].Username}" + " won!\n";
                        //  update user stats
                        break;

                    default:
                        _battleLog += $"An error occurred during battle.";
                        return new List<User>();
                }

                Console.Write(_battleLog);
                return _users;
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred during battle: {e.Message}");
                return new List<User>();
            }
        }

        public int StartBattle()
        {
            int roundCount = 0;
            int roundResult;

            while (roundCount <= _maxRounds)
            {
                _battleLog += $"Round: {roundCount}\n";
                if (_users[0].UserDeck.DeckCards.Count == 0)
                {
                    return 2;
                }
                if (_users[1].UserDeck.DeckCards.Count == 0)
                {
                    return 1;
                }

                //  Get random cards for the round
                Card card1 = _users[0].UserDeck.GetRandomCard();
                _battleLog += $"{_users[0].Username} is using {card1.Name}.\n";

                Card card2 = _users[1].UserDeck.GetRandomCard();
                _battleLog += $"{_users[1].Username} is using {card2.Name}.\n";

                //  Battle begins
                roundResult = BattleLogic(card1, card2);

                switch (roundResult)
                {
                    case 0:
                        _battleLog += "Draw! No one won this round.\n";
                        continue;

                    case 1:
                        _battleLog += $"{_users[0].Username} won this round!\n";
                        _users[0].UserDeck.AddCardToDeck(card2);
                        _users[1].UserDeck.RemoveCardFromDeck(card2);
                        break;

                    case 2:
                        _battleLog += $"{_users[1].Username} won this round!\n";
                        _users[1].UserDeck.AddCardToDeck(card1);
                        _users[0].UserDeck.RemoveCardFromDeck(card2);
                        break;

                    //  Error
                    default:
                        Console.WriteLine($"An error occurred during battle.");
                        _battleLog += "An internal server occurred during battle.";
                        return -1;
                }
                roundCount++;
            }
            return -1;
        }

        public int BattleLogic(Card card1, Card card2)
        {
            char type1 = card1.GetType();
            char type2 = card2.GetType();

            //  Pure monster fight
            if (type1 == 'M' && type2 == 'M')
            {
                _battleLog += "Both cards are of type Monster! Pure monster fight commencing!\n";
                return PureMonsterFight(card1, card2);
            }

            //  Monstercard vs. Spellcard
            if (type1 == 'M' && type2 == 'S' || type1 == 'S' && type2 == 'M')
            {
                _battleLog += $"Fight!\n";
                return MonsterSpellFight(card1, card2);
            }

            return -1;
        }

        public int PureMonsterFight(Card card1, Card card2)
        {
            if (card1 is MonsterCard monster1 && card2 is MonsterCard monster2)
            {
                char speciality1 = monster1.Speciality;
                char speciality2 = monster2.Speciality;
                double damage1 = monster1.Damage;
                double damage2 = monster2.Damage;

                if (speciality1 == 'G' && speciality2 == 'D')
                {
                    damage1 = 0;
                    _battleLog = $"{card1.Name} was too afraid of {card2.Name} to attack!\n";
                }
                if (speciality2 == 'G' && speciality1 == 'D')
                {
                    damage2 = 0;
                    _battleLog = $"{card2.Name} was too afraid of {card1.Name} to attack!\n";
                }

                if (speciality1 == 'W' && speciality2 == 'O')
                {
                    damage2 = 0;
                    _battleLog = $"{card1.Name} controlled {card2.Name} to not attack!\n";
                }
                if (speciality2 == 'W' && speciality1 == 'O')
                {
                    damage1 = 0;
                    _battleLog = $"{card2.Name} controlled {card1.Name} to not attack!\n";
                }

                return Fight(damage1, damage2);
            }
            return -1;
        }

        /*
        Missing:
        • The armor of Knights is so heavy that WaterSpells make them drown them instantly. 
        • The Kraken is immune against spells. 
        • The FireElves know Dragons since they were little and can evade their attacks. 
        */
        public int MonsterSpellFight(Card card1, Card card2)
        {
            char element1 = card1.GetElement();
            char element2 = card2.GetElement();
            double damage1 = card1.Damage;
            double damage2 = card2.Damage;
            int value = 2;

            //  Normal fight
            if (element1 == element2)
            {
                return Fight(damage1, damage2);
            }

            switch(element1)
            {
                case 'W':

                    if(element2 == 'F')
                    {
                        _battleLog = $"{card1.Name} is of type *Water*.\n";
                        _battleLog = $"{card2.Name} is of type *Fire*.\n";
                        _battleLog = $"{card1.Name} does double damage!";
                        damage1 *= value;
                    }
                    else
                    {
                        damage1 /= value;
                    }
                    break;

                case 'F':

                    if(element2 == 'W')
                    {
                        damage1 /= value;
                    }
                    else
                    {
                        damage1 *= value;
                    }
                    break;

                case 'N':

                    if(element2 == 'F')
                    {
                        damage1 /= value;
                    }
                    else
                    {
                        damage1 *= value;
                    }
                    break;

                default:
                    return -1;

            }

            return Fight(damage1, damage2);
        }

        public int Fight(double damage1, double damage2)
        {
            if (damage1 > damage2)
            {
                return 1;
            }
            else if (damage1 < damage2)
            {
                return 2;
            }
            else
            {
                return 0;
            }
        }

        //public bool Remove()

    }
}
