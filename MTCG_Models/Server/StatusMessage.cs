using Npgsql.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.MTCG_Models.Server
{
    public class StatusMessage
    {
        //  Method that is responsible for handling the correct HTTP status messages
        public string GetStatusMessage(int statusCode)
        {
            return ((HttpStatusCode)statusCode).ToString();
        }

        public string GetHeader(int statusCode)
        {
            string status = GetStatusMessage(statusCode);
            return $"HTTP/1.1 {statusCode} {status}";
        }
    }
}
