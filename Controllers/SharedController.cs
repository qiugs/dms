using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RMA_Docker.Controllers {

    [Authorize]
    public class SharedController : Controller {
        /// <summary>
        /// This page for Maintenance
        /// </summary>
        /// <returns></returns>
        public ActionResult UnderDevelopment() {
            return View();
        }
    }

}