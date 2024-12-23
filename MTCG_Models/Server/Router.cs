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
        private Dictionary<string, string> _request;
        private Response _response;
        private readonly Parser _parser;
        private readonly UserManagement _userManagement;

        public Router(string request)
        {
            _parser = new(request);
            _userManagement = new UserManagement();
        }

        //  Method that redirects users depending on the HTTP method
        public Response RequestHandler()
        {
            _request = _parser.ParseRequest();
            if (_request == null)
            {
                return _response = new(400, "Malformed Request Syntax.");
            }

            MethodHandler();
            return _response;
        }

        public void MethodHandler()
        {
            string method, path;
            if (!_request.ContainsKey("Method") || !_request.ContainsKey("Path"))
            {
                _response = new(400, "Request Body is empty.");
                return;
            }
            method = _request["Method"];
            path = _request["Path"];

            switch (method)
            {
                case "GET":
                    GetRequestHandler(path);
                    break;

                case "POST":
                    PostRequestHandler(path);
                    break;

                case "PUT":
                    PutRequestHandler(path);
                    break;

                case "DELETE":
                    DeleteRequestHandler(path);
                    break;

                default:
                    _response = new(405, "Method not supported.");
                    break;
            }
        }

        //  Method that handles GET Requests
        public void GetRequestHandler(string path)
        {
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
        public void PostRequestHandler(string path)
        {
            Console.WriteLine($"Handling POST Request for {path}...");

            int statusCode;
            string message;
            string token = "";
            switch (path)
            {
                //  API-Endpoint for sign up
                case "/users":
                    (statusCode, message) = _userManagement.Register(_request);
                    _response = new(statusCode, message);
                    break;

                //  API-Endpoint for login
                case "/sessions":
                    (statusCode, message) = _userManagement.Login(_request);
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
        public void PutRequestHandler(string path)
        {
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

        public void DeleteRequestHandler(string path)
        {
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
