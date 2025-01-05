using MonsterTradingCardsGame.Models;
using Npgsql.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.Server
{
    public class Response
    {
        private readonly int _statusCode;
        private readonly string _status;
        private readonly string _contentType;
        private readonly object _bodyObject;

        private string _token;
        private string _body;
        private User _battleRequest = null;

        //  Options for prettier formatting of JSON serialization
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public Response(int statusCode, object bodyObject)
        {
            _statusCode = statusCode;
            _status = GetStatusMessage();
            _contentType = $"Content-Type: application/json\r\n";
            _bodyObject = bodyObject;
        }

        public string GetStatusMessage()
        {
            StatusMessage statusMessage = new();
            return statusMessage.GetHeader(_statusCode);
        }

        public void SetToken(string token)
        {
            _token = token;
        }

        public void SetBody()
        {
            string message;
            if (_bodyObject is List<Card>)
            {
                message = SerializeBody();
                SetResponseWithList(message);
            }
            else if (_bodyObject is string)
            {
                message = (string)_bodyObject;
                SetResponse(message);
            }
            else
            {
                message = SerializeBody();
                SetResponse(message);
            }
        }
        public string SerializeBody()
        {
            return JsonSerializer.Serialize(_bodyObject, _jsonOptions);
        }

        public void SetResponse(string message)
        {
            if (_token != null)
            {
                _body = JsonSerializer.Serialize(new
                {
                    message = _bodyObject,
                    token = _token
                }, _jsonOptions);
            }
            else
            {
                _body = JsonSerializer.Serialize(new
                {
                    message = _bodyObject
                }, _jsonOptions);
            }
        }

        public void SetResponseWithList(string message)
        {
            if (_token != null)
            {
                _body = JsonSerializer.Serialize(new
                {
                    message = JsonSerializer.Deserialize<JsonArray>(message),
                    token = _token
                }, _jsonOptions);
            }
            else
            {
                _body = JsonSerializer.Serialize(new
                {
                    message = JsonSerializer.Deserialize<JsonArray>(message),
                }, _jsonOptions);
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

        public void SetBattleRequest(User user)
        {
            _battleRequest = user;
        }

        public User GetBattleRequest()
        {
            return _battleRequest;
        }

        public string GetResponse()
        {
            SetBody();
            return $"{_status}\r\n{_contentType}\r\n{_body}";
        }

        public object GetBodyObject()
        {
            return _bodyObject;
        }
    }
}
