using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.Models
{
    public class Stack
    {
        public List<Card> StackCards;

        public Stack(List<Card> stackCards) 
        {
            StackCards = stackCards;
        }
    }
}
