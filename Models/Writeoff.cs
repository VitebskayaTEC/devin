using System;

namespace Devin.Models
{
    public class Writeoff
    {
        public int Id { get; set; }

        public int Cost_Article { get; set; }

        public DateTime Date { get; set; }

        public string Params { get; set; }

        public string Type { get; set; }

        public string Name { get; set; }

        public string DefExcel { get; set; }

        public string Template { get; set; }
    }
}