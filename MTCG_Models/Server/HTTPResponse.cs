using Npgsql.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.MTCG_Models.Server
{
    public class HTTPResponse
    {
        //  Method that is responsible for handling the correct HTTP status messages
        public static string GetStatusMessage(int statusCode)
        {
            return statusCode switch
            {
                200 => "OK",
                201 => "Created",
                400 => "Bad Request",
                401 => "Unauthorized",
                404 => "Not Found",
                405 => "Method Not Allowed",
                409 => "Conflict",
                500 => "Internal Server Error",
                _ => "Unknown Status"
            };
        }

        public static string GetHeader(int statusCode)
        {
            string status = GetStatusMessage(statusCode);
            return $"HTTP/1.1 {statusCode} {status}";
        }
    }
}
