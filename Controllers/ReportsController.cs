using RMA_Docker.Classes;
using RMA_Docker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace RMA_Docker.Controllers {

    [Authorize]
    public class ReportsController : Controller {
        // GET: Reports
        public ActionResult Reports() {
            ViewBag.UserData = getDropDownList();
            return View();
        }

        private List<SelectListItem> getDropDownList() {
            HashSet<String> hashArr = (new ReportOperations()).GetUsersInAuditTrails();
            MembershipUserCollection users = Membership.GetAllUsers();
            foreach (MembershipUser user in users) { hashArr.Add(user.UserName); }
            List<SelectListItem> dropDownList = new List<SelectListItem>();
            foreach (String userName in hashArr) {
                dropDownList.Add(new SelectListItem { Text = userName, Value = userName });
            }
            return dropDownList;
        }

        private String GetTheLogoPath() {
            String logoPath = @"\Content\Images\logo.png";
            String physicalPath = Server.MapPath(logoPath);
            return physicalPath;
        }

        [HttpPost]
        public FileContentResult GenerateTotalDocumentsDownloadedReport() {
            String physicalPath = GetTheLogoPath();
            ReportOperations reportOperations = new ReportOperations();
            Byte[] bytestream = reportOperations.GenerateReportForTotalDocumentsDownloaded(physicalPath);
            return File(bytestream, "application/pdf", "TotalDocumentsDownloaded" + "Report.pdf");
        }

        [HttpPost]
        public FileContentResult GenerateDocumentsDownloadedReportForASpecificUser(String userName) {
            String physicalPath = GetTheLogoPath();
            ReportOperations reportOperations = new ReportOperations();
            Byte[] bytestream = reportOperations.GenerateReportForDocumentsDownloadedBySpecificUser(physicalPath, userName);
            return File(bytestream, "application/pdf", userName + "_DocumentsDownloaded" + "Report.pdf");
        }

        [HttpPost]
        public FileContentResult GenerateUserActivityReportForASpecificUser(String userName) {
            String physicalPath = GetTheLogoPath();
            ReportOperations reportOperations = new ReportOperations();
            Byte[] bytestream = reportOperations.GenerateReportForUserActivity(physicalPath, userName);
            return File(bytestream, "application/pdf", userName + "_UserActivity" + "Report.pdf");
        }

        [HttpPost]
        public JsonResult GetTotalDocumentsDownloadedReport() {
            List<FilesDownloadAuditTrail> filesDownloadAuditTrails = (new AuditTrailOperations()).GetTotalFilesDownloadedAuditTrails();
            List<FilesDownloadAuditTrailJsonModel> fileDownloadedList = new List<FilesDownloadAuditTrailJsonModel>();
            foreach (FilesDownloadAuditTrail item in filesDownloadAuditTrails) {
                FilesDownloadAuditTrailJsonModel filesDownloadAuditTrailJsonModel = new FilesDownloadAuditTrailJsonModel();
                filesDownloadAuditTrailJsonModel.UserName = item.UserName;
                filesDownloadAuditTrailJsonModel.NiceNameOrAreaName = item.FileName;
                filesDownloadAuditTrailJsonModel.DateTimeDownloaded = (item.DateTimeDownloaded).ToString();
                fileDownloadedList.Add(filesDownloadAuditTrailJsonModel);
            }
            return Json(new { 
                Total = fileDownloadedList.Count,
                Data = fileDownloadedList
            });
        }

        [HttpPost]
        public JsonResult GetDocumentsDownloadedReportBySpecificUser(string userName) {
            List<FilesDownloadAuditTrailJsonModel> filesDownloadAuditTrailJsonModeldata = new List<FilesDownloadAuditTrailJsonModel>();
            if (!String.IsNullOrEmpty(userName)) {
                List<FilesDownloadAuditTrail> filesDownloadedList = (new AuditTrailOperations()).GetFilesDownloadedAuditTrailsBySpecificUser(userName);
                foreach (FilesDownloadAuditTrail item in filesDownloadedList) {
                    FilesDownloadAuditTrailJsonModel filesDownloadAuditTrailJsonModel = new FilesDownloadAuditTrailJsonModel();
                    filesDownloadAuditTrailJsonModel.UserName = item.UserName;
                    filesDownloadAuditTrailJsonModel.NiceNameOrAreaName = item.FileName;
                    filesDownloadAuditTrailJsonModel.DateTimeDownloaded = (item.DateTimeDownloaded).ToString();
                    filesDownloadAuditTrailJsonModeldata.Add(filesDownloadAuditTrailJsonModel);
                }
            } 
            return Json(new { 
                Total = filesDownloadAuditTrailJsonModeldata.Count,
                Data = filesDownloadAuditTrailJsonModeldata
            });
        }

        [HttpPost]
        public JsonResult GetUserActivityReportForASpecificUser(string userName) {
            List<UserLoginAuditTrailJsonModel> UserLoginAuditTrailViewModeldata = new List<UserLoginAuditTrailJsonModel>();
            if (!String.IsNullOrEmpty(userName)) {
                AuthenticationsAndAuthorizationsOperations aNaOps = new AuthenticationsAndAuthorizationsOperations();
                List<UserLoginAuditTrail> userActivityAuditTrails = aNaOps.GetUserActivityAuditTrailsBySpecificUser(aNaOps.GetUserIDByUserName(userName));
                foreach (UserLoginAuditTrail item in userActivityAuditTrails) {
                    UserLoginAuditTrailJsonModel userLoginAuditTrailJsonModel = new UserLoginAuditTrailJsonModel();
                    userLoginAuditTrailJsonModel.UserID = (item.UserID).ToString();
                    userLoginAuditTrailJsonModel.UserName = item.UserName;
                    userLoginAuditTrailJsonModel.DateTimeLogged = (item.DateTimeLogged).ToString();
                    UserLoginAuditTrailViewModeldata.Add(userLoginAuditTrailJsonModel);
                }
            }
            return Json(new {
                Total = UserLoginAuditTrailViewModeldata.Count,
                Data = UserLoginAuditTrailViewModeldata
            });
        }

    }

}