using MonsterTradingCardsGame.MTCG_Models.Models;
using Npgsql.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.MTCG_Models.Server
{
    public class Response
    {
        private readonly int _statusCode;
        private readonly string _status;
        private readonly string _contentType;
        private string _token;
        private string _body;
        private readonly string _bodyContent;
        private bool bodyIsList = false;

        //  For prettier formatting of json serialization
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };


        public Response(int statusCode, string message)
        { 
            _statusCode = statusCode;
            _status = HTTPResponse.GetHeader(_statusCode);
            _contentType = $"Content-Type: application/json\r\n";
            _bodyContent = message;
        }

        public Response(int statusCode, List<Card> cards)
        {
            _statusCode = statusCode;
            _status = HTTPResponse.GetHeader(_statusCode);
            _contentType = $"Content-Type: application/json\r\n";

            _bodyContent = JsonSerializer.Serialize(cards, _jsonOptions);
            bodyIsList = true;
            Console.WriteLine(_bodyContent);
        }

        public void SetToken(string token)
        {
            _token = token;
        }

        public string GetResponse()
        {
            SetBody();
            return $"{_status}\r\n{_contentType}\r\n{_body}";
        }
        public void SetBody()
        {
            if(!bodyIsList)
            {
                if (Parser.CheckIfValidString(_token))
                {
                    _body = JsonSerializer.Serialize(new
                    {
                        message = _bodyContent,
                        token = _token
                    }, _jsonOptions);
                }
                else
                {
                    _body = JsonSerializer.Serialize(new
                    {
                        message = _bodyContent
                    }, _jsonOptions);
                }
            }
            else
            {
                if (Parser.CheckIfValidString(_token))
                {
                    _body = JsonSerializer.Serialize(new
                    {
                        message = JsonSerializer.Deserialize<JsonArray>(_bodyContent),
                        token = _token
                    }, _jsonOptions);
                }
                else
                {
                    _body = JsonSerializer.Serialize(new
                    {
                        message = JsonSerializer.Deserialize<JsonArray>(_bodyContent)
                    }, _jsonOptions);
                }
            }
        }

        public bool CheckIfUserError()
        {
            return _statusCode >= 400 && _statusCode < 500;
        }

        public bool CheckIfServerError()
        {
            return _statusCode >= 500;
        }
    }
}
