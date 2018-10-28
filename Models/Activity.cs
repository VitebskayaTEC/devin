using System;

namespace Devin.Models
{
    public class Activity
    {
        [Slapper.AutoMapper.Id]
        public DateTime Date { get; set; }

        public string Source { get; set; }

        public string Id { get; set; }

        public string Text { get; set; }

        public string Username { get; set; }
    }
}