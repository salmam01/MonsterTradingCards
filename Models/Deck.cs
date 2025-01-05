using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.Models
{
    public class Deck
    {
        public List<Card> DeckCards;

        public Deck(List<Card> deckCards)
        {
            DeckCards = deckCards;
        }

        public Card GetRandomCard()
        {
            Random random = new();
            return DeckCards[random.Next(DeckCards.Count)];
        }

        public void AddCardToDeck(Card card)
        {
            DeckCards.Add(card);
        }

        public void RemoveCardFromDeck(Card card)
        {
            DeckCards.Remove(card);
        }
    }
}
