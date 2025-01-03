﻿using MonsterTradingCardsGame.Database;
using MonsterTradingCardsGame.Services;
using MonsterTradingCardsGame.Services.Authentication;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.Server
{
    public class Router
    {
        private Response _response;
        private Request _request;
        private readonly Parser _parser;
        private readonly UserManagement _userManagement;
        private readonly PackageManagement _packageManagement;
        private DatabaseConnection _dbConnection;

        public Router(string requestStr, int shopId)
        {
            _dbConnection = new();
            _parser = new(requestStr);
            _userManagement = new(_dbConnection, shopId);
            _packageManagement = new(_dbConnection, shopId);
        }

        //  Method that redirects users depending on the HTTP method
        public Response RequestHandler()
        {
            _request = _parser.ParseRequest();
            if (_request == null)
            {
                return _response = new(400, "Malformed request syntax.");
            }
            if (_request.GetMethod() == null || _request.GetPath() == null)
            {
                return _response = new(400, "Malformed request syntax.");
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

            if (path == "/")
            {
                _response = new(200, "Welcome to Monster Trading Cards!");
                return;
            }

            string? token = _parser.ExtractToken(_request.GetHeaders()["Authorization"]);
            if (token == null)
            {
                Console.WriteLine("Unauthorized: invalid user token.");
                _response = new(401, "Unauthorized.");
                return;
            }

            //  Makes better sense to always return the object they are requesting instead of a list etc!!!
            if (_userManagement.CheckIfTokenIsValid(token))
            {
                string username = "";
                if (path.StartsWith("/users/"))
                {
                    username = _parser.ExtractUsername(path);
                }

                switch (path)
                {
                    case "/cards":
                        _response = _userManagement.GetUserStack(token);
                        break;

                    case "/deck":
                        _response = _userManagement.GetUserDeck(token);
                        break;

                    case "/deck?format=plain":
                        _response = _userManagement.GetUserDeck(token);
                        break;

                    case "/stats":
                        _response = _userManagement.GetUserStats(token);
                        break;

                    case var usernamePath when usernamePath == $"/users/{username}":
                        _response = _userManagement.GetUserData(token, username);
                        break;

                    case "/scoreboard":
                        _response = _userManagement.GetScoreboard();
                        break;

                    case "/tradings":
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
            else
            {
                Console.WriteLine("Unauthorized: invalid user token.");
                _response = new(401, "Unauthorized.");
            }
        }

        //  Method that handles POST Requests
        public void PostRequestHandler(string path)
        {
            //  Can be split into further methods to handle authenticated vs unauthenticated routes
            Console.WriteLine($"Handling POST Request for {path}...");

            if (path == "/users")
            {
                _request = _parser.ParseBody("dictionary");
                _response = _userManagement.SignUp(_request.GetBody());
                return;
            }
            if (path == "/sessions")
            {
                _request = _parser.ParseBody("dictionary");
                _response = _userManagement.Login(_request.GetBody());
                return;
            }

            string? token = _parser.ExtractToken(_request.GetHeaders()["Authorization"]);
            if (token == null)
            {
                Console.WriteLine("Unauthorized: invalid user token.");
                _response = new(401, "Unauthorized.");
                return;
            }

            using NpgsqlConnection connection = _dbConnection.OpenConnection();

            if (_userManagement.CheckIfTokenIsValid(token))
            {
                switch (path)
                {
                    case "/packages":
                        _request = _parser.ParseBody("card");

                        if (_userManagement.CheckIfAdmin(connection, token))
                        {
                            _response = _packageManagement.CreatePackage(_request.GetCards());
                        }
                        else
                        {
                            Console.WriteLine("Unauthorized: user is not admin.");
                            _response = new(401, "Unauthorized.");
                        }
                        break;

                    case "/transactions/packages":
                        _response = _userManagement.AquirePackage(token);
                        break;

                    case "/tradings":
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
            else
            {
                Console.WriteLine("Unauthorized: invalid user token.");
                _response = new(401, "Unauthorized.");
            }
        }

        public void PutRequestHandler(string path)
        {
            Console.WriteLine($"Handling POST Request for {path}...");
            string? token = _parser.ExtractToken(_request.GetHeaders()["Authorization"]);
            if (token == null)
            {
                Console.WriteLine("Unauthorized: invalid user token.");
                _response = new(401, "Unauthorized.");
                return;
            }

            if (_userManagement.CheckIfTokenIsValid(token))
            {
                string username = "";
                if (path.StartsWith("/users/"))
                {
                    username = _parser.ExtractUsername(path);
                }
                switch (path)
                {
                    case "/deck":
                        _request = _parser.ParseBody("list");
                        _response = _userManagement.ConfigureUserDeck(_request.GetCardIds(), token);
                        break;

                    case var userPath when userPath == $"/users/{username}":
                        _request = _parser.ParseBody("dictionary");
                        _response = _userManagement.UpdateUserData(_request.GetBody(), token, username);
                        break;

                    default:
                        Console.WriteLine("Invalid path.");
                        _response = new(404, "Invalid path.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("Unauthorized: invalid user token.");
                _response = new(401, "Unauthorized.");
            }
        }

        public void DeleteRequestHandler(string path)
        {
            Console.WriteLine($"Handling POST Request for {path}...");
            string? token = _parser.ExtractToken(_request.GetHeaders()["Authorization"]);
            if (token == null)
            {
                Console.WriteLine("Unauthorized: invalid user token.");
                _response = new(401, "Unauthorized.");
                return;
            }

            if (_userManagement.CheckIfTokenIsValid(token))
            {
                switch (path)
                {
                    case "/deck":
                        Console.WriteLine("Invalid path.");
                        _response = new(404, "Invalid path.");
                        break;

                    case "/tradings":
                        Console.WriteLine("Invalid path.");
                        _response = new(404, "Invalid path.");
                        break;

                    default:
                        //_parser.ParseBody();
                        Console.WriteLine("Invalid path.");
                        _response = new(404, "Invalid path.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("Unauthorized: invalid user token.");
                _response = new(401, "Unauthorized.");
            }
        }
    }
}
