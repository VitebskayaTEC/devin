using Devin.Models;
using Devin.ViewModels;
using System.Diagnostics;
using System.Web.Mvc;

namespace Devin.Controllers
{
    public class AdminController : Controller
    {
        public ActionResult Index() => View();

        public ActionResult Scripts()
        {
            return View();
        }

        public ActionResult Exec(string cmdText)
        {
            var cmd = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                }
            };
            cmd.Start();

            cmd.StandardInput.WriteLine(cmdText);
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            cmd.WaitForExit();

            string output = cmd.StandardOutput.ReadToEnd();

            return Content(output);

            // shutdown /m \\ego -r -t 5000
        }

        public ActionResult Licenses() => View();
	}
}