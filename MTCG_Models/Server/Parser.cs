using MonsterTradingCardsGame.MTCG_Models.Server;
using Npgsql.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.MTCG_Models.Server
{
    public class Parser
    {
        public static Dictionary<string, string> ParseRequest(StreamWriter writer, string request)
        {
            //  Split the request string by each line
            string[] lines = request.Split("\r\n");
            Dictionary<string, string> body = new();
            Dictionary<string, string> requestData = new();

            if (lines.Length == 0)
            {
                Console.WriteLine("Malformed Request Syntax.");
                HTTPResponse.Response(writer, 400);  
                //HTTPResponse.Response(writer, 400, "Malformed Request Syntax.");
                return requestData;
            }

            //  First line of the HTTP request contains the method, path and HTTP version
            string[] firstLine = lines[0].Split(" ");
            string method = firstLine[0].Trim();
            string path = firstLine[1].Trim();
            string ver = firstLine[2].Trim();

            requestData["Method"] = method;
            requestData["Path"] = path;
            requestData["HttpVer"] = ver;

            //  Split the header into pairs and add them to the dictionary
            int i = 1;
            while (i < lines.Length && lines[i] != "")
            {
                string[] headerParts = lines[i].Split(':', 2);

                if (headerParts.Length == 2)
                {
                    requestData[headerParts[0].Trim()] = headerParts[1].Trim();
                }
                i++;
            }
            i++;

            string bodyStr = string.Join("\r\n", lines.Skip(i).ToArray());
            if (CheckIfValidString(bodyStr))
            {
                //HTTPResponse.Response(writer, 400, "Request Body is empty");
                HTTPResponse.Response(writer, 400);
                return requestData;
            }

            body = ParseBody(writer, bodyStr);

            foreach (var data in body)
            {
                requestData[data.Key] = data.Value;
            }

            return requestData;
        }

        public static Dictionary<string, string> ParseBody(StreamWriter writer, string bodyStr)
        {
            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, string>>(bodyStr);
            }
            catch (JsonException e)
            {
                Console.WriteLine("An error occured during Deserialization: " + e.Message);
                HTTPResponse.Response(writer, 400);
                //HTTPResponse.Response(writer, 400, "An error occured during Deserialization");
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine("Parsing request body failed: " + e.Message);
                HTTPResponse.Response(writer, 400);
                //HTTPResponse.Response(writer, 400, "Parsing request body failed");
                throw;
            }
        }

        //  Helper method to check for an empty string
        public static bool CheckIfValidString(string str)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str) || str.Length == 0)
            {
                return false;
            }
            return true;
        }
    }
}
