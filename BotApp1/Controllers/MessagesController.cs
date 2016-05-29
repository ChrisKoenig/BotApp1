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
                string result = ProcessRequest(filter);
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


        private string ProcessRequest(string keywords)
        {
            var records = GetRecords(keywords);
            return records;
        }

        private string GetRecords(string keywords)
        {
            string urlRequestString = $"https://chkoenigdemo01.api.crm.dynamics.com/api/data/v8.0/knowledgearticles?$filter=contains(description,%27{keywords}%27)";

            var request = HttpWebRequest.Create(urlRequestString);
            AddAuthorizationHeader(request);

            string result = "";

            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());
                result = reader.ReadToEnd();
            }
            return result;

        }

        private async void AddAuthorizationHeader(WebRequest client)
        {
            client.PreAuthenticate = true;

            string resourceId = "https://chkoenigdemo01.crm.dynamics.com";
            string authorityUrl = "https://login.windows.net/common";
            string clientId = "fa8c25d3-37fb-44d2-a941-506edd3b49e1";
            string clientSecret = "0OUGEY1M49aeJhAjAP0txOg15xubh6aCp3fjQk7w+Is=";

            AuthenticationContext authContext = new AuthenticationContext(authorityUrl, false);
            ClientCredential clientCredential = new ClientCredential(clientId, clientSecret);
            AuthenticationResult authResult = await authContext.AcquireTokenAsync(resourceId, clientCredential);

            client.Headers.Add("Authorization", new AuthenticationHeaderValue("Bearer", authResult.AccessToken).ToString());

        }
        
    }

}