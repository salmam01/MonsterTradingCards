using MonsterTradingCardsGame.MTCG_Models.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.MTCG_Models.Server
{
    public class Router
    {
        private readonly Dictionary<string, string> _request;
        private Response _response;
        public Router(string request)
        {
            Parser parser = new(request);
            _request = parser.ParseRequest();
        }

        //  Method that redirects users depending on the HTTP method
        public Response HandleRequest()
        {
            string contentType = "application/json";

            if (_request == null)
            {
                
                return _response = new Response(400, contentType, "Bad Request");;
            }

            MethodHandler();
            return _response;
        }

        public Response MethodHandler()
        {
            string method = _request["method"];
            switch (method)
            {
                case "GET":
                    GetRequestHandler();
                    break;

                case "POST":
                    PostRequestHandler();
                    break;

                case "PUT":
                    PutRequestHandler();
                    //HTTPResponse.Response(writer, 405);
                    break;

                case "DELETE":
                    DeleteRequestHandler();
                    //HTTPResponse.Response(writer, 405);
                    break;

                default:
                    //HTTPResponse.Response(writer, 405);
                    break;
            }
        }

        //  Method that handles GET Requests
        public Response GetRequestHandler()
        {
            string path = _request["Path"];
            Console.WriteLine($"Handling GET Request for {path}...");

            switch (path)
            {
                case "/":
                    //HTTPResponse.Response(writer, 200);
                    break;

                case "/cards":
                    //  To be implemented
                    break;

                case "/deck":
                    //  To be implemented
                    break;

                //  Path does not exist, send Error Response Code to Client
                default:
                    //HTTPResponse.Response(writer, 404);
                    break;
            }
        }

        //  REWORK THIS SHIT PLS
        //  Method that handles POST Requests
        public Response PostRequestHandler()
        {
            string path = _request["Path"];
            Console.WriteLine($"Handling POST Request for {path}...");

            int statusCode;
            string token = "";
            switch (path)
            {
                //  API-Endpoint for sign up
                case "/users":

                    Console.WriteLine($"Redirecting to {path}.");
                    statusCode = UserManagement.Register(_request);

                    Console.WriteLine($"Status: {statusCode}");
                    //HTTPResponse.Response(writer, statusCode);
                    break;

                //  API-Endpoint for login
                case "/sessions":

                    Console.WriteLine($"Redirecting to {path}.");
                    statusCode = UserManagement.Login(_request);

                    Console.WriteLine(statusCode);
                    //HTTPResponse.Response(writer, statusCode);
                    break;

                case "/packages":
                    if (TokenManagement.CheckIfTokenIsValid(_request["Username"], token))
                    {
                        //  To be implemented
                    }
                    break;

                case "/transactions":
                    break;

                //  Path does not exist, send Error Response Code to Client
                default:
                    Console.WriteLine("Path invalid.");
                    //HTTPResponse.Response(writer, 404);
                    break;
            }
        }

        public void PutRequestHandler()
        {
            string path = _request["Path"];
            Console.WriteLine($"Handling POST Request for {path}...");

            int statusCode;
            string token = null;

            switch (path)
            {
                case "/deck":
                    break;

                default:
                    Console.WriteLine("Path invalid.");
                    //HTTPResponse.Response(writer, 404);
                    break;
            }
        }

        public void DeleteRequestHandler()
        {
            string path = _request["Path"];
            Console.WriteLine($"Handling POST Request for {path}...");

            int statusCode;
            string token = null;

            switch (path)
            {
                case "/deck":
                    break;

                default:
                    Console.WriteLine("Path invalid.");
                    //HTTPResponse.Response(writer, 404);
                    break;
            }

        }

    }
}
