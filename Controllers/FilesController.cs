using RMA_Docker.Classes;
using RMA_Docker.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using aspnet_mvc_razor_app.Classes;

namespace RMA_Docker.Controllers {

    [Authorize]
    public class FilesController : Controller {
        #region common
        public ActionResult Files() {
            String userName = HttpContext.User.Identity.Name;
            return View(FilterAccessibleFolderAndFiles(IsSystemAdministrator(userName), userName));
        }

        public ActionResult SubFolder(String virtualPath) {
            if (String.Equals(virtualPath, IsSystemAdministrator(HttpContext.User.Identity.Name)) || 
                String.Equals(virtualPath, UtilityOperations.GetDockerCommonFolderPath())) {
                return RedirectToAction("Files");
            }
            return View(FilterAccessibleFolderAndFiles(virtualPath, HttpContext.User.Identity.Name));
        }

        public ActionResult SearchFile(String searchValue, String virtualPath) {
            FileModel fileModel = new FileModel(virtualPath, "");
            fileModel.IsRootDir = true;
            fileModel.IsSearchResult = true;
            String userName = HttpContext.User.Identity.Name;
            DocumentsOperations docOps = new DocumentsOperations();
            AuthenticationsAndAuthorizationsOperations aNaOps = new AuthenticationsAndAuthorizationsOperations();
            if (aNaOps.IsSystemAdministratorUser(userName) || aNaOps.IsDMSAdministratorUser(userName)) { userName = null; }
            String[] usersPath = docOps.GetAllUsersFolderInArray(UtilityOperations.GetDockerRootPath() + "/");
            List<FileModel> searchCurrentResult = new List<FileModel>();
            List<FileModel> searchCommonResult = new List<FileModel>();
            foreach (String vPath in docOps.GetVirtualPathsBySearchValue(searchValue, userName)) {
                FileModel file = null;
                if (vPath.StartsWith(UtilityOperations.GetDockerCommonFolderPath())) {
                    file = (new FileModel()).GetFolderOrFile(vPath);
                    file.IsCommonFolder = true;
                    if (usersPath.Contains(file.VirtualPath)) { file.IsUserFolder = true; }
                    searchCommonResult.Add(file);
                } else if (vPath.StartsWith(virtualPath) && !String.Equals(vPath, virtualPath)) {
                    file = (new FileModel()).GetFolderOrFile(vPath);
                    if (usersPath.Contains(file.VirtualPath)) { file.IsUserFolder = true; }
                    searchCurrentResult.Add(file);
                }
            }
            fileModel.CurrentFolderFiles = searchCurrentResult;
            fileModel.CommonFolderFiles = searchCommonResult;
            fileModel.Tags = new List<String>();
            return View(fileModel);
        }

        public ActionResult SearchTags(String virtualPath, String tagName) {
            FileModel fileModel = new FileModel(virtualPath, "");
            fileModel.IsRootDir = true;
            fileModel.IsSearchResult = true;
            DocumentsOperations docOps = new DocumentsOperations();
            AuthenticationsAndAuthorizationsOperations aNaOps = new AuthenticationsAndAuthorizationsOperations();
            String[] usersPath = docOps.GetAllUsersFolderInArray(UtilityOperations.GetDockerRootPath() + "/");
            List<FileModel> searchCurrentResult = new List<FileModel>();
            List<FileModel> searchCommonResult = new List<FileModel>();
            foreach (String vPath in docOps.GetVirtualPathsByTagName(aNaOps.GetUserIDByUserName(HttpContext.User.Identity.Name),tagName)) {
                FileModel file = null;
                if (vPath.StartsWith(UtilityOperations.GetDockerCommonFolderPath())) {
                    file = (new FileModel()).GetFolderOrFile(vPath);
                    file.IsCommonFolder = true;
                    if (usersPath.Contains(file.VirtualPath)) { file.IsUserFolder = true; }
                    searchCommonResult.Add(file);
                } else {
                    file = (new FileModel()).GetFolderOrFile(vPath);
                    if (usersPath.Contains(file.VirtualPath)) { file.IsUserFolder = true; }
                    searchCurrentResult.Add(file);
                }
            }
            fileModel.CurrentFolderFiles = searchCurrentResult;
            fileModel.CommonFolderFiles = searchCommonResult;
            fileModel.Tags = new List<String>();
            return View(fileModel);
        }

