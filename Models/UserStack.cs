using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.Models
{
    public class UserStack
    {
        public List<Card> StackCards;

        public UserStack(List<Card> stackCards) 
        {
            StackCards = stackCards;
        }
    }
}
