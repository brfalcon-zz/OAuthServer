using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace OAuthClient
{
    public class ClientAPI
    {
        private static string _hostUri = "http://localhost:2093/";

        public void Consume()
        {
            string username = "teste7";
            string password = "123456";

            Console.WriteLine("Registrando usuario '{0}'...", username);
            Console.WriteLine();

            var success = RegisterUser(username, password).Result;

            if (success)
            {
                try
                {
                    Console.WriteLine("Acessando recurso restrito nao usando o token...");
                    var orderList = GetOrders("").Result;
                    Console.WriteLine("Resultado:\n{0}", orderList);
                    Console.WriteLine();

                    Console.WriteLine("Requisitando token para o usuario '{0}'...", username);
                    var token = RequestToken(username, password).Result;
                    Console.WriteLine("Resultado:\n{0}", token["access_token"]);
                    Console.WriteLine();

                    Console.WriteLine("Acessando recurso restrito usando o token...");
                    orderList = GetOrders(token["access_token"]).Result;
                    Console.WriteLine("Resultado:\n{0}", orderList);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERRO -> A Requisição de token falhou! Motivo: {0}", ex.Message);
                }
            }
            else
            {
                Console.WriteLine("ERRO -> Nao foi possível registrar o usuário");
            }

            Console.Read();
        }

        public async Task<bool> RegisterUser(string username, string password)
        {
            HttpResponseMessage response;
            var pairs = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>( "username", username ),
                    new KeyValuePair<string, string> ( "password", password ),
                    new KeyValuePair<string, string>( "confirmPassword", password )
                };
            var content = new FormUrlEncodedContent(pairs);

            using (var client = new HttpClient())
            {
                var registrationEndpoint = new Uri(new Uri(_hostUri), "api/account/register");
                response = await client.PostAsync(registrationEndpoint, content);
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode;
        }

        public async Task<Dictionary<string, string>> RequestToken(string userName, string password)
        {
            HttpResponseMessage response;
            var pairs = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>( "grant_type", "password" ),
                    new KeyValuePair<string, string>( "username", userName ),
                    new KeyValuePair<string, string> ( "password", password )
                };
            var content = new FormUrlEncodedContent(pairs);

            using (var client = new HttpClient())
            {
                var tokenEndpoint = new Uri(new Uri(_hostUri), "Token");
                response = await client.PostAsync(tokenEndpoint, content);
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(string.Format("Error: {0}", responseContent));
            }

            return JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);
        }

        public async Task<string> GetOrders(string accessToken)
        {
            string orderList = null;

            using(var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var apiReturn = await client.GetAsync(new Uri(new Uri(_hostUri), "/api/orders"));
                var apiReturnContent = await apiReturn.Content.ReadAsStringAsync();
                
                orderList = apiReturnContent;
            }

            return orderList;
        }
    }
}
