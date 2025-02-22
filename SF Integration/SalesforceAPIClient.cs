using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;

namespace SF_Integration
{
    class SalesforceAPIClient
    {
        private const string LOGIN_ENDPOINT = "https://login.salesforce.com/services/oauth2/token";
        private const string API_ENDPOINT = "/services/data/v51.0/";

        public string Username { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string AuthToken { get; private set; }
        public string InstanceUrl { get; private set; }

        static SalesforceAPIClient()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        public async Task LoginAsync()
        {
            var formData = new Dictionary<string, string>
            {
                { "grant_type", "password" },
                { "client_id", ClientId },
                { "client_secret", ClientSecret },
                { "username", Username },
                { "password", Password + Token }
            };

            using (var httpClient = new HttpClient())
            {
                try
                {
                    var requestContent = new FormUrlEncodedContent(formData);
                    HttpResponseMessage response = await httpClient.PostAsync(LOGIN_ENDPOINT, requestContent).ConfigureAwait(false);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        Console.WriteLine("Authentication Successful!");

                        var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseBody);

                        if (values != null && values.TryGetValue("access_token", out string authToken) && values.TryGetValue("instance_url", out string instanceUrl))
                        {
                            AuthToken = authToken;
                            InstanceUrl = instanceUrl;

                            Console.WriteLine("AuthToken: " + AuthToken);
                            Console.WriteLine("InstanceURL: " + InstanceUrl);
                        }
                        else
                        {
                            Console.WriteLine("Invalid response structure.");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync().ConfigureAwait(false)}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.Message);
                }
            }
        }
        public async Task<string> QueryAsync(string soqlQuery)
        {
            using (var client = new HttpClient())
            {
                string restRequest = $"{InstanceUrl}{API_ENDPOINT}query?q={soqlQuery}";
                var request = new HttpRequestMessage(HttpMethod.Get, restRequest);

                request.Headers.Add("Authorization", "Bearer " + AuthToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Add("X-PrettyPrint", "1");

                HttpResponseMessage response = await client.SendAsync(request);
                return await response.Content.ReadAsStringAsync();
            }
        }
        public async Task<HttpResponseMessage> PatchAsync(string endpoint, string jsonBody)
        {
            using (var client = new HttpClient())
            {
                string restRequest = $"{InstanceUrl}{API_ENDPOINT}{endpoint}";
                var request = new HttpRequestMessage(HttpMethod.Patch, restRequest)
                {
                    Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
                };

                request.Headers.Add("Authorization", "Bearer " + AuthToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Add("X-PrettyPrint", "1");

                var response = await client.SendAsync(request);

                return  response;
            }
        }
    }
}
