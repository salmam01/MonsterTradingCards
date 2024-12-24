using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.MTCG_Models.Server
{
    //  Class to store requests in
    public class Request
    {
        private readonly string _method;
        public readonly string _path;
        public readonly string _ver;
        public Dictionary<string, string> _headers;
        public Dictionary<string, string> _body;

        public Request(string method, string path, string ver)
        {
            _method = method;
            _path = path;
            _ver = ver;
        }

        public void SetHeaders(Dictionary<string, string> headers)
        {
            _headers = headers;
        }

        public void SetBody(Dictionary<string, string> body)
        {
            _body = body;
        }

        public string GetMethod()
        {
            return _method;
        }

        public string GetPath()
        {
            return _path;
        }

        public Dictionary<string, string> GetHeaders()
        {
            return _headers;
        }

        public Dictionary<string, string> GetBody()
        {
            return _body;
        }
    }
}
