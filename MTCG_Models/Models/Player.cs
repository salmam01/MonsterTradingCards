using System;

namespace MonsterTradingCardsGame.MTCG_Models.Models
{
    public class Player
    {
        private string _username;
        private string _password;
        private int _coins;
        private int _elo;

        //  All currently owned cards
        private List<Card> _stack;
        //  Selected cards for battle by user
        private Card[] _deck = new Card[4];


        public Player(string username, string password)
        {
            _username = username;
            _password = password;
            _elo = 100;
            _coins = 20;
            _stack = new List<Card>();
            _deck = new Card[4];
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

        public bool IsDeckFree()
        {
            if (_deck.Length == 0) return true;

            for (int i = 0; i < _deck.Length; i++)
            {
                if (_deck[i] == null)
                {
                    return true;
                }
            }
            return false;
        }

        //  Don't forget to calculate the trueposition before function call
        public bool CheckPosition(int position)
        {
            if (position < 0 || position >= _deck.Length)
            {
                Console.WriteLine("Invalid position. Please choose a position between 1 and 4.");
                return false;
            }
            if (_deck[position] != null)
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
            if (_deck.Length == 0)
            {
                Console.WriteLine("Deck is empty.");
                return;
            }
            for (int i = 0; i < _deck.Length; i++)
            {
                Console.WriteLine(i + 1 + "." + _deck[i]);
            }
        }

        public void printStack()
        {
            for (int i = 0; i < _stack.Count; i++)
            {
                Console.WriteLine(i + 1 + ". " + _stack[i]);
            }
        }
    }
}