        private String IsSystemAdministrator(String userName) {
            AuthenticationsAndAuthorizationsOperations aNaOps = new AuthenticationsAndAuthorizationsOperations();
            String virtualPath = UtilityOperations.GetDockerRootPath();
            if (!aNaOps.IsSystemAdministratorUser(userName) && !aNaOps.IsDMSAdministratorUser(userName)) { virtualPath += "/" + userName; }
            return virtualPath;
        }

        private FileModel FilterAccessibleFolderAndFiles(String virtualPath, String userName) {
            FileModel fileModel = null;
            AuthenticationsAndAuthorizationsOperations aNaOps = new AuthenticationsAndAuthorizationsOperations();
            DocumentsOperations docOps = new DocumentsOperations();
            FileModel commonFileModel = new FileModel(UtilityOperations.GetDockerCommonFolderPath(), "");
            if (!aNaOps.IsSystemAdministratorUser(userName) && !aNaOps.IsDMSAdministratorUser(userName)) {
                fileModel = new FileModel(virtualPath, "/"+userName);
                List<String> dbFilesVPath = docOps.GetFilesByUserName(userName).Select(file => file.VirtualPath).ToList();
                if (fileModel.IsRootDir) {
                    fileModel.CommonFolderFiles = commonFileModel.CurrentFolderFiles.Where(file => dbFilesVPath.Contains(file.VirtualPath)).ToList();
                } else { fileModel.CommonFolderFiles = new List<FileModel>(); }
                fileModel.Tags = docOps.GetAllTagsByUserId(aNaOps.GetUserIDByUserName(userName));
            } else {
                fileModel = new FileModel(virtualPath, "");
                String[] usersPath = docOps.GetAllUsersFolderInArray(UtilityOperations.GetDockerRootPath()+"/");
                foreach (FileModel file in fileModel.CurrentFolderFiles) { 
                    if (usersPath.Contains(file.VirtualPath)) { file.IsUserFolder = true; }
                }
                if (fileModel.IsRootDir) { fileModel.CommonFolderFiles = commonFileModel.CurrentFolderFiles; } 
                else { fileModel.CommonFolderFiles = new List<FileModel>(); }
                fileModel.Tags = docOps.GetAllTagsByUserId(aNaOps.GetUserIDByUserName(userName));
            }
            return fileModel;
        }

        public ActionResult CommonFolderMgt() {
            CommonFolderManagement cfm = new CommonFolderManagement();
            List<CommonFolderAssignment> cfaList = new List<CommonFolderAssignment>();
            DocumentsOperations docOps = new DocumentsOperations();

            foreach (var item in docOps.GetFilesStartsWithVirtualPath(UtilityOperations.GetDockerCommonFolderPath())) {
                cfaList.Add(new CommonFolderAssignment {
                    FolderName = item.Name,
                    CreatedAt = item.DateTimeUploaded.ToString(),
                    VirtualPath = item.VirtualPath,
                    UsersName = docOps.GetUsersNameByFileID(item.ID),
                    RolesName = docOps.GetRolesNameByFileID(item.ID) }); }
            cfm.CommonFolderAssignment = cfaList;
            return View(cfm);
        }

