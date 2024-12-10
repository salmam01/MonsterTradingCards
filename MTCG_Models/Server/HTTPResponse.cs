using Npgsql.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.MTCG_Models.Server
{
    public class HTTPResponse
    {
        //  Method that is responsible for handling the correct HTTP status messages
        public static void Response(StreamWriter writer, int statusCode, string message)
        {
            //  Send Response back to client based on the status code
            switch (statusCode)
            {
                case 200:
                    writer.WriteLine($"HTTP/1.1 {statusCode} OK");
                    break;

                case 201:
                    writer.WriteLine($"HTTP/1.1 {statusCode} Created");
                    break;

                case 400:
                    writer.WriteLine($"HTTP/1.1 {statusCode} Bad Request");
                    break;

                case 401:
                    writer.WriteLine($"HTTP/1.1 {statusCode} Unauthorized");
                    break;

                case 404:
                    writer.WriteLine($"HTTP/1.1 {statusCode} Not Found");
                    break;

                case 405:
                    writer.WriteLine($"HTTP/1.1 {statusCode} Method Not Allowed");
                    break;

                case 409:
                    writer.WriteLine($"HTTP/1.1 {statusCode} Conflict");
                    break;

                case 500:
                    writer.WriteLine($"HTTP/1.1 {statusCode} Internal Server Error");
                    break;

                //  Unexpected status codes
                default:
                    writer.WriteLine($"HTTP/1.1 {statusCode} Unknown Status");
                    message = "Client requested unknown status.";
                    break;
            }

            writer.WriteLine("Content-Type: application/json");
            writer.WriteLine();
            writer.WriteLine(message);
        }
    }
}
