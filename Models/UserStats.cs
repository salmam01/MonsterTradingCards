using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.Models
{
    public class UserStats
    {
        public int Elo { get; set; }
        public int Coins { get; set; }
        public int GamesPlayed { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }

        public UserStats(int elo, int coins, int gamesPlayed, int wins, int losses) 
        {
            Elo = elo;
            Coins = coins;
            GamesPlayed = gamesPlayed;
            Wins = wins;
            Losses = losses;
        }

        public void UpdateElo(int amount)
        {
            if(Elo + amount <= 0)
            {
                Elo = 0;
            }
            else
            {
                Elo += amount;
            }
        }

        public void UpdateCoins(int amount)
        {
            Coins += amount;
        }

        public void UpdateWins()
        {
            GamesPlayed++;
            Wins++;
        }

        public void UpdateLosses()
        {
            GamesPlayed++;
            Losses++;
        }

    }
}
