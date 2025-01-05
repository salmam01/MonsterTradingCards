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
            Elo += amount;
        }

        public void UpdateCoins(int amount)
        {
            Coins += amount;
        }

        public void UpdateGamesPlayed(int amount)
        {
            GamesPlayed += amount;
        }

        public void UpdateWins(int amount)
        {
            Wins += amount;
        }

        public void UpdateLosses(int amount)
        {
            Losses += amount;
        }

    }
}
