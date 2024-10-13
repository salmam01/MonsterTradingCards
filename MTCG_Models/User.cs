using MTCG_Models;
using System;

namespace MTCG_Model
{
    public class User
    {
        public User(string username, string password)
        {
            this.username = username;
            this.password = password;
            this.elo = 100;
            this.coins = 20;
            this.stack = new List<Card>();
            this.deck = new Card[4];
        }

        private string username;
        private string password;
        private int coins;
        private int elo;

        //  All currently owned cards
        private List<Card> stack;
        //  Selected cards for battle by user
        private Card[] deck = new Card[4];

        public string GetUsername { get { return username; } }
        public string GetPassword { get { return password; } }
        public int GetElo { get { return elo; } }
        public int GetCoins { get { return coins; } }
        //public List<Card> GetStack { get { return stack; } }
        //public Card[] GetDeck { get {   return deck; } }

        /*public void AddCardToStack(Card card)
        {
            this.stack.Add(card);
        }*/

        public bool IsDeckFree()
        {
            if(this.deck.Length == 0) return true;

            for(int i = 0; i < this.deck.Length; i++)
            {
                if (this.deck[i] == null)
                {
                    return true;
                }
            }
            return false;
        }

        //  Don't forget to calculate the trueposition before function call
        public bool CheckPosition(int position)
        {
            if (position < 0 || position >= this.deck.Length)
            {
                Console.WriteLine("Invalid position. Please choose a position between 1 and 4.");
                return false;
            }
            if (this.deck[position] != null)
            {
                Console.WriteLine("There is already a card at that position.");
                return false;
            }
            return true;
        }

        /*public void AddCardToDeck(Card card, int position)
        {
            this.deck[position] = card;
            Console.WriteLine("Card added to Deck at Position " + position + ".");
        }
      
        public void SwitchDeckCards(Card card)
        {
            PrintDeck();
            Console.WriteLine("Please select the card you would like to switch.");
            int position = Convert.ToInt32(Console.ReadLine());

            Card temp = this.deck[position];
            this.deck[position] = card;
            AddCardToStack(temp);
            PrintDeck();
        }
        */
        public void PrintDeck()
        {
            if(this.deck.Length == 0)
            {
                Console.WriteLine("Deck is empty.");
                return;
            }
            for (int i = 0; i < this.deck.Length; i++)
            {
                Console.WriteLine((i + 1) + "." + this.deck[i]);
            }
        }

        public void printStack()
        {
            for (int i = 0; i < this.stack.Count; i++)
            {
                Console.WriteLine((i + 1) + ". " + this.stack[i]);
            }
        }

        /*public void Attack(User enemy)
        {
           
        }*/
    }
}
