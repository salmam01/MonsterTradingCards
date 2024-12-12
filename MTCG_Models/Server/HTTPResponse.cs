using Npgsql.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.MTCG_Models.Server
{
    public class HTTPResponse
    {
        public HTTPResponse(int statusCode, string status, string message) 
        {
            string header = $"HTTP/1.1 {statusCode} {status}";
            string response = JsonSerializer.Serialize(new
            {
                message
            });
        }

        //  Method that is responsible for handling the correct HTTP status messages
        public static void Response(StreamWriter writer, int statusCode)
        {
            //  Send Response back to client based on the status code
            string message;
            bool stopServer = false;

            switch (statusCode)
            {
                case 200:
                    writer.WriteLine($"HTTP/1.1 {statusCode} OK");
                    message = "Welcome to Monster Trading Cards Game!";
                    break;

                case 201:
                    writer.WriteLine($"HTTP/1.1 {statusCode} Created");
                    message = "User created successfully.";
                    break;

                case 400:
                    writer.WriteLine($"HTTP/1.1 {statusCode} Bad Request");
                    message = "Username or Password cannot be empty.";
                    break;

                case 401:
                    writer.WriteLine($"HTTP/1.1 {statusCode} Unauthorized");
                    message = "Invalid Client Authentication Token.";
                    stopServer = true;
                    break;

                case 404:
                    writer.WriteLine($"HTTP/1.1 {statusCode} Not Found");
                    message = "Incorrect Username or Password.";
                    break;

                case 405:
                    writer.WriteLine($"HTTP/1.1 {statusCode} Method Not Allowed");
                    message = "Method not supported";
                    stopServer = true;
                    break;

                case 409:
                    writer.WriteLine($"HTTP/1.1 {statusCode} Conflict");
                    message = "Username already exists.";
                    break;

                case 500:
                    writer.WriteLine($"HTTP/1.1 {statusCode} Internal Server Error");
                    message = "Internal Server error occured.";
                    stopServer = true;
                    break;

                //  Unexpected status codes
                default:
                    writer.WriteLine($"HTTP/1.1 {statusCode} Unknown Status");
                    message = "Client requested unknown status.";
                    stopServer = true;
                    break;
            }

            writer.WriteLine("Content-Type: application/json");
            writer.WriteLine();
            writer.WriteLine(message);

            //  Not rlly sure how to do this yet
            //  Gotta stop server
            if (stopServer)
            {

            }
        }

        public void GetResponseMessage(int statusCode)
        {
            switch(statusCode)
            {

            }
        }
    }
}
