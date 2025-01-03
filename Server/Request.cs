using MonsterTradingCardsGame.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.Server
{
    //  Class to store requests in
    public class Request
    {
        private readonly string _method;
        private readonly string _path;
        private readonly string _version;
        private Dictionary<string, string> _headers;
        private Dictionary<string, string> _body;
        private List<Card> _cards;
        private List<string> _cardIds;

        public Request(string method, string path, string ver)
        {
            _method = method;
            _path = path;
            _version = ver;
        }

        public void SetHeaders(Dictionary<string, string> headers)
        {
            _headers = headers;
        }

        public void SetBody(Dictionary<string, string> body)
        {
            _body = body;
        }

        public void SetCardIds(List<string> cardIds)
        {
            _cardIds = cardIds;
        }

        public void SetCards(List<Card> cards)
        {
            _cards = cards;
        }

        public string GetMethod()
        {
            return _method;
        }

        public string GetPath()
        {
            return _path;
        }

        public Dictionary<string, string> GetHeaders()
        {
            return _headers;
        }

        public Dictionary<string, string> GetBody()
        {
            return _body;
        }

        public List<Card> GetCards()
        {
            return _cards;
        }

        public List<string> GetCardIds()
        {
            return _cardIds;
        }
    }
}
