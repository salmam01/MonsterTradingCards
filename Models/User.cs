using System;
using System.Drawing;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json.Serialization;

namespace MonsterTradingCardsGame.Models
{
    public class User
    {
        public string Username { get; set; }
        [JsonIgnore] public Guid UserID { get; set; }
        public string Bio { get; set; }
        public string Image { get; set; }
        public string Name { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] public UserStats Stats { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] public Deck UserDeck { get; set; }      
        
        public User(string username)
        {
            Username = username;
        }
        
    }
}
