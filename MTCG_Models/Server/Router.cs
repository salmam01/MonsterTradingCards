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
        private readonly UserManagement _userManagement;

        public Router(string request)
        {
            Parser parser = new(request);
            _request = parser.ParseRequest();
            _userManagement = new UserManagement();
        }

        //  Method that redirects users depending on the HTTP method
        public Response HandleRequest()
        {
            MethodHandler();

            if (_request == null)
            {
                return _response = new Response(400, "Bad Request");
            }                        
            return _response;
        }

        public void MethodHandler()
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
                    break;

                case "DELETE":
                    DeleteRequestHandler();
                    break;

                default:
                    _response = new(405, "Method not supported.");
                    break;
            }
        }

        //  Method that handles GET Requests
        public void GetRequestHandler()
        {
            string path = _request["Path"];
            Console.WriteLine($"Handling GET Request for {path}...");

            switch (path)
            {
                case "/":
                    _response = new(200, "Welcome to Monster Trading Cards!");
                    break;

                case "/cards":
                    //  To be implemented
                    break;

                case "/deck":
                    //  To be implemented
                    break;

                //  Path does not exist, send Error Response Code to Client
                default:
                    Console.WriteLine("Invalid path.");
                    _response = new(404, "Invalid path."); 
                    break;
            }
        }

        //  Method that handles POST Requests
        public void PostRequestHandler()
        {
            string path = _request["Path"];
            Console.WriteLine($"Handling POST Request for {path}...");

            int statusCode;
            string message;
            string token = "";
            switch (path)
            {
                //  API-Endpoint for sign up
                case "/users":
                    (statusCode, message) = _userManagement.Register(_request);
                    Console.WriteLine($"Status: {statusCode}, {message}");
                    _response = new(statusCode, message);
                    break;

                //  API-Endpoint for login
                case "/sessions":
                    (statusCode, message) = _userManagement.Login(_request);
                    Console.WriteLine($"Status: {statusCode}, {message}");
                    _response = new(statusCode, message);
                    break;

                case "/packages":
                    if (TokenManagement.CheckIfTokenIsValid(_request["Username"], token))
                    {
                        //  To be implemented
                    }

                    Console.WriteLine("Invalid path.");
                    _response = new(404, "Invalid path.");
                    break;

                case "/transactions":
                    Console.WriteLine("Invalid path.");
                    _response = new(404, "Invalid path.");
                    break;

                //  Path does not exist, send Error Response Code to Client
                default:
                    Console.WriteLine("Invalid path.");
                    _response = new(404, "Invalid path.");
                    break;
            }
        }

        //  PUT and DELETE will be implemented later
        public void PutRequestHandler()
        {
            string path = _request["Path"];
            Console.WriteLine($"Handling POST Request for {path}...");

            string token = null;

            switch (path)
            {
                case "/deck":
                    Console.WriteLine("Invalid path.");
                    _response = new(404, "Invalid path.");
                    break;

                default:
                    Console.WriteLine("Invalid path.");
                    _response = new(404, "Invalid path.");
                    break;
            }
        }

        public void DeleteRequestHandler()
        {
            string path = _request["Path"];
            Console.WriteLine($"Handling POST Request for {path}...");

            string token = null;

            switch (path)
            {
                case "/deck":
                    Console.WriteLine("Invalid path.");
                    _response = new(404, "Invalid path.");
                    break;

                default:
                    Console.WriteLine("Invalid path.");
                    _response = new(404, "Invalid path.");
                    break;
            }

        }
    }
}
