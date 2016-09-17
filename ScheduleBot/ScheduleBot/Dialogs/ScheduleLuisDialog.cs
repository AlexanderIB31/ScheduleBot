using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;

namespace ScheduleBot.Dialogs
{
    [Serializable]
    [LuisModel("", "")]
    public class ScheduleLuisDialog : LuisDialog<object>
    {
        [LuisIntent("")]
        public async Task ProcessNone(IDialogContext context, LuisResult result)
        {
            var message = "Sorry, I couldn't understand you";

            await context.PostAsync(message, "en-US");

            context.Wait(MessageReceived);
        }

        //    [LuisIntent("GetHelp")]
        //    public async Task ProcessGetHelp(IDialogContext context, LuisResult result)
        //    {
        //        string message = "I'm a simple weather bot. Here are some examples of things you can ask me about: \n\n" +
        //                  "\"What is the weather like in Moscow today?\" \n\n" +
        //                  "\"Any news about temperature today?\" \n\n" +
        //                  "or just tell me \"Hello\" or \"Thank you\"";
        //        await context.PostAsync(message, "en-US");
        //        context.Wait(MessageReceived);
        //    }

        //    [LuisIntent("Hello")]
        //    public async Task ProcessHello(IDialogContext context, LuisResult result)
        //    {
        //        var messages = new string[]
        //        {
        //            "Hello!",
        //            "Nice to meet you!",
        //            "Hi! What can I help you with?",
        //            "I'm here to help you!"
        //        };

        //        var message = messages[(new Random()).Next(messages.Count() - 1)];
        //        await context.PostAsync(message, "en-US");
        //        context.Wait(MessageReceived);
        //    }

        //    [LuisIntent("Thanks")]
        //    public async Task ProcessThanks(IDialogContext context, LuisResult result)
        //    {
        //        var messages = new string[]
        //        {
        //            "Never mind",
        //            "You are welcome!",
        //            "Happy to be useful"
        //        };

        //        var message = messages[(new Random()).Next(messages.Count() - 1)];
        //        await context.PostAsync(message, "en-US");
        //        context.Wait(MessageReceived);
        //    }

        //    [LuisIntent("GetSchedule")]
        //    public async Task GetSchedule(IDialogContext context, LuisResult result)
        //    {
        //        var parameter = "temperature";
        //        var location = "Moscow";
        //        DateTime date = DateTime.Today.Date;

        //        EntityRecommendation entityContainer;
        //        if (result.TryFindEntity("builtin.geography.city", out entityContainer))
        //        {
        //            location = entityContainer.Entity;
        //        }

        //        if (result.TryFindEntity("builtin.datetime.date", out entityContainer))
        //        {
        //            DateTime.TryParse(entityContainer?.Resolution?.SingleOrDefault().Value, out date);
        //        }

        //        if (result.TryFindEntity("parameter", out entityContainer))
        //        {
        //            parameter = entityContainer.Entity;
        //        }

        //        DocumentDbRepository<Item>.Initialize();

        //        string message;
        //        if (forecast != null)
        //        {
        //            if (parameter.Contains("humid")) { message = $"The humidity on {forecast.Date} in {location} is {forecast.Humidity}\r\n"; }
        //            else if (parameter.Contains("pres")) { message = $"The pressure on {forecast.Date} in {location} is {forecast.Pressure}\r\n"; }
        //            else if (parameter.Contains("temp")) { message = $"The temperature on {forecast.Date} in {location} is {forecast.Temp}\r\n"; }
        //            else { message = "Sorry, unknown parameter \"{parameter}\" requested... Try again"; }
        //        }
        //        else { message = "Sorry! I was not able to get the forecast."; }

        //        await context.PostAsync(message, "en-US");
        //        //await context.PostWithTranslationAsync(message, "en-US", Thread.CurrentThread.CurrentCulture.Name);

        //        context.Wait(MessageReceived);
        //    }
        //}
    }
}