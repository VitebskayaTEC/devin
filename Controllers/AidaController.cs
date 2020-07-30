using Devin.Models;
using Devin.Models.Views;
using LinqToDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Devin.Controllers
{
    public class AidaController : Controller
    {
        // views

        public ActionResult Index() => View();


        // view partials

        public ActionResult Load(string query, string type, string sort, string direction)
        {
            if (string.IsNullOrEmpty(query))
            {
                return List(sort, direction);
            }
            else
            {
                return Search(query, type);
            }
        }

        public ActionResult List(string sort, string direction)
        {
            var personal = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(sort)) sort = "default";
            if (string.IsNullOrEmpty(direction)) direction = "up";

            using (var db = new SiteContext())
            {
                personal = db.Users
                    .ToDictionary(x => x.UName.ToLower(), x => x.DisplayName);
            }

            var model = new List<Aida>();

            using (var db = new DevinContext())
            {
                var ids = db.Report
                   .GroupBy(x => x.RHost)
                   .Select(g => g.Select(x => x.ID).Max())
                   .ToList();

                var reports = db.Report
                    .Where(x => ids.Contains(x.ID))
                    .OrderBy(x => x.ID)
                    .ToList();

                var items = db.Item
                    .Where(x => ids.Contains(x.ReportID) && x.IPage == "Суммарная информация")
                    .Select(x => new { x.ReportID, x.IField, x.IValue })
                    .ToList();

                var systems = db.Item
                    .Where(x => x.IField == "Операционная система" && x.IPage == "Отчёт")
                    .Select(x => new { x.ReportID, x.IValue })
                    .ToList();

                var processors = items
                    .Where(x => x.IField == "Тип ЦП")
                    .Select(x => new { x.ReportID, x.IValue })
                    .ToList();

                var motherboards = items
                    .Where(x => x.IField == "Системная плата")
                    .Select(x => new { x.ReportID, x.IValue })
                    .ToList();

                var rams = items
                    .Where(x => x.IField == "Системная память")
                    .Select(x => new { x.ReportID, x.IValue })
                    .ToList();

                var disks = items
                    .Where(x => x.IField == "Дисковый накопитель")
                    .GroupBy(x => x.ReportID)
                    .Select(g => new
                    { 
                        ReportID = g.Key, 
                        IValue = g
                            .Select(x => x.IValue)
                            .ToList()
                    })
                    .Select(x => new
                    {
                        x.ReportID,
                        IValue = 
                            x.IValue.FirstOrDefault(y => y.Contains("SSD"))
                            ?? x.IValue.FirstOrDefault(y => !y.Contains("USB"))
                            ?? x.IValue.FirstOrDefault()
                    })
                    .ToList();

                var monitors = items
                    .Where(x => x.IField == "Монитор")
                    .GroupBy(x => x.ReportID)
                    .Select(g => new
                    {
                        ReportID = g.Key,
                        IValue = g
                            .Select(x => x.IValue)
                            .Distinct()
                            .ToArray()
                    })
                    .ToList();

                foreach (var id in ids)
                {
                    var report = reports.FirstOrDefault(x => x.ID == id);

                    var aida = new Aida
                    {
                        Id = report.ID,
                        Name = report.RHost,
                        User = report.RUser.ToLower(),
                        UserName = personal.TryGetValue(report.RUser.ToLower(), out string v) ? v : "",
                        LastReport = report.RDateTime,
                    };

                    // операционная система

                    string os = systems.FirstOrDefault(x => x.ReportID == report.ID)?.IValue ?? "";
                    if (os.Contains("WinXP")) { aida.OS = "Windows XP"; aida.OSScore = 2; }
                    else if (os.Contains("6.1.7600")) { aida.OS = "Windows 7"; aida.OSScore = 7; }
                    else if (os.Contains("Win7")) { aida.OS = "Windows 7"; aida.OSScore = 7; }
                    else if (os.Contains("14393")) { aida.OS = "Windows 10 1607"; aida.OSScore = 1607; }
                    else if (os.Contains("16299")) { aida.OS = "Windows 10 1709"; aida.OSScore = 1709; }
                    else if (os.Contains("17134")) { aida.OS = "Windows 10 1803"; aida.OSScore = 1803; }
                    else if (os.Contains("17763")) { aida.OS = "Windows 10 1809"; aida.OSScore = 1809; }
                    else if (os.Contains("18362")) { aida.OS = "Windows 10 1903"; aida.OSScore = 1903; }
                    else if (os.Contains("18363")) { aida.OS = "Windows 10 1909"; aida.OSScore = 1909; }
                    else if (os.Contains("Win2003")) { aida.OS = "Windows Server 2003"; aida.OSScore = 2003; }
                    else if (os.Contains("Win2012")) { aida.OS = "Windows Server 2012"; aida.OSScore = 2012; }
                    else { aida.OS = os; aida.OSScore = 1; }

                    // процессор

                    string processor = processors.FirstOrDefault(x => x.ReportID == report.ID)?.IValue ?? "";
                    aida.Cpu = processor;
                    aida.CpuCore = processor.Contains("Quad") || processor.Contains("Hexa") ? 4
                        : processor.Contains("Dual") ? 2 : 1;
                    processor = processor.Substring(processor.IndexOf(',') + 1).Trim();
                    if (processor.Contains(' ')) { processor = processor.Substring(0, processor.IndexOf(' ')); }
                    aida.CpuFrequency = int.TryParse(processor, out int i) ? i : 0;

                    // память

                    var motherboard = motherboards.FirstOrDefault(x => x.ReportID == report.ID)?.IValue ?? "";
                    var ram = rams.FirstOrDefault(x => x.ReportID == report.ID)?.IValue ?? "";

                    if (motherboard.Contains("DDR5") || ram.Contains("DDR5")) { aida.RamType = 5; }
                    else if (motherboard.Contains("DDR4") || ram.Contains("DDR4")) { aida.RamType = 4; }
                    else if (motherboard.Contains("DDR3") || ram.Contains("DDR3")) { aida.RamType = 3; }
                    else if (motherboard.Contains("DDR2") || ram.Contains("DDR2")) { aida.RamType = 2; }
                    else if (motherboard.Contains("DDR") || ram.Contains("DDR")) { aida.RamType = 1; }
                    else { aida.RamType = 4; }

                    if (ram.Contains(' ')) { ram = ram.Substring(0, ram.IndexOf(' ')); }
                    aida.RamValue = decimal.TryParse(ram, out decimal d) ? d : 0;

                    // диск

                    string hdd = disks.FirstOrDefault(x => x.ReportID == report.ID)?.IValue ?? "";

                    aida.Disk = hdd;
                    aida.DiskType = hdd.Contains("SSD") ? "SSD" : "HDD";
                    
                    if (hdd.Contains('(')) { hdd = hdd.Substring(hdd.IndexOf('(') + 1).Replace(")", ""); }
                    if (hdd.Contains(',')) { hdd = hdd.Substring(0, hdd.IndexOf(',')); }

                    aida.DiskValue = hdd;
                    if (hdd.ToLower().Contains("гб")) { aida.DiskCapacity  = 1024; }
                    else if (hdd.ToLower().Contains("тб")) { aida.DiskCapacity = 1024 * 1014; }
                    else { aida.DiskCapacity = 1; }

                    aida.DiskCapacity *= (int.TryParse(hdd.Contains(' ') ? hdd.Substring(0, hdd.IndexOf(' ')) : hdd, out i) ? i : 0);

                    // мониторы

                    aida.DisplayScore = 0;
                    foreach (var monitor in monitors.FirstOrDefault(x => x.ReportID == report.ID)?.IValue ?? new string[0])
                    {
                        string monitor_type = "";
                        i = monitor.IndexOf('[');
                        d = 0;

                        if (i > -1)
                        {
                            monitor_type = monitor.Substring(i + 1);

                            i = monitor_type.IndexOf(']');
                            if (i > -1)
                            {
                                monitor_type = monitor_type.Substring(0, i);
                            }

                            i = monitor_type.IndexOf(' ');
                            if (i > -1)
                            {
                                if (decimal.TryParse(monitor_type.Substring(0, monitor_type.IndexOf('"')).Replace('.', ','), out d))
                                {
                                    if (d < 35)
                                    {
                                        if (d > aida.DisplayScore)
                                        {
                                            aida.DisplayScore = d;
                                        }
                                    }
                                    else
                                    {
                                        d = 0;
                                    }
                                }
                            }
                        }

                        aida.DisplaysSize.Add(d);
                        aida.DisplaysCaptions.Add(monitor);
                    }

                    // счёт

                    aida.CpuScore = aida.CpuCore * aida.CpuFrequency;
                    aida.RamScore = aida.RamType * aida.RamValue;
                    aida.DiskScore = (aida.DiskType == "SSD" ? 5 : 1) * aida.DiskCapacity;

                    // добавление в список

                    if (!report.RHost.Contains("BOOK") && !hdd.Contains("VMware") && !os.Contains("Server"))
                    {
                        model.Add(aida);
                    }
                }

                // нормализация счёта (подгоняем под 255)

                decimal cpuMin = decimal.MaxValue, cpuMax = decimal.MinValue;
                decimal ramMin = decimal.MaxValue, ramMax = decimal.MinValue;
                decimal diskMin = decimal.MaxValue, diskMax = decimal.MinValue;
                decimal osMin = decimal.MaxValue, osMax = decimal.MinValue;
                decimal displayMin = decimal.MaxValue, displayMax = decimal.MinValue;

                foreach (var aida in model)
                {
                    if (aida.CpuScore > cpuMax) cpuMax = aida.CpuScore;
                    if (aida.CpuScore < cpuMin) cpuMin = aida.CpuScore;

                    if (aida.RamScore > ramMax) ramMax = aida.RamScore;
                    if (aida.RamScore < ramMin) ramMin = aida.RamScore;

                    if (aida.DiskScore > diskMax) diskMax = aida.DiskScore;
                    if (aida.DiskScore < diskMin) diskMin = aida.DiskScore;

                    if (aida.OSScore > osMax) osMax = aida.OSScore;
                    if (aida.OSScore < osMin) osMin = aida.OSScore;

                    if (aida.DisplayScore > displayMax) displayMax = aida.DisplayScore;
                    if (aida.DisplayScore < displayMin) displayMin = aida.DisplayScore;
                }

                foreach (var aida in model)
                {
                    decimal score = (aida.CpuScore - cpuMin) / (cpuMax - cpuMin);
                    aida.CpuScore = Math.Round(score * 400);

                    score = (aida.RamScore - cpuMin) / (ramMax - cpuMin);
                    aida.RamScore = Math.Round(score * 400);

                    score = (aida.DiskScore - diskMin) / (diskMax - diskMin);
                    aida.DiskScore = Math.Round(score * 400);

                    // общий счёт

                    aida.TotalScore = aida.CpuScore + aida.RamScore + aida.DiskScore;
                }

                // сортировка

                if (direction == "up")
                {
                    switch (sort)
                    {
                        case "score": model = model.OrderBy(x => x.TotalScore).ToList(); break;
                        case "cpu": model = model.OrderBy(x => x.CpuScore).ToList(); break;
                        case "ram": model = model.OrderBy(x => x.RamScore).ToList(); break;
                        case "disk": model = model.OrderBy(x => x.DiskScore).ToList(); break;
                        case "display": model = model.OrderBy(x => x.DisplayScore).ToList(); break;
                        case "ad": model = model.OrderBy(x => x.UserName).ToList(); break;
                        case "user": model = model.OrderBy(x => x.User).ToList(); break;
                        case "os": model = model.OrderBy(x => x.OSScore).ToList(); break;
                        default: model = model.OrderBy(x => x.Name).ToList(); break;
                    }
                }
                else
                {
                    switch (sort)
                    {
                        case "score": model = model.OrderByDescending(x => x.TotalScore).ToList(); break;
                        case "cpu": model = model.OrderByDescending(x => x.CpuScore).ToList(); break;
                        case "ram": model = model.OrderByDescending(x => x.RamScore).ToList(); break;
                        case "disk": model = model.OrderByDescending(x => x.DiskScore).ToList(); break;
                        case "display": model = model.OrderByDescending(x => x.DisplayScore).ToList(); break;
                        case "ad": model = model.OrderByDescending(x => x.UserName).ToList(); break;
                        case "user": model = model.OrderByDescending(x => x.User).ToList(); break;
                        case "os": model = model.OrderByDescending(x => x.OSScore).ToList(); break;
                        default: model = model.OrderByDescending(x => x.Name).ToList(); break;
                    }
                }

                ViewBag.Sort = sort;
                ViewBag.Direction = direction;

                return View("List", model.ToList());
            }
        }

        public ActionResult Search(string query, string type)
        {
            using (var db = new DevinContext())
            {
                switch (type)
                {
                    case "apps":
                        var apps = db.Item
                            .Where(x => x.IPage == "Установленные программы")
                            .Where(x => x.IField == "Версия")
                            .Where(x => x.IDevice.ToLower().Contains(query))
                            .Join(db.Report, i => i.ReportID, r => r.ID, (i, r) => new Item
                            {
                                ReportID = r.ID,
                                ReportHost = r.RHost,
                                IDevice = i.IDevice,
                                IValue = i.IValue
                            })
                            .ToList()
                            .OrderBy(x => x.ReportHost)
                                .ThenBy(x => x.IDevice)
                                .ThenBy(x => x.IValue)
                            .ToList();

                        return View("SearchPartials/Apps", apps);

                    case "summaries":
                        var summaries = db.Item
                            .Where(x => x.IPage == "Суммарная информация")
                            .Where(x => (x.IGroup + x.IField + x.IValue).ToLower().Contains(query))
                            .Join(db.Report, i => i.ReportID, r => r.ID, (i, r) => new Item
                            {
                                ReportID = r.ID,
                                ReportHost = r.RHost,
                                IGroup = i.IGroup,
                                IField = i.IField,
                                IValue = i.IValue
                            })
                            .ToList()
                            .OrderBy(x => x.ReportHost)
                                .ThenBy(x => x.IGroup)
                                .ThenBy(x => x.IField)
                                .ThenBy(x => x.IValue)
                            .ToList();

                        return View("SearchPartials/Summary", summaries);

                    case "fulltext":
                        var items = db.Item
                            .Where(x => (x.IPage + x.IGroup + x.IDevice + x.IField + x.IValue).ToLower().Contains(query))
                            .Join(db.Report, i => i.ReportID, r => r.ID, (i, r) => new Item
                            {
                                ReportID = r.ID,
                                ReportHost = r.RHost,
                                IPage = i.IPage,
                                IGroup = i.IGroup,
                                IDevice = i.IDevice,
                                IField = i.IField,
                                IValue = i.IValue
                            })
                            .ToList()
                            .OrderBy(x => x.ReportHost)
                                .ThenBy(x => x.IDevice)
                                .ThenBy(x => x.IValue)
                            .ToList();

                        return View("SearchPartials/Fulltext", items);

                    case "computers":
                        var ids = db.Report
                            .GroupBy(x => x.RHost)
                            .Select(g => g.Select(x => x.ID).Max())
                            .ToList();

                        var reports = from r in db.Report
                                      from i in db.Item.LeftJoin(x => x.ReportID == r.ID).DefaultIfEmpty(new Item { IValue = "" })
                                      where ids.Contains(r.ID) && i.IField == "Операционная система" && i.IPage == "Отчёт" && r.RHost.Contains(query)
                                      orderby r.RHost
                                      select new Report
                                      {
                                          ID = r.ID,
                                          RHost = r.RHost,
                                          RUser = r.RUser,
                                          RDateTime = r.RDateTime,
                                          Windows = i.IValue,
                                      };

                        return View("SearchPartials/Computers", reports.ToList());
                }

                return HttpNotFound();
            }
        }


        // card partials

        public ActionResult Card(int Id) => View(model: Id);

        public ActionResult Summary(int Id) => View("CardPartials/Summary", model: Id);

        public ActionResult Software(int Id) => View("CardPartials/Software", model: Id);

        public ActionResult All(int Id) => View("CardPartials/All", model: Id);

        public ActionResult Autorun(int Id) => View("CardPartials/Autorun", model: Id);

        public ActionResult Devices(int Id) => View("CardPartials/Devices", model: Id);

        public ActionResult Programs(int Id) => View("CardPartials/Programs", model: Id);


        // actions

        public JsonResult Move(string[] items, string destination)
        {
            int destId = destination.IndexOf('|') > -1 
                ? int.Parse(destination.Substring(destination.IndexOf('|') + 1)) 
                : 0;

            using (var db = new DevinContext())
            {
                foreach (var item in items)
                {
                    string itemType = item.Substring(0, item.IndexOf('|'));
                    int itemId = int.Parse(item.Substring(item.IndexOf('|') + 1));

                    switch (itemType)
                    {
                        case "report":
                            db.Report
                                .Where(x => x.ID == itemId)
                                .Set(x => x.PlaceId, destId)
                                .Update();
                            break;

                        case "folder":
                            db.Folders
                                .Where(x => x.Id == itemId)
                                .Set(x => x.FolderId, destId)
                                .Update();
                            break;
                    }
                }

                return Json(new { Done = "Позиция сохранена" });
            }
        }

        public JsonResult Delete(int Id)
        {
            using (var db = new DevinContext())
            {
                string name = db.Report.Where(x => x.ID == Id).Select(x => x.RHost).FirstOrDefault() ?? "Наименование не определено";

                db.Report.Where(x => x.ID == Id).Delete();
                db.Item.Where(x => x.ReportID == Id).Delete();
                db.Log(User, "aida", Id, "Отчет по компьютеру \"" + name + "\" удален из базы данных");

                return Json(new { Done = "Отчет о компьютере удален из базы" });
            }
        }
    }
}