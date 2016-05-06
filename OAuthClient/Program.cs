using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OAuthClient
{
    class Program
    {
        static void Main(string[] args)
        {
            ClientAPI api = new ClientAPI();
            api.Consume();
        }
    }
}
