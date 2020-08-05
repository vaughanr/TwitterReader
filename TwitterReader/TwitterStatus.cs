using System;
using System.Text.Json.Serialization;

namespace TwitterReader
{
    public class TwitterStatus
    {
        public string Text { get; set; }

        public DateTime Created_At { get; set; }
        public long Id { get; set; }
    }
}