        [HttpPost]
        public ActionResult CommonFolderMgt(CommonFolderManagement cfm, String submit, String tableSelectedFolder) {
            DocumentsOperations docOps = new DocumentsOperations();
            String virtualPath="", physicalPath="", folderPath="";
            switch (submit) {
                case "Create":
                    virtualPath = UtilityOperations.GetDockerCommonFolderPath();
                    physicalPath = UtilityOperations.GetServerMapPath(virtualPath);
                    folderPath = Path.Combine(UtilityOperations.DecodePath(physicalPath, Server), cfm.FolderName);
                    if (folderPath != "\\") {
                        if (!Directory.Exists(folderPath)) {
                            docOps.InsertNewFolder(virtualPath, folderPath, HttpContext.User.Identity.Name, cfm.FolderName);
                        } else { TempData["CommonFolderMgtErrorMsg"] = "Warning - Folder already existed."; }
                    }
                    break;
                case "Edit":
                    return RedirectToAction("EditCommonFolder", new { virtualPath = tableSelectedFolder });

                case "Delete":
                    physicalPath = UtilityOperations.DecodePath(UtilityOperations.GetServerMapPath(tableSelectedFolder), Server);
                    FileAttributes attr = System.IO.File.GetAttributes(physicalPath);
                    if (Directory.Exists(physicalPath)) {
                        if (docOps.GetFilesStartsWithVirtualPath(tableSelectedFolder) != null) { docOps.DeleteFile(tableSelectedFolder); }
                        Directory.Delete(physicalPath, true);
                    } else { TempData["CommonFolderMgtErrorMsg"] = "Warning - Folder not exist in the system."; }
                    break;
            }
            return RedirectToAction("CommonFolderMgt");
        }

        public ActionResult EditCommonFolder(String virtualPath) {
            DocumentsOperations docOps = new DocumentsOperations();
            var file = docOps.GetFileByVirtualPath(virtualPath);
            CommonFolderAssignment cfa = new CommonFolderAssignment {
                FolderName = file.Name,
                CreatedAt = file.DateTimeUploaded.ToString(),
                VirtualPath = file.VirtualPath,
                UsersName = docOps.GetUsersNameByFileID(file.ID),
                RolesName = docOps.GetRolesNameByFileID(file.ID)};
            MembershipUserCollection allUsers = Membership.GetAllUsers();
            List<String> excludedUsers = new List<String>();
            foreach (MembershipUser user in allUsers) {
                if (!cfa.UsersName.Contains(user.UserName)) { excludedUsers.Add(user.UserName); }
            }
            cfa.ExcludedUsers = excludedUsers;
            cfa.ExcludedRoles = Roles.GetAllRoles().Except(cfa.RolesName).ToList();
            return View(cfa);
        }

        [HttpPost]
        public ActionResult EditCommonFolder(CommonFolderAssignment cfa, String submit, String editCommonFolderKey, String editCommonFolderVal) {
            DocumentsOperations docOps = new DocumentsOperations();
            AuthenticationsAndAuthorizationsOperations aNaOps = new AuthenticationsAndAuthorizationsOperations();
            switch (submit) {
                case "Add":
                    if (String.Equals(editCommonFolderKey.ToUpper(), "USER")) {
                        docOps.InsertUsersFilesAuthorizations(aNaOps.GetUserIDByUserName(editCommonFolderVal), docOps.GetFileIDByVirtualPath(cfa.VirtualPath));
                    } else if (String.Equals(editCommonFolderKey.ToUpper(), "ROLE")) {
                        docOps.InsertRolesFilesAuthorizations(aNaOps.GetRoleIDByRoleName(editCommonFolderVal), docOps.GetFileIDByVirtualPath(cfa.VirtualPath));
                    }
                    break;
                case "Remove":
                    if (String.Equals(editCommonFolderKey.ToUpper(), "USER")) {
                        docOps.RemoveUsersFilesAuthorizations(aNaOps.GetUserIDByUserName(editCommonFolderVal), docOps.GetFileIDByVirtualPath(cfa.VirtualPath));
                    } else if (String.Equals(editCommonFolderKey.ToUpper(), "ROLE")) {
                        docOps.RemoveRolesFilesAuthorizations(aNaOps.GetRoleIDByRoleName(editCommonFolderVal), docOps.GetFileIDByVirtualPath(cfa.VirtualPath));
                    }
                    break;
            }
            return RedirectToAction("EditCommonFolder", new { virtualPath = cfa.VirtualPath });
        }
        #endregion

