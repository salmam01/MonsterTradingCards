using Npgsql.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.MTCG_Models
{
    internal class Parser
    {
        public Parser() { }

        public static List<string> LoginParser()
        {

            return null;
        }

        public static Dictionary<string, string> RegisterParser(StreamWriter writer, string body, string method)
        {
            //  If body contains nothing in it, output an error code
            if (string.IsNullOrWhiteSpace(body))
            {
                //HTTPResponse(writer, 400, "Request Body is empty.");
                return null;
            }

            try
            {
                var data = new Dictionary<string, string>();
                data = JsonSerializer.Deserialize<Dictionary<string, string>>(body);

                return data;
            }
            catch (Exception e)
            {
                Console.WriteLine("Parsing body failed." + e.Message);
                //HTTPResponse(writer, 400, "Parsing body failed.");
                throw;
            }
        }
    }
}
