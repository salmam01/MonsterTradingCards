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
        private readonly string _status;
        private readonly string _contentType;
        private readonly string _message;

        public Response(int statusCode, string contentType, string message)
        { 
            _status = HTTPResponse.GetHeader(statusCode);
            _contentType = $"Content-Type: {contentType}\r\n";
            _message = JsonSerializer.Serialize(new
            {
                message
            });
        }

        public string GetResponse()
        {
            return $"{_status}\r\n{_contentType}\r\n{_message}";
        }

    }
}
