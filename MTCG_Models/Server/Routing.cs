using MonsterTradingCardsGame.MTCG_Models.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.MTCG_Models.Server
{
    public class Routing
    {
        //  Method that redirects users depending on the HTTP method
        public static void Router(StreamWriter writer, Dictionary<string, string> request)
        {
            string method = request["Method"];

            switch (method)
            {
                case "GET":
                    GetRequestHandler(writer, request);
                    break;

                case "POST":
                    PostRequestHandler(writer, request);
                    break;

                case "PUT":
                    PutRequestHandler(writer, request);
                    HTTPResponse.Response(writer, 405);
                    break;

                case "DELETE":
                    DeleteRequestHandler(writer, request);
                    HTTPResponse.Response(writer, 405);
                    break;

                default:
                    HTTPResponse.Response(writer, 405);
                    break;
            }
        }

        //  Method that handles GET Requests
        public static void GetRequestHandler(StreamWriter writer, Dictionary<string, string> request)
        {
            string path = request["Path"];
            Console.WriteLine($"Handling GET Request for {path}...");

            switch (path)
            {
                case "/":
                    HTTPResponse.Response(writer, 200);
                    break;

                case "/cards":
                    //  To be implemented
                    break;

                case "/deck":
                    //  To be implemented
                    break;

                //  Path does not exist, send Error Response Code to Client
                default:
                    HTTPResponse.Response(writer, 404);
                    break;
            }
        }

        //  REWORK THIS SHIT PLS
        //  Method that handles POST Requests
        public static void PostRequestHandler(StreamWriter writer, Dictionary<string, string> request)
        {
            string path = request["Path"];
            Console.WriteLine($"Handling POST Request for {path}...");

            int statusCode;
            string token = "";
            switch (path)
            {
                //  API-Endpoint for sign up
                case "/users":

                    Console.WriteLine($"Redirecting to {path}.");
                    statusCode = UserManagement.Register(request);

                    Console.WriteLine($"Status: {statusCode}");
                    HTTPResponse.Response(writer, statusCode);
                    break;

                //  API-Endpoint for login
                case "/sessions":

                    Console.WriteLine($"Redirecting to {path}.");
                    statusCode = UserManagement.Login(request);

                    Console.WriteLine(statusCode);
                    HTTPResponse.Response(writer, statusCode);
                    break;

                case "/packages":
                    if (TokenManagement.CheckIfTokenIsValid(request["Username"], token))
                    {
                        //  To be implemented
                    }
                    break;

                case "/transactions":
                    break;

                //  Path does not exist, send Error Response Code to Client
                default:
                    Console.WriteLine("Path invalid.");
                    HTTPResponse.Response(writer, 404);
                    break;
            }
        }

        public static void PutRequestHandler(StreamWriter writer, Dictionary<string, string> request)
        {
            string path = request["Path"];
            Console.WriteLine($"Handling POST Request for {path}...");

            int statusCode;
            string token = null;

            switch (path)
            {
                case "/deck":
                    break;

                default:
                    Console.WriteLine("Path invalid.");
                    HTTPResponse.Response(writer, 404);
                    break;
            }
        }

        public static void DeleteRequestHandler(StreamWriter writer, Dictionary<string, string> request)
        {
            string path = request["Path"];
            Console.WriteLine($"Handling POST Request for {path}...");

            int statusCode;
            string token = null;

            switch (path)
            {
                case "/deck":
                    break;

                default:
                    Console.WriteLine("Path invalid.");
                    HTTPResponse.Response(writer, 404);
                    break;
            }

        }

    }
}
