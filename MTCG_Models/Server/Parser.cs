using MonsterTradingCardsGame.MTCG_Models.Models;
using MonsterTradingCardsGame.MTCG_Models.Server;
using Npgsql.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.MTCG_Models.Server
{
    public class Parser
    {
        private readonly string[] _requestStrLines;
        private string _bodyStr;
        private Request _request;

        public Parser(string requestStr)
        {
            //  Split the request string by each line
            _requestStrLines = requestStr.Split("\r\n");
        }

        public Request ParseRequest()
        {
            if (_requestStrLines.Length == 0)
            {
                Console.WriteLine("Malformed Request Syntax.");
                return _request;
            }

            //  First line of the HTTP request contains the method, path and HTTP version
            string[] firstLine = _requestStrLines[0].Split(" ");
            _request = new(firstLine[0].Trim(), firstLine[1].Trim(), firstLine[2].Trim());
            
            ParseHeaders();
            if (_request.GetHeaders() == null)
            {
                Console.WriteLine("Empty headers.");
                return _request;
            }

            return _request;
        }

        public void ParseHeaders()
        {
            //  Split the header into pairs and add them to the dictionary
            Dictionary<string, string> requestData = new();

            int i = 1;
            while (i < _requestStrLines.Length && _requestStrLines[i] != "")
            {
                string[] headerParts = _requestStrLines[i].Split(':', 2);

                if (headerParts.Length == 2)
                {
                    requestData[headerParts[0].Trim()] = headerParts[1].Trim();
                }
                i++;
            }
            i++;

            _request.SetHeaders(requestData);
            _bodyStr = string.Join("\r\n", _requestStrLines.Skip(i));
            _bodyStr = _bodyStr.Trim();
        }

        public Request ParseBody()
        {
            try
            {
                if (!CheckIfValidString(_bodyStr))
                {
                    Console.WriteLine("Invalid body string.");
                }
                _request.SetBody(JsonSerializer.Deserialize<Dictionary<string, string>>(_bodyStr));

                if (_request.GetBody() == null)
                {
                    Console.WriteLine("Empty Body.");
                }
            }
            catch (JsonException e)
            {
                Console.WriteLine("An error occured during Deserialization: " + e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Parsing request body failed: " + e.Message);
            }
            return _request;

        }

        public Request ParseCards()
        {
            try
            {
                if (!CheckIfValidString(_bodyStr))
                {
                    Console.WriteLine("Invalid body string.");
                }
                _request.SetCards(JsonSerializer.Deserialize<List<Card>>(_bodyStr));

                if (_request.GetCards() == null)
                {
                    Console.WriteLine("Empty Cards.");
                }

                foreach (Card card in _request.GetCards())
                {
                    Console.WriteLine(card.Name);
                }
            }
            catch (JsonException e)
            {
                Console.WriteLine("An error occured during Deserialization: " + e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Parsing request body failed: " + e.Message);
            }
            return _request;
        }

        public Request ParseCardIds()
        {
            try
            {
                if (!CheckIfValidString(_bodyStr))
                {
                    Console.WriteLine("Invalid body string.");
                }
                _request.SetCardIds(JsonSerializer.Deserialize<List<string>>(_bodyStr));

                if (_request.GetCards() == null)
                {
                    Console.WriteLine("Empty Card Ids.");
                }

                foreach (string cardId in _request.GetCardIds())
                {
                    Console.WriteLine(cardId);
                }
            }
            catch (JsonException e)
            {
                Console.WriteLine("An error occured during Deserialization: " + e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Parsing request body failed: " + e.Message);
            }
            return _request;
        }

        public Request ParseUserData()
        {
            try
            {
                if (!CheckIfValidString(_bodyStr))
                {
                    Console.WriteLine("Invalid body string.");
                }
                _request.SetCardIds(JsonSerializer.Deserialize<List<string>>(_bodyStr));

                if (_request.GetCards() == null)
                {
                    Console.WriteLine("Empty Card Ids.");
                }

                foreach (string cardId in _request.GetCardIds())
                {
                    Console.WriteLine(cardId);
                }
            }
            catch (JsonException e)
            {
                Console.WriteLine("An error occured during Deserialization: " + e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Parsing request body failed: " + e.Message);
            }
            return _request;
        }

        public string ExtractToken(string tokenStr)
        {
            string token = tokenStr.Trim();
            return token.Substring(7);
        }

        /*public string ExtractUsername(string path)
        {
            if()
            {

            }
        }*/

        //  Helper method to check for an empty string
        public static bool CheckIfValidString(string str)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str) || str.Length <= 0)
            {
                return false;
            }
            return true;
        }
    }
}
