using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;

namespace ScheduleBot
{
    public class Item
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "Steward")]
        public string Steward { get; set; }

        [JsonProperty(PropertyName = "Schedule")]
        public IDictionary<string, IList<DayOfWeek>> Schedule { get; set; }
    }

    public class DayOfWeek
    {
        [JsonProperty(PropertyName = "TopWeek")]
        public Lesson TopWeek;
        [JsonProperty(PropertyName = "BelowWeek")]
        public Lesson BelowWeek;
    }

    public class Lesson
    {
        [JsonProperty(PropertyName = "TimeStart")]
        [JsonConverter(typeof(CustomDateFormat), "HH:mm")]
        public DateTime TimeStart { get; set; }
        [JsonProperty(PropertyName = "TimeEnd")]
        [JsonConverter(typeof(CustomDateFormat), "HH:mm")]
        public DateTime TimeEnd { get; set; }
        [JsonProperty(PropertyName = "Subject")]
        public string Subject { get; set; }
        [JsonProperty(PropertyName = "TeacherData")]
        public string TeacherData { get; set; }
        [JsonProperty(PropertyName = "TypeLesson")]
        public TypeLesson TypeLesson { get; set; }
        [JsonProperty(PropertyName = "Cabinet")]
        public string Cabinet { get; set; }

    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum TypeLesson
    {
        [EnumMember(Value = "lab")]
        Lab,
        [EnumMember(Value = "lecture")]
        Lecture,
        [EnumMember(Value = "seminar")]
        Seminar
    }

    public class CustomDateFormat : IsoDateTimeConverter
    {
        public CustomDateFormat(string format)
        {
            DateTimeFormat = format;
        }
    }
}