        #region Upload and Download
        [HttpPost]
        public JsonResult Upload(String virtualPath) {
            String jsonRet = "";
            if (String.IsNullOrEmpty(virtualPath) || virtualPath == "/") {
                virtualPath = UtilityOperations.GetDockerRootPath(Server);
            }
            String userName = HttpContext.User.Identity.Name;
            foreach (String file in Request.Files) {
                var fileContent = Request.Files[file];
                var fileNiceName = Request.Form["fileNiceName"];
                var fileDesc = Request.Form["fileDesc"];
                if (fileContent != null && fileContent.ContentLength > 0) {
                    String filePath = Path.Combine(UtilityOperations.GetServerMapPath(virtualPath), Path.GetFileName(fileContent.FileName));
                    DocumentsOperations docOps = new DocumentsOperations();
                    //docOps.InsertFile(fileContent.FileName, virtualPath, fileNiceName, fileDesc);
                    docOps.InsertFileEncodeFileName(fileContent.FileName, virtualPath+"/"+ fileContent.FileName, fileNiceName, fileDesc, userName, false);
                    fileContent.SaveAs(filePath);
                    jsonRet = "Uploaded";
                }
            }
            return new JsonResult() { Data = jsonRet };
        }

        public void DownloadFile(String virtualPath, String userID) {
            String physicalPath = UtilityOperations.GetServerMapPath(virtualPath);
            physicalPath = UtilityOperations.DecodePath(physicalPath, Server);
            List<String> archives = new List<String>();
            archives.Add(physicalPath);
            RecordFileDownload(virtualPath, userID);
            UtilityOperations.DownloadFiles(archives, this.HttpContext.ApplicationInstance.Context);
        }

        public void RecordFileDownload(String virtualPath, String userName) {
            DocumentsOperations docOps = new DocumentsOperations();
            (new AuditTrailOperations()).InsertFilesDownloadAuditTrails(docOps.GetFileIDByVirtualPath(virtualPath), 
            (new AuthenticationsAndAuthorizationsOperations()).GetUserIDByUserName(userName));
        }
        #endregion

        #region FileManagement 
        public JsonResult CreateNewFolder(String virtualPath, String userName, String folderName) {
            String physicalPath =UtilityOperations.GetServerMapPath(virtualPath);
            String decodePath = UtilityOperations.DecodePath(physicalPath, Server);
            String folderPath = Path.Combine(decodePath, folderName);
            String retJsonMsg = "";
            if (folderPath != "\\") {
                if (!Directory.Exists(folderPath)) {
                    DocumentsOperations docOps = new DocumentsOperations();
                    docOps.InsertNewFolder(virtualPath, folderPath, userName, folderName);
                } else {
                    retJsonMsg += "Folder name already exists. Please enter a different folder name";
                }
            }
            return new JsonResult() { Data = retJsonMsg };
        }

        public JsonResult DeleteFile(String virtualPath) {
            bool retJsonBool = false;
            if (virtualPath.Length > 1) {
                String physicalPath = UtilityOperations.GetServerMapPath(virtualPath);
                physicalPath = UtilityOperations.DecodePath(physicalPath, Server);
                FileAttributes attr = System.IO.File.GetAttributes(physicalPath);
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory) {
                    if (Directory.Exists(physicalPath)) {
                        DocumentsOperations docOps = new DocumentsOperations();
                        if (docOps.GetFilesStartsWithVirtualPath(virtualPath) != null) { docOps.DeleteFile(virtualPath); }
                        Directory.Delete(physicalPath, true);
                        retJsonBool = true;
                    }
                } else {
                    if (System.IO.File.Exists(physicalPath)) {
                        DocumentsOperations docOps = new DocumentsOperations();
                        if (docOps.GetFileByVirtualPath(virtualPath) != null) { docOps.DeleteFile(virtualPath); }
                        System.IO.File.Delete(physicalPath);
                        retJsonBool = true;
                    }
                }
            }
            return new JsonResult() { Data = retJsonBool };
        }

