using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace ScheduleBot.Dialogs
{
    [Serializable]
    [LuisModel("35463052-d3b7-43d1-9464-04b4a170ced1", "571c4e87b13c474c846843cbae1a83df")]
    public class ScheduleLuisDialog : LuisDialog<object>
    {
        private readonly string _group;
        private readonly string _name;

        private string CloselyLesson(IList<DayOfWeek> item, DateTime date, bool isTopWeek)
        {
            foreach (var cur in item)
            {
                if (isTopWeek)
                {
                    if (cur.TopWeek.TimeEnd.Minute >= date.Minute && (cur.TopWeek.TimeStart.Minute - 10) <= date.Minute)
                    {
                        return cur.TopWeek.TeacherData;
                    }
                }
                else
                {
                    if (cur.BelowWeek.TimeEnd.Minute >= date.Minute && (cur.BelowWeek.TimeStart.Minute - 10) <= date.Minute)
                    {
                        return cur.BelowWeek.TeacherData;
                    }
                }
            }
            return "";
        }

        private string FindLesson(IList<DayOfWeek> item, DateTime date, bool isTopWeek)
        {
            foreach (var cur in item)
            {
                if (isTopWeek)
                {
                    if (cur.TopWeek.TimeEnd.Minute >= date.Minute && (cur.TopWeek.TimeStart.Minute - 10) <= date.Minute)
                    {
                        return cur.TopWeek.Subject;
                    }
                }
                else
                {
                    if (cur.BelowWeek.TimeEnd.Minute >= date.Minute && (cur.BelowWeek.TimeStart.Minute - 10) <= date.Minute)
                    {
                        return cur.BelowWeek.Subject;
                    }
                }
            }
            return "";
        }

        public ScheduleLuisDialog(Tuple<string, string> userData)
        {
            _name = userData.Item1;
            _group = userData.Item2;
        }

        [LuisIntent("")]
        public async Task ProcessNone(IDialogContext context, LuisResult result)
        {
            var message = "Sorry, I couldn't understand you";

            await context.PostAsync(message, "en-US");

            context.Wait(MessageReceived);
        }

        [LuisIntent("Help")]
        public async Task ProcessGetHelp(IDialogContext context, LuisResult result)
        {
            string message = "I'm a simple schedule bot. You can ask me about: \n\n" +
                      "\"What I have lessons tomorrow?\" \n\n" +
                      "\"What is the name of the teacher?\" \n\n" +
                      "\"How many lessons I have today?\"" +
                      "or just tell me \"Hello\" or \"Thank you\"";
            await context.PostAsync(message, "en-US");
            context.Wait(MessageReceived);
        }

        [LuisIntent("Greeting")]
        public async Task ProcessGreeting(IDialogContext context, LuisResult result)
        {
            var messages = new string[]
            {
                    "Hello!",
                    "Nice to meet you!",
                    "Hi! What can I help you with?",
                    "I'm here to help you!"
            };

            var message = messages[(new Random()).Next(messages.Count() - 1)];
            await context.PostAsync(message, "en-US");
            context.Wait(MessageReceived);
        }

        [LuisIntent("Thanks")]
        public async Task ProcessThanks(IDialogContext context, LuisResult result)
        {
            var messages = new string[]
            {
                    "Never mind",
                    "You are welcome!",
                    "Happy to be useful"
            };

            var message = messages[(new Random()).Next(messages.Count() - 1)];
            await context.PostAsync(message, "en-US");
            context.Wait(MessageReceived);
        }

        [LuisIntent("ListLessons")]
        public async Task GetListLessons(IDialogContext context, LuisResult result)
        {
            EntityRecommendation entityContainer;
            string when = "", resMsg = "";
            if (result.TryFindEntity("TimeType", out entityContainer))
            {
                when = entityContainer.Entity;
            }
            if (result.TryFindEntity("SubjectType", out entityContainer))
            {
                if (entityContainer.Entity.ToLower().Contains("lesson"))
                {
                    var items = await DocumentDbRepository<Item>.GetItemsAsync(x => x.Id.Contains(_group) || x.Id == _group);
                    var day = DateTime.Today;
                    var startLearning = new DateTime(day.Year, 9, 1);
                    for (int i = 0; i < 7; ++i)
                    {
                        if (startLearning.AddDays(i).DayOfWeek == System.DayOfWeek.Saturday)
                        {
                            startLearning = startLearning.AddDays(i);
                            break;
                        }
                    }
                    var isTopWeek = (((day - startLearning).Days / 7) & 1) == 1;
                    if (when.ToLower() == "tomorrow" || when.ToLower() == "today")
                    {
                        day = when.ToLower() == "tomorrow" ? DateTime.Today.AddDays(1) : DateTime.Today;
                        var t = items
                            .Where(x => x.Schedule.Keys.Contains(day.DayOfWeek.ToString()))
                            .Select(x => x.Schedule[day.DayOfWeek.ToString()]);
                        if (t.FirstOrDefault() != null)
                        {
                            resMsg = $"Lessons on {day}: ";
                            foreach (var cur in t.FirstOrDefault())
                            {
                                if (isTopWeek)
                                {
                                    resMsg +=
                                        $"\n{cur.TopWeek.TimeStart.Hour} - {cur.TopWeek.TimeEnd.Hour} |" +
                                        $"{cur.TopWeek.Subject}({cur.TopWeek.Cabinet}; {cur.TopWeek.TypeLesson})";
                                }
                                else
                                {
                                    resMsg +=
                                        $"\n{cur.BelowWeek.TimeStart.Hour} - {cur.BelowWeek.TimeEnd.Hour} |" +
                                        $"{cur.BelowWeek.Subject}({cur.BelowWeek.Cabinet}; {cur.BelowWeek.TypeLesson})";
                                }
                            }
                        }
                    }
                    else if (when.ToLower() == "next" || when.ToLower() == "current" || when.ToLower() == "now")
                    {                       
                        var t = items
                            .Select(x => FindLesson(x.Schedule[DateTime.Today.DayOfWeek.ToString()], day, isTopWeek));
                        if (t.FirstOrDefault() != null)
                        {
                            resMsg = $"Lesson is {t.FirstOrDefault()}: ";
                        }
                    }

                }
            }
            await context.PostAsync(resMsg, "en-US");
            context.Wait(MessageReceived);
        }

        [LuisIntent("TeacherInfo")]
        public async Task GetTeacherInfo(IDialogContext context, LuisResult result)
        {
            EntityRecommendation entityContainer;
            string teacher, resMsg = "";
            if (result.TryFindEntity("NameType", out entityContainer))
            {
                teacher = entityContainer.Entity;
                if (teacher.ToLower().Contains("teacher"))
                {
                    if (result.TryFindEntity("SubjectType", out entityContainer))
                    {
                        var name = entityContainer.Entity;
                        var day = DateTime.Today;
                        if (name.ToLower().Contains("name") && day.DayOfWeek != System.DayOfWeek.Saturday)
                        {
                            var startLearning = new DateTime(day.Year, 9, 1);
                            for (int i = 0; i < 7; ++i)
                            {
                                if (startLearning.AddDays(i).DayOfWeek == System.DayOfWeek.Saturday)
                                {
                                    startLearning = startLearning.AddDays(i);
                                    break;
                                }
                            }
                            var isTopWeek = (((day - startLearning).Days / 7) & 1) == 1;

                            var items = await DocumentDbRepository<Item>.GetItemsAsync(x => x.Id.Contains(_group) || x.Id == _group);
                            var res =
                                items
                                .Select(x => CloselyLesson(x.Schedule[day.DayOfWeek.ToString()], day, isTopWeek));
                            if (string.IsNullOrEmpty(res.FirstOrDefault()))
                            {
                                resMsg += $"Teacher`s name is {res.FirstOrDefault()}";
                            }
                        }
                    }
                }
            }
            await context.PostAsync(resMsg, "en-US");
            context.Wait(MessageReceived);
        }

        [LuisIntent("Quantity")]
        public async Task GetQuantity(IDialogContext context, LuisResult result)
        {
            EntityRecommendation entityContainer;
            string resMsg = "I can`t to count of subjects", when;
            if (result.TryFindEntity("CountType", out entityContainer))
            {
                var hm = entityContainer.Entity.ToLower().Contains("how much");
                if (hm && result.TryFindEntity("TimeType", out entityContainer))
                {
                    when = entityContainer.Entity.ToLower();
                    var day = when == "tomorrow" ? DateTime.Today.AddDays(1) : DateTime.Today;
                    var items = await DocumentDbRepository<Item>.GetItemsAsync(x => x.Id.Contains(_group) || x.Id == _group);
                    var res = items.Select(x => x.Schedule[day.DayOfWeek.ToString()].Count);
                    resMsg = $"{when} " + (when == "tomorrow" ? "will be " : "are ") + $"{res}";
                }
            }
            await context.PostAsync(resMsg, "en-US");
            context.Wait(MessageReceived);
        }
    }
}