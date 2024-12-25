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
        private string _message;
        private readonly string _messageStr;
        private string _token;

        public Response(int statusCode, string message)
        { 
            _statusCode = statusCode;
            _status = HTTPResponse.GetHeader(_statusCode);
            _contentType = $"Content-Type: application/json\r\n";
            _messageStr = message;
        }

        public void SetToken(string token)
        {
            _token = token;
        }

        public string GetResponse()
        {
            SetMessage();
            return $"{_status}\r\n{_contentType}\r\n{_message}";
        }
        public void SetMessage()
        {
            if (Parser.CheckIfValidString(_token))
            {
                _message = JsonSerializer.Serialize(new
                {
                    message = _messageStr,
                    token = _token
                });
            }
            else
            {
                _message = JsonSerializer.Serialize(new
                {
                    message = _messageStr
                });
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
