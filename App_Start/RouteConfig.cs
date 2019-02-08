using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace RMA_Docker {
    public class RouteConfig {
        public static void RegisterRoutes(RouteCollection routes) {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Account", action = "Logon", id = UrlParameter.Optional },
                namespaces: new[] { "RMA_Docker.Controllers"  }
            );
            routes.MapMvcAttributeRoutes();
        }
    }
}
