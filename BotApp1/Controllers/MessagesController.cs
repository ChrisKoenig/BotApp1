using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Utilities;
using Newtonsoft.Json;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Xrm.Sdk.Client;
using System.ServiceModel;
using System.IO;
using System.Net.Http.Headers;

namespace BotApp1
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<Message> Post([FromBody]Message message)
        {
            if (message.Type == "Message")
            {
                // return our reply to the user
                // return message.CreateReplyMessage($"You said: {message.Text}");
                string filter = message.Text;
                string result = await ProcessRequest(filter);
                return message.CreateReplyMessage(result);
            }
            else
            {
                return HandleSystemMessage(message);
            }
        }

        private Message HandleSystemMessage(Message message)
        {
            if (message.Type == "Ping")
            {
                Message reply = message.CreateReplyMessage();
                reply.Type = "Ping";
                return reply;
            }
            else if (message.Type == "DeleteUserData")
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == "BotAddedToConversation")
            {
            }
            else if (message.Type == "BotRemovedFromConversation")
            {
            }
            else if (message.Type == "UserAddedToConversation")
            {
            }
            else if (message.Type == "UserRemovedFromConversation")
            {
            }
            else if (message.Type == "EndOfConversation")
            {
            }

            return null;
        }


        private async Task<string> ProcessRequest(string keywords)
        {
            var records = await GetRecords(keywords);
            return records;
        }

        private async Task<string> GetRecords(string keywords)
        {
            try
            {
                string serviceUrl = $"https://chkoenigdemo01.api.crm.dynamics.com/api/data/v8.1/knowledgearticles?$filter=contains(content,%27{keywords}%27)";

                // build the client
                HttpClient client = new HttpClient();
                client.Timeout = new TimeSpan(0, 2, 0);

                HttpRequestMessage request1 = new HttpRequestMessage(HttpMethod.Get, serviceUrl);
                request1.Method = HttpMethod.Get;

                // wait for the response
                HttpResponseMessage response1 = await client.SendAsync(request1);

                // we're expecting a 401 status code
                if (response1.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var header= response1.Headers.WwwAuthenticate.ToString();
                    var authUrl = "";
                    var resourceUrl = "";
                    var authHeader = "";
                    ParseAuthResponse(header, out authHeader, out authUrl, out resourceUrl);

                    var token = await GetAuthToken(authUrl, resourceUrl);

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authHeader, token);
                    HttpRequestMessage request2 = new HttpRequestMessage(HttpMethod.Get, serviceUrl);
                    request2.Method = HttpMethod.Get;

                    // wait for the response
                    HttpResponseMessage response2 = await client.SendAsync(request2);
                    if (response2.IsSuccessStatusCode)
                    {
                        return "FINISH: RESPONSE2";
                    }
                    else
                    {
                        return "ERROR: RESPONSE2";
                    }
                }
                else
                {
                    return "FINISH: RESPONSE1";
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "ERROR: " + ex.Message;
            }
        }

        private void ParseAuthResponse(string header, out string authHeader, out string authUrl, out string resourceUrl)
        {
            var items = header.Split(' ');
            authHeader = items[0].Trim();
            authUrl = items[1].Substring(18, items[1].Length - 19);
            resourceUrl = items[2].Substring(12);
        }

        private async Task<string> GetAuthToken(string authUrl, string resourceUrl)
        {
            //string resourceId = "https://chkoenigdemo01.crm.dynamics.com";
            //string authorityUrl = "https://login.microsoftonline.com/d1a9ab67-6f36-4c34-84ad-ad634026ed95/oauth2/authorize";
            string clientId = "fa8c25d3-37fb-44d2-a941-506edd3b49e1";
            string clientSecret = "0OUGEY1M49aeJhAjAP0txOg15xubh6aCp3fjQk7w+Is=";

            AuthenticationContext authContext = new AuthenticationContext(authUrl, false);
            ClientCredential clientCredential = new ClientCredential(clientId, clientSecret);
            AuthenticationResult authResult = await authContext.AcquireTokenAsync(resourceUrl, clientCredential);
            string authHeader = authResult.CreateAuthorizationHeader();

            return authResult.AccessToken;

        }

        //string authorityUrl = "https://login.windows.net/common";
        //string authorityUrl = "https://login.microsoftonline.com/d1a9ab67-6f36-4c34-84ad-ad634026ed95/oauth2/token";

    }

}