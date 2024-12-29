using MonsterTradingCardsGame.MTCG_Models.Database;
using MonsterTradingCardsGame.MTCG_Models.Server;
using MonsterTradingCardsGame.MTCG_Models.Services.Authentication;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.MTCG_Models.Services
{
    public class Router
    {
        private Response _response;
        private Request _request;
        private readonly Parser _parser;
        private readonly UserManagement _userManagement;
        private readonly PackageManagement _packageManagement;
        private readonly NpgsqlConnection _connection;

        public Router(string requestStr, int shopId)
        {
            DatabaseConnection dbConn = new();
            _connection = dbConn.OpenConnection();
            _parser = new(requestStr);
            _userManagement = new(_connection);
            _packageManagement = new(_connection, shopId);
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
            _connection.Close();
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
            _request = _parser.ParseBody();
            string token = _parser.ExtractToken(_request.GetHeaders()["Authorization"]);

            switch (path)
            {
                case "/":
                    _response = new(200, "Welcome to Monster Trading Cards!");
                    break;

                case "/cards":
                    //  To be implemented
                    Console.WriteLine("Invalid path.");
                    _response = new(404, "Invalid path.");
                    break;

                case "/deck":
                    //  To be implemented
                    Console.WriteLine("Invalid path.");
                    _response = new(404, "Invalid path.");
                    break;

                case "/users/username":
                    //  To be implemented
                    //_response = _userManagement.GetUserData(_request.GetBody(), token);
                    _response = new(404, "Invalid path.");
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
            string token;

            switch (path)
            {
                case "/users":
                    _request = _parser.ParseBody();
                    _response = _userManagement.SignUp(_request.GetBody());
                    break;

                case "/sessions":
                    _request = _parser.ParseBody();
                    _response = _userManagement.Login(_request.GetBody());
                    break;

                case "/packages":
                    _request = _parser.ParseCards();
                    token = _parser.ExtractToken(_request.GetHeaders()["Authorization"]);
                    
                    if (_userManagement.CheckIfTokenIsValid(token) && _userManagement.CheckIfAdmin(token))
                    {
                        _response = _packageManagement.CreatePackage(_request.GetCards());
                    }
                    else
                    {
                        _response = new(401, "Unauthorized.");
                    }
                    break;

                case "/transactions/packages":
                    token = _parser.ExtractToken(_request.GetHeaders()["Authorization"]);
                    if (_userManagement.CheckIfTokenIsValid(token))
                    {
                        Guid? userId = _userManagement.GetUserId(token);
                        if(userId != null)
                        {
                            _response = _userManagement.AquirePackage();
                        }
                        else
                        {
                            Console.WriteLine("Username is null.");
                            _response = new(500, "Internal Server Error occured.");
                        }
                    }
                    else
                    {
                        _response = new(401, "Unauthorized.");
                    }
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
            //string token = _parser.ExtractToken(_request.GetHeaders()["Authorization"]);

            switch (path)
            {
                case "/deck":
                    _request = _parser.ParseBody();
                    Console.WriteLine("Invalid path.");
                    _response = new(404, "Invalid path.");
                    break;

                default:
                    _request = _parser.ParseBody();
                    Console.WriteLine("Invalid path.");
                    _response = new(404, "Invalid path.");
                    break;
            }
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
                    _response = new(404, "Invalid path.");
                    break;

                default:
                    _parser.ParseBody();
                    Console.WriteLine("Invalid path.");
                    _response = new(404, "Invalid path.");
                    break;
            }
        }
    }
}
