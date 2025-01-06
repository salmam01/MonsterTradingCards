using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.Models
{
    public class UserDeck
    {
        public List<Card> DeckCards;
        [JsonIgnore] public List<Card> AddedCards;
        [JsonIgnore] public List<Card> DeletedCards;


        public UserDeck(List<Card> deckCards)
        {
            DeckCards = deckCards;
            AddedCards = new();
            DeletedCards = new();
        }

        public Card GetRandomCard()
        {
            Random random = new();
            return DeckCards[random.Next(DeckCards.Count)];
        }

        public void AddCardToDeck(Card card)
        {
            DeckCards.Add(card);
            AddedCards.Add(card);
        }

        public void RemoveCardFromDeck(Card card)
        {
            DeckCards.Remove(card);
            DeletedCards.Add(card);
        }
    }
}
