using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System.Configuration;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using System.Web.UI;
using Microsoft.Bot.Builder.Dialogs;
using ScheduleBot.Dialogs;

namespace ScheduleBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            try
            {
                if (activity == null || activity.GetActivityType() != ActivityTypes.Message)
                {
                    HandleSystemMessage(activity);
                }
                var client = activity.GetStateClient();
                var userData = await client.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);
                #region clear userdata
                //userData.SetProperty<string>("Name", "");
                //userData.SetProperty<string>("Group", "");
                //userData.SetProperty<bool?>("isMeet", false);
                //await client.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                #endregion
                var name = userData.GetProperty<string>("Name");
                if (string.IsNullOrEmpty(name))
                {
                    ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                    var isMeet = userData.GetProperty<bool?>("isMeet");
                    if (isMeet == null || isMeet == false)
                    {
                        string meetMsg = @"Hi, my name is Rik, 
I am here to help you remind schedule of lessons at the university. 
Please tell me your name, and number of you group(fully).
Example: Sergey, 308";
                        Activity reply = activity.CreateReply(meetMsg);
                        userData.SetProperty<bool?>("isMeet", true);
                        await client.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                        await connector.Conversations.ReplyToActivityAsync(reply);
                    }
                    else
                    {
                        var rep = CheckPerson(activity.Text);
                        if (rep.Item2 != null)
                        {
                            userData.SetProperty<string>("Name", rep.Item2);
                            userData.SetProperty<string>("Group", rep.Item3);
                            await client.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                        }
                        Activity reply = activity.CreateReply(rep.Item1);
                        await connector.Conversations.ReplyToActivityAsync(reply);
                    }
                }
                else
                {
                    await Conversation.SendAsync(activity, () => new ScheduleLuisDialog(
                        new Tuple<string,string>(userData.GetProperty<string>("Name"), userData.GetProperty<string>("Group"))));
                }
                var response = Request.CreateResponse(HttpStatusCode.OK);
                return response;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }

        // <status, name, group>
        Tuple<string, string, string> CheckPerson(string msg)
        {
            var words = msg.Split(',');
            if (words.Length != 2)
            {
                return new Tuple<string, string, string>(@"Input data is incorrect, follow the example.", null, null);
            }
            else
            {
                var test = words[0].Contains("8О") || words[0].Contains('-')
                    ? new Tuple<string, string>(words[1], words[0])
                    : new Tuple<string, string>(words[0], words[1]);
                return new Tuple<string, string, string>($"All right! {test.Item1}, let`s look what can I do. Please, send me next word: 'help', if you want to know about my abilities.", test.Item1, test.Item2);
            }
        }
    }
}