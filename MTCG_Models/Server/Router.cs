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
        private Response _response;
        private Request _request;
        private readonly Parser _parser;
        private readonly UserManagement _userManagement;

        public Router(string requestStr)
        {
            _parser = new(requestStr);
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
            if (!Parser.CheckIfValidString(_request.GetMethod()) || !Parser.CheckIfValidString(_request.GetPath()))
            {
                return _response = new(400, "Method or Path is missing from the request.");
            }
            if (_request.GetHeaders() == null)
            {
                return _response = new(400, "Headers or Body are missing from the request.");
            }

            MethodHandler();
            return _response;
        }

        public void MethodHandler()
        {
            string method = _request.GetMethod();
            string path = _request.GetPath();

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

            int statusCode;
            string message;
            _request = _parser.ParseBody();
            string token = _parser.ExtractToken(_request.GetHeaders()["Authorization"]);

            switch (path)
            {
                case "/":
                    statusCode = 200;
                    message = "Welcome to Monster Trading Cards!";
                    break;

                case "/cards":
                    //  To be implemented
                    Console.WriteLine("Invalid path.");
                    statusCode = 404;
                    message = "Invalid path.";
                    break;

                case "/deck":
                    //  To be implemented
                    Console.WriteLine("Invalid path.");
                    statusCode = 404;
                    message = "Invalid path.";
                    break;

                case "/users":
                    (statusCode, message) = _userManagement.GetUserData(_request.GetBody(), token);
                    break;

                //  Path does not exist, send Error Response Code to Client
                default:
                    Console.WriteLine("Invalid path.");
                    statusCode = 404;
                    message = "Invalid path.";
                    break;
            }
            _response = new(statusCode, message);
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
                case "/users":
                    _request = _parser.ParseBody();
                    (statusCode, message) = _userManagement.SignUp(_request.GetBody());
                    break;

                case "/sessions":
                    _request = _parser.ParseBody();
                    (statusCode, message, token) = _userManagement.Login(_request.GetBody());
                    
                    break;

                case "/packages":
                    _request = _parser.ParseCards();
                    token = _parser.ExtractToken(_request.GetHeaders()["Authorization"]);
                    (statusCode, message) = _userManagement.CreatePackage(_request.GetCards(), token);
                    break;

                case "/transactions":
                    _request = _parser.ParseBody();
                    Console.WriteLine("Invalid path.");
                    statusCode = 404;
                    message = "Invalid path.";
                    break;

                //  Path does not exist, send Error Response Code to Client
                default:
                    Console.WriteLine("Invalid path.");
                    statusCode = 404;
                    message = "Invalid path.";
                    break;
            }

            _response = new(statusCode, message);
            if(string.IsNullOrEmpty(token) || string.IsNullOrWhiteSpace(token) || token.Length <= 0)
            {
                _response.SetToken(token);
            }
            
        }

        //  PUT and DELETE will be implemented later
        public void PutRequestHandler(string path)
        {
            Console.WriteLine($"Handling POST Request for {path}...");
            //string token = _parser.ExtractToken(_request.GetHeaders()["Authorization"]);

            switch (path)
            {
                case "/deck":
                    _request = _parser.ParseBody();
                    Console.WriteLine("Invalid path.");
                    break;

                default:
                    _request = _parser.ParseBody();
                    Console.WriteLine("Invalid path.");
                    break;
            }

            _response = new(404, "Invalid path.");

        }

        public void DeleteRequestHandler(string path)
        {
            Console.WriteLine($"Handling POST Request for {path}...");
            //string token = _parser.ExtractToken(_request.GetHeaders()["Authorization"]);

            switch (path)
            {
                case "/deck":
                    _parser.ParseBody();
                    Console.WriteLine("Invalid path.");
                    break;

                default:
                    _parser.ParseBody();
                    Console.WriteLine("Invalid path.");
                    break;
            }
            _response = new(404, "Invalid path.");
        }
    }
}
