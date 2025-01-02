using System;

namespace MonsterTradingCardsGame.MTCG_Models.Models
{
    public class ScoreBoard
    {
        public string Username { get; set; }
        public int Elo { get; set; }
        public ScoreBoard(string username, int elo)
        {
            Username = username;
            Elo = elo;
        }
    }
}
