using RMA_Docker.Classes;
using RMA_Docker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RMA_Docker.Controllers {

    [Authorize]
    public class HomeController : Controller {

        public ActionResult Index() {
            DashboardViewModel dvModel = new DashboardViewModel();

            DCEOperations dceOps = new DCEOperations();
            dvModel.CountTotalTemplates = dceOps.GetTotalTemplatesCount();

            AuthenticationsAndAuthorizationsOperations aNaOps = new AuthenticationsAndAuthorizationsOperations();
            dvModel.CountTotalWorkers = aNaOps.GetTotalUsersCount();
            dvModel.CountTotalRoles = aNaOps.GetTotalRolesCount();

            DocumentsOperations docOps = new DocumentsOperations();
            dvModel.CountTotalDocuments = docOps.GetTotalFilesAndFolders();
            return View(dvModel);
        }

        public ActionResult About() {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact() {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}