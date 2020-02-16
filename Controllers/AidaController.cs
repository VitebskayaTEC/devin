using Devin.Models;
using LinqToDB;
using System.Linq;
using System.Web.Mvc;

namespace Devin.Controllers
{
    public class AidaController : Controller
    {
        // views

        public ActionResult Index() => View();

        public ActionResult Fact() => View("Pages/Fact");


        // view partials

        public ActionResult Load(string query, string type)
        {
            using (var db = new DevinContext())
            {
                if (string.IsNullOrEmpty(query))
                {
                    var ids = db.Report
                        .GroupBy(x => x.RHost)
                        .Select(g => g.Select(x => x.ID).Max())
                        .ToList();

                    var reports = from r in db.Report
                                  from i in db.Item.LeftJoin(x => x.ReportID == r.ID).DefaultIfEmpty(new Item { IValue = "" })
                                  where ids.Contains(r.ID) && i.IField == "Версия ОС"
                                  orderby r.RHost
                                  select new Report
                                  {
                                      ID = r.ID,
                                      RHost = r.RHost,
                                      RUser = r.RUser,
                                      RDateTime = r.RDateTime,
                                      Windows = i.IValue,
                                  };

                    return View("List", reports.ToList());
                }
                else
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
                                          where ids.Contains(r.ID) && i.IField == "Версия ОС" && r.RHost.Contains(query)
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