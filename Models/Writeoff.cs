using Dapper;
using System;
using System.Collections.Generic;

namespace Devin.Models
{
    public class Writeoff
    {
        public int Id { get; set; }

        public int CostArticle { get; set; }

        public DateTime Date { get; set; }

        public DateTime LastExcelDate { get; set; }

        public string LastExcel { get; set; }

        public string Params { get; set; }

        public string Type { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string DefExcel { get; set; }

        public string Template { get; set; }

        public int FolderId { get; set; }

        public List<Repair> Repairs { get; set; } = new List<Repair>();

        public string[] Parameters => (Params ?? "").Split(new string[] { ";;" }, StringSplitOptions.None);

        public float AllCost()
        {
            float cost = 0;
            foreach (var repair in Repairs)
            {
                cost += repair.Number * repair.Storage.Cost;
            }
            return cost;
        }

        public void Load()
        {
            using (var conn = Database.Connection())
            {
                Repairs = conn.Query<Repair>("SELECT * FROM Repairs WHERE WriteoffId = @Id", new { Id }).AsList();
            }
        }
    }
}