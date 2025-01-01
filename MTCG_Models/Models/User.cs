using System;
using System.Drawing;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json.Serialization;

namespace MonsterTradingCardsGame.MTCG_Models.Models
{
    public class User
    {
        public string Username { get; set; }
        [JsonInclude][JsonPropertyName("Password")] private string _password;
        public string Bio { get; set; }
        public string Image { get; set; }
        public string Name { get; set; }

        public struct PlayerStats
        {
            public int Elo { get; set; }
            public int Coins { get; set; }
            public int GamesPlayed { get; set; }
            public int Wins { get; set; }
            public int Losses { get; set; }

            public PlayerStats(int elo, int coins, int gamesPlayed, int wins, int losses)
            {
                Elo = elo;
                Coins = coins;
                GamesPlayed = gamesPlayed;
                Wins = wins;
                Losses = losses;
            }
        }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] public PlayerStats Stats { get; set; } = new();
        public User() { }

        public User(string username, string password)
        {
            Username = username;
            _password = password;
        }

        public void SetUserStats(int elo, int coins, int gamesPlayed, int wins, int losses)
        {
            Stats = new PlayerStats(elo, coins, gamesPlayed, wins, losses);
        }

        [JsonIgnore] public string GetPassword { get { return _password; } }

    }
}
