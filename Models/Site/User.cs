using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Devin.Models.Site
{
    [Table(Name = "Users")]
    public class User
    {
        [Column]
        public string UName { get; set; }

        [Column]
        public string DisplayName { get; set; }
    }
}