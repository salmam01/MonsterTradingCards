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
        private const int _maxRounds = 100;
        private const int _eloPerBattle = 50;
        private const int _coinsPerWin = 25;

        public BattleManagement(List<User> readyUsers)
        {
            _users = readyUsers;
        }

        public List<User> ProcessBattle()
        {
            try
            {
                if(_users == null || _users.Count < 2)
                {
                    Console.WriteLine("List is empty. Battle cannot start with no users.");
                    return new List<User>();
                }

                if (_users[0].UserDeck == null || _users[1].UserDeck == null || _users[0].UserDeck.DeckCards.Count <= 0 || _users[1].UserDeck.DeckCards.Count <= 0)
                {
                    Console.WriteLine("No deck cards available. Battle cannot start.");
                    return new List<User>();
                }

                _battleLog = "\n*******************************************************BATTLE LOG*******************************************************\n";
                _battleLog += "\n";

                int battleResult = StartBattle();
                _battleLog += "Battle has ended.\nBattle Result: ";

                switch(battleResult)
                {
                    case 0:
                        _battleLog += "Draw. Everyone keeps their cards!\n";
                        Console.WriteLine(_battleLog);
                        return new List<User>();

                    case 1:
                        _battleLog += $"{_users[0].Username}" +" won!\n";
                        WinnerReward(_users[0]);
                        LoserPenality(_users[1]);
                        break;

                    case 2:
                        _battleLog += $"{_users[1].Username}" + " won!\n";
                        WinnerReward(_users[1]);
                        LoserPenality(_users[0]);
                        break;

                    default:
                        _battleLog += $"An error occurred during battle.\n";
                        Console.WriteLine(_battleLog);
                        return new List<User>();
                }

                Console.Write(_battleLog);
                foreach (User user in _users)
                {
                    Console.WriteLine(user.Username + " stats:");
                    Console.WriteLine("Elo: " + user.Stats.Elo);
                    Console.WriteLine("Coins: " + user.Stats.Coins);
                    Console.WriteLine("Games Played: " + user.Stats.GamesPlayed);
                    Console.WriteLine("Wins: " + user.Stats.Wins);
                    Console.WriteLine("Losses: " + user.Stats.Losses);
                }

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
            int roundCount = 1;
            int roundResult;

            while (roundCount <= _maxRounds)
            {
                if (_users[0].UserDeck.DeckCards.Count == 0)
                {
                    _battleLog += $"{_users[0].Username}'s Battle Deck is empty. Battle ending...\n";
                    return 2;
                }
                if (_users[1].UserDeck.DeckCards.Count == 0)
                {
                    _battleLog += $"{_users[1].Username}'s Battle Deck is empty. Battle ending...\n";
                    return 1;
                }

                _battleLog += "\n************************************************************************************************************************";
                _battleLog += $"Round {roundCount}\n";
                _battleLog += "************************************************************************************************************************\n\n";

                /*
                _battleLog += $"{_users[0].Username}'s Deck:\n";
                foreach (Card card in _users[0].UserDeck.DeckCards)
                {
                    _battleLog += $"Name: {card.Name}, Damage: {card.Damage}\n";
                }

                _battleLog += $"\n{_users[1].Username}'s Deck:\n";
                foreach (Card card in _users[1].UserDeck.DeckCards)
                {
                    _battleLog += $"Name: {card.Name}, Damage: {card.Damage}\n";
                }
                _battleLog += "\n";
                */

                //  Get random cards for the round
                Card card1 = _users[0].UserDeck.GetRandomCard();
                Card card2 = _users[1].UserDeck.GetRandomCard();

                if (card1 == null || card2 == null)
                {
                    _battleLog += "An error occurred during battle: One of the cards is empty.\n";
                    return -1;
                }
                _battleLog += $"{_users[0].Username} is using {card1.Name}. Damage: {card1.Damage}\n";
                _battleLog += $"{_users[1].Username} is using {card2.Name}. Damage: {card2.Damage}\n";

                //  Battle begins
                roundResult = BattleLogic(card1, card2);

                switch (roundResult)
                {
                    case 0:
                        _battleLog += "Draw! No one wins this round.\n";
                        continue;

                    case 1:
                        _battleLog += $"{_users[0].Username} wins this round!\n";
                        _users[0].UserDeck.AddCardToDeck(card2);
                        _users[1].UserDeck.RemoveCardFromDeck(card2);
                        _battleLog += $"{_users[0].Username} takes over {_users[1].Username}'s card {card2.Name}.\n";
                        break;

                    case 2:
                        _battleLog += $"{_users[1].Username} wins this round!\n";
                        _users[1].UserDeck.AddCardToDeck(card1);
                        _users[0].UserDeck.RemoveCardFromDeck(card1);
                        _battleLog += $"{_users[1].Username} takes over {_users[0].Username}'s card {card1.Name}.\n";
                        break;

                    default:
                        _battleLog += "An error occurred during battle.\n";
                        return -1;
                }
                roundCount++;
            }
            return 0;
        }

        public int BattleLogic(Card card1, Card card2)
        {
            char type1 = card1.Type;
            char type2 = card2.Type;

            //  Pure monster fight
            if (type1 == 'M' && type2 == 'M')
            {
                _battleLog += "Both cards are of type Monster! Pure monster fight commencing!\n";
                return PureMonsterFight(card1, card2);
            }

            //  Monstercard vs. SpellCard or SpellCard vs SpellCard
            if (type1 == 'M' && type2 == 'S' || type1 == 'S' && type2 == 'M' || type1 == type2)
            {
                _battleLog += $"{card1.Name} is of type {type1}\n";
                _battleLog += $"{card2.Name} is of type {type2}\n";
                _battleLog += "Fight!\n";
                return MonsterAndSpellFight(card1, card2);
            }

            _battleLog += "Error: Cards have no type.\n";
            return -1;
        }

        public int PureMonsterFight(Card card1, Card card2)
        {
            if (card1.Type == 'M' && card2.Type == 'M')
            {
                char speciality1 = card1.Speciality;
                char speciality2 = card2.Speciality;
                double damage1 = card1.Damage;
                double damage2 = card2.Damage;

                if (speciality1 == 'G' && speciality2 == 'D')
                {
                    damage1 = 0;
                    _battleLog += $"{card1.Name} was too afraid of {card2.Name} to attack!\n";
                }
                if (speciality2 == 'G' && speciality1 == 'D')
                {
                    damage2 = 0;
                    _battleLog += $"{card2.Name} was too afraid of {card1.Name} to attack!\n";
                }

                if (speciality1 == 'W' && speciality2 == 'O')
                {
                    damage2 = 0;
                    _battleLog += $"{card1.Name} controlled {card2.Name} to not attack!\n";
                }
                if (speciality2 == 'W' && speciality1 == 'O')
                {
                    damage1 = 0;
                    _battleLog += $"{card2.Name} controlled {card1.Name} to not attack!\n";
                }

                if (speciality1 == 'W' && card1.Element == 'F' && speciality2 == 'D')
                {
                    damage2 = 0;
                    _battleLog += $"{card1.Name} evaded {card2.Name}'s attack!";
                }
                if (speciality2 == 'W' && card2.Element == 'F' && speciality1 == 'D')
                {
                    damage1 = 0;
                    _battleLog += $"{card2.Name} evaded {card1.Name}'s attack!";
                }

                return Fight(card1, card2, damage1, damage2);
            }
            return -1;
        }

        public int MonsterAndSpellFight(Card card1, Card card2)
        {
            char element1 = card1.Element;
            char element2 = card2.Element;
            double damage1 = card1.Damage;
            double damage2 = card2.Damage;
            int multiplier = 2;
            
            //  Kraken immunity
            if(card1.Type == 'M' && card2.Type == 'S')
            {
                if(card1.Speciality == 'K')
                {
                    _battleLog += $"Kraken are immune to spells! {card1.Name} gains immunity against {card2.Name}!\n";
                    damage2 = 0;
                    return Fight(card1, card2, damage1, damage2);
                }
            }
            if (card1.Type == 'S' && card2.Type == 'M')
            {
                if (card2.Speciality == 'K')
                {
                    _battleLog += $"Kraken are immune to spells! {card2.Name} gains immunity against {card1.Name}!\n";
                    damage2 = 0;
                    return Fight(card1, card2, damage1, damage2);
                }
            }

            //  Knight drowning
            if (card1.Speciality == 'N' && element2 == 'W')
            {
                _battleLog += $"The {card1.Name} drowned instantly because of his heavy armor!";
                damage1 = 0;
                return Fight(card1, card2, damage1, damage2);
            }
            if (card1.Speciality == 'W' && element2 == 'N')
            {
                _battleLog += $"The {card2.Name} drowned instantly because of his heavy armor!";
                damage2 = 0;
                return Fight(card1, card2, damage1, damage2);
            }

            //  Normal fight
            if (element1 == element2)
            {
                _battleLog += $"Both cards are of element *{element1}*. No additional bonuses apply.\n";
                return Fight(card1, card2, damage1, damage2);
            }

            switch(element1)
            {
                case 'W':

                    if(element2 == 'F')
                    {
                        _battleLog += $"{card1.Name} is of element *Water*.\n";
                        _battleLog += $"{card2.Name} is of element *Fire*.\n";
                        _battleLog += $"{card1.Name} does double damage!\n";
                        damage1 *= multiplier;
                    }
                    else
                    {
                        _battleLog += $"{card1.Name} is of element *Water*.\n";
                        _battleLog += $"{card2.Name} is of element *Normal*.\n";
                        _battleLog += $"{card1.Name} does half damage!\n";
                        damage1 /= multiplier;
                    }
                    break;

                case 'F':

                    if(element2 == 'W')
                    {
                        _battleLog += $"{card1.Name} is of element *Fire*.\n";
                        _battleLog += $"{card2.Name} is of element *Water*.\n";
                        _battleLog += $"{card1.Name} does half damage!\n";
                        damage1 /= multiplier;
                    }
                    else
                    {
                        _battleLog += $"{card1.Name} is of element *Fire*.\n";
                        _battleLog += $"{card2.Name} is of element *Normal*.\n";
                        _battleLog += $"{card1.Name} does double damage!\n";
                        damage1 *= multiplier;
                    }
                    break;

                case 'N':

                    if(element2 == 'F')
                    {
                        _battleLog += $"{card1.Name} is of element *Normal*.\n";
                        _battleLog += $"{card2.Name} is of element *Fire*.\n";
                        _battleLog += $"{card1.Name} does half damage!\n";
                        damage1 /= multiplier;
                    }
                    else
                    {
                        _battleLog += $"{card1.Name} is of element *Normal*.\n";
                        _battleLog += $"{card2.Name} is of element *Water*.\n";
                        _battleLog += $"{card1.Name} does double damage!\n";
                        damage1 *= multiplier;
                    }
                    break;

                default:
                    return -1;

            }

            return Fight(card1, card2, damage1, damage2);
        }

        public int Fight(Card card1, Card card2, double damage1, double damage2)
        {
            _battleLog += $"{card1.Name} *---* {card2.Name}\n";

            if (damage1 > damage2)
            {
                _battleLog += $"{card1.Damage} ---> {card2.Damage}\n";
                _battleLog += $"{damage1} ---> {damage2}\n";
                return 1;
            }
            else if (damage1 < damage2)
            {
                _battleLog += $"{card1.Damage} <--- {card2.Damage}\n";
                _battleLog += $"{damage1} <--- {damage2}\n";
                return 2;
            }
            else
            {
                _battleLog += $"{card1.Damage} ==== {card2.Damage}\n";
                _battleLog += $"{damage1} ==== {damage2}\n";
                return 0;
            }
        }

        public void WinnerReward(User user)
        {
            user.Stats.UpdateElo(_eloPerBattle);
            user.Stats.UpdateCoins(_coinsPerWin);
            user.Stats.UpdateWins();
        }

        public void LoserPenality(User user)
        {
            user.Stats.UpdateElo(-_eloPerBattle);
            user.Stats.UpdateLosses();
        }
    }
}
