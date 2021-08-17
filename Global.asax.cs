using LinqToDB.Data;
using System.Diagnostics;
using System.Web.Mvc;
using System.Web.Routing;

namespace Devin
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            RouteTable.Routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            RouteTable.Routes.MapRoute("Default", "{controller}/{action}/{id}", defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional });

            #if DEBUG
            DataConnection.TurnTraceSwitchOn();
            DataConnection.WriteTraceLine = (s, s1, tl) => Debug.WriteLine(s + " | " + tl);
            #endif
        }
    }
}