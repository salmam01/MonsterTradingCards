using MonsterTradingCardsGame.MTCG_Models.Models;
using Npgsql.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
        private string _bodyContent;

        private string _token;
        private string _body;

        //  For prettier formatting of json serialization
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public Response(int statusCode, object bodyContent)
        {
            _statusCode = statusCode;
            _status = HTTPResponse.GetHeader(_statusCode);
            _contentType = $"Content-Type: application/json\r\n";

            SetBody(bodyContent);
            Console.WriteLine(_bodyContent);
        }

        public void SetBody(object bodyContent)
        {
            if (bodyContent is List<Card>) 
            {
                _bodyContent = SerializeBody(bodyContent);
                SetResponseWithList();
            }
            else if (bodyContent is string)
            {
                _bodyContent = (string)bodyContent;
                SetResponse();
            }
            else
            {
                _bodyContent = SerializeBody(bodyContent);
                SetResponse();
            }
        }

        public void SetResponse()
        {
            if(_token != null)
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

        public void SetResponseWithList()
        {
            if (_token != null)
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
                    message = JsonSerializer.Deserialize<JsonArray>(_bodyContent),
                }, _jsonOptions);
            }
        }

        public string SerializeBody(object bodyContent)
        {
            return JsonSerializer.Serialize(bodyContent, _jsonOptions);
        }

        public void SetToken(string token)
        {
            _token = token;
        }

        public string GetResponse()
        {
            return $"{_status}\r\n{_contentType}\r\n{_body}";
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
