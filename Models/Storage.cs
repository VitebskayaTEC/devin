using System;

namespace Devin.Models
{
    public class Storage
    {
        public int INum { get; set; }

        public string Name { get; set; }

        public string Ncard { get; set; }

        public string Class_Name { get; set; }

        public float Price { get; set; }

        public int Nadd { get; set; }

        public int Nis { get; set; }

        public int Nuse { get; set; }

        public int Nbreak { get; set; }

        public DateTime Date { get; set; }

        public int Delit { get; set; }

        public string Uchet { get; set; }

        public int Id_Cart { get; set; }

        public int G_Id { get; set; }

        public bool IsOff() => (Nadd == Nbreak) && (Nis + Nuse == 0);

        public string Led()
        {
            if (Nadd != (Nis + Nuse + Nbreak)) return "warning";
            else if (Nadd == Nbreak) return "off";
            else if (Nis == 0 || Nuse > 0) return "onwork";
            else return "on";
        }
    }
}