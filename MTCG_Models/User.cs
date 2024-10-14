using MTCG_Models;
using System;

namespace MTCG_Model
{
    public class User
    {
        private string _username;
        private string _password;
        private int _coins;
        private int _elo;

        //  All currently owned cards
        private List<Card> _stack;
        //  Selected cards for battle by user
        private Card[] _deck = new Card[4];


        public User(string username, string password)
        {
            this._username = username;
            this._password = password;
            this._elo = 100;
            this._coins = 20;
            this._stack = new List<Card>();
            this._deck = new Card[4];
        }

        public string GetUsername { get { return _username; } }
        public string GetPassword { get { return _password; } }
        public int GetElo { get { return _elo; } }
        public int GetCoins { get { return _coins; } }
        //public List<Card> GetStack { get { return stack; } }
        //public Card[] GetDeck { get {   return deck; } }

        /*public void AddCardToStack(Card card)
        {
            this.stack.Add(card);
        }*/

        public bool Login(string username, string password)
        {
            if (username == null || password == null)
            {
                Console.WriteLine("Username or password cannot be empty!");
                return false;
            }
            if (username == this._username && password == this._password)
            {
                Console.WriteLine("Login successful!");
                return true;
            }

            Console.WriteLine("Credentials do not match!");
            return false;
        }

        public bool IsDeckFree()
        {
            if(this._deck.Length == 0) return true;

            for(int i = 0; i < this._deck.Length; i++)
            {
                if (this._deck[i] == null)
                {
                    return true;
                }
            }
            return false;
        }

        //  Don't forget to calculate the trueposition before function call
        public bool CheckPosition(int position)
        {
            if (position < 0 || position >= this._deck.Length)
            {
                Console.WriteLine("Invalid position. Please choose a position between 1 and 4.");
                return false;
            }
            if (this._deck[position] != null)
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
            if(this._deck.Length == 0)
            {
                Console.WriteLine("Deck is empty.");
                return;
            }
            for (int i = 0; i < this._deck.Length; i++)
            {
                Console.WriteLine((i + 1) + "." + this._deck[i]);
            }
        }

        public void printStack()
        {
            for (int i = 0; i < this._stack.Count; i++)
            {
                Console.WriteLine((i + 1) + ". " + this._stack[i]);
            }
        }

        /*public void Attack(User enemy)
        {
           
        }*/
    }
}