        public JsonResult Rename() {
            var virtualPath = Request.Form["virtualPath"];
            var newName = Request.Form["newName"];
            var oldName = Request.Form["oldName"];
            String decodePath = UtilityOperations.DecodePath(UtilityOperations.GetServerMapPath(virtualPath), Server);
            String newDecodePath = decodePath.Replace(oldName, newName);
            String retJsonMsg = "";
            FileAttributes attr = System.IO.File.GetAttributes(decodePath);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory) {
                if (System.IO.Directory.Exists(decodePath)) {
                    System.IO.Directory.Move(decodePath, decodePath.Replace(oldName, newName));
                    retJsonMsg += " DirRenamed";
                }
            } else {
                if (System.IO.File.Exists(decodePath)) {
                    System.IO.File.Move(decodePath, decodePath.Replace(oldName, newName));
                    retJsonMsg += " FileRenamed";
                }
            }
            DocumentsOperations docOps = new DocumentsOperations();
            if (docOps.GetFileByVirtualPath(virtualPath) != null) {
                docOps.RenameFile(newName, virtualPath, virtualPath.Replace(oldName, newName));
                retJsonMsg += " DBRenamed";
            }
            return new JsonResult() { Data = retJsonMsg };
        }
        #endregion

        #region external features
        public JsonResult OCRThisFile(String virtualPath) {
            String physicalPath = UtilityOperations.DecodePath(UtilityOperations.GetServerMapPath(virtualPath), Server);
            SingleFileOCR singleFileOCR = new SingleFileOCR(physicalPath);
            String retVal = singleFileOCR.StartOCR();
            if (!retVal.StartsWith("ERROR")) {
                DocumentsOperations docOps = new DocumentsOperations();
                var file = docOps.GetFileByVirtualPath(virtualPath);
                String oriFileExt = (new System.IO.FileInfo(physicalPath)).Extension;
                String retFileExt = (new System.IO.FileInfo(retVal)).Extension;
                retFileExt = retVal.Substring(retVal.Length - (retFileExt.Length+4));
                docOps.InsertFileEncodeFileName(file.Name.Replace(oriFileExt, retFileExt), file.VirtualPath.Replace(oriFileExt, retFileExt), "", "OCR Performed @ "+ DateTime.Now, HttpContext.User.Identity.Name, false);
                docOps.InsertFileBeenOCR(docOps.GetFileIDByVirtualPath(file.VirtualPath.Replace(oriFileExt, retFileExt)), docOps.IdentifyDocumentType(retVal));
                return new JsonResult() { Data = "" };
            }
            return new JsonResult() { Data = retVal };
        }

        public JsonResult GetFileTags(String virtualPath) {
            DocumentsOperations docOps = new DocumentsOperations();
            List<TagsOnFile> tags = docOps.GetTagsByFileID(docOps.GetFileIDByVirtualPath(virtualPath));
            List<TagsMetadata> jsonTags = new List<TagsMetadata>();
            foreach (TagsOnFile tag in tags) {
                jsonTags.Add(new TagsMetadata {TagName=tag.TagName,LastModified=tag.LastModifiedDateTime.ToString()});
            }
            return Json(new { Total = jsonTags.Count, Data = jsonTags });
        }

        [HttpPost]
        public JsonResult InsertFileTags(IEnumerable<String> tags, String virtualPath) {
            DocumentsOperations docOps = new DocumentsOperations();
            if (tags != null && tags.Count() > 0) {
                docOps.InsertTagsByFileID(docOps.GetFileIDByVirtualPath(virtualPath), tags);
            } else {
                docOps.DeleteTagsByFileID(docOps.GetFileIDByVirtualPath(virtualPath));
            }
            return new JsonResult() { Data = "" };
        }
        #endregion
    }
}