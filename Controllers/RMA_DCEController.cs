using RMA_Docker.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RMA_Docker.Models;
using aspnet_mvc_razor_app.Controllers;
using Newtonsoft.Json;
using aspnet_mvc_razor_app.Classes;

namespace RMA_Docker.Controllers {

    [Authorize]
    public class RMA_DCEController : OCRController {

        //default loading page for New Template page.
        public ActionResult AddDocument() {
            return View();
        }

        public JsonResult UploadTemplate() {
            String retJsonMsg = String.Empty;
            foreach (String file in Request.Files) {
                var fileContent = Request.Files[file];
                var docType = Request.Form["DocType"];
                var docDesc = Request.Form["DocDesc"];
                var skipPages = Request.Form["SkipPages"];
                List<int> pagesToSkill = new List<int>();
                if (skipPages.Length > 0) {
                    List<String> skippages = skipPages.Split(',').ToList();
                    foreach (String skip in skippages) {
                        int number;
                        bool retInt = int.TryParse(skip, out number);
                        if (retInt) { pagesToSkill.Add(number); }
                        else { retJsonMsg = "ERROR - Skip page value ["+skip+"] is not a number!"; break; }
                    }
                }
                if (fileContent != null && fileContent.ContentLength > 0 && String.IsNullOrEmpty(retJsonMsg)) {
                    String virtualPath = UtilityOperations.GetDCEDockerRootPath();
                    String physicalPath = DecodePath(Path.Combine(UtilityOperations.GetServerMapPath(virtualPath), Path.GetFileName(fileContent.FileName)));
                    String userName = HttpContext.User.Identity.Name;
                    DocumentsOperations docOps = new DocumentsOperations(); 
                    if (docOps.GetFileByVirtualPath(virtualPath + "/" + fileContent.FileName) == null) {
                        System.IO.Directory.CreateDirectory(UtilityOperations.GetServerMapPath(virtualPath));
                        fileContent.SaveAs(physicalPath);
                        docOps.InsertFileEncodeFileName(fileContent.FileName, virtualPath + "/" + fileContent.FileName, "_DCEDockerFile", docDesc, userName, false);
                        DCEOperations dceOps = new DCEOperations();
                        dceOps.InsertTemplate(virtualPath + "/" + fileContent.FileName, docType, docDesc, userName);
                        List<String> retStrArr = GetPDFContents(physicalPath, pagesToSkill);
                        dceOps.InsertOCRContents(fileContent.FileName, retStrArr[0], retStrArr[1]);
                    } else { retJsonMsg = "ERROR - The file already eixsts."; }
                }
            }
            return new JsonResult() { Data = retJsonMsg };
        }

        public JsonResult GetUploadedTemplates() {
            DCEOperations dceOps = new DCEOperations();
            List<UploadedTemplates> listAllTemplates = new List<UploadedTemplates>();
            foreach (DCE_Templates template in dceOps.getAllUploadedTemplates()) {
                listAllTemplates.Add(new UploadedTemplates{
                    Name = template.Name,
                    DocumentType = template.DocumentType,
                    VirtualPath = template.VirtualPath,
                    DateTimeUploaded = template.DateTimeUploaded.ToString(),
                    Description = template.Description,
                    FileID = template.FileID
                });
            }
            return Json(new { Total = listAllTemplates.Count, Data = listAllTemplates });
        }

        [HttpPost]
        public JsonResult DeleteTemplate(String virtualPath) {
            bool retJsonBool = false;
            DocumentsOperations docOps = new DocumentsOperations();
            if (docOps.GetFileByVirtualPath(virtualPath) != null) {
                DCEOperations dceOps = new DCEOperations();
                Guid fileID = dceOps.GetFileIDFromTemplateByVirtualPath(virtualPath);
                if (fileID != Guid.Empty) {
                    dceOps.DeleteUploadedTemplateKeywords(fileID);
                    dceOps.DeleteUploadedTemplates(virtualPath);
                    docOps.DeleteFile(virtualPath);
                    String physicalPath = UtilityOperations.GetServerMapPath(virtualPath);
                    physicalPath = DecodePath(physicalPath);
                    if (System.IO.File.Exists(physicalPath)) { System.IO.File.Delete(physicalPath); }
                    retJsonBool = true;
                }
            }
            return new JsonResult() { Data = retJsonBool };
        }

        private String DecodePath(String path) {
            if (String.IsNullOrEmpty(path) || path == "/") { path = UtilityOperations.GetDockerRootPath(Server) + "/DCEDocker"; }
            String decodePath = path.Replace("/", "\\");
            return decodePath;
        }

        public ActionResult GenerateKeywords(String virtualPath) {
            DCEOperations dceOps = new DCEOperations();
            TemplateKeywords templateKeywords = new TemplateKeywords();
            templateKeywords.Keywords = GetKeywordsByVirtualPath(virtualPath);
            DCE_Templates template = dceOps.GetTemplateByVirtualPath(virtualPath);
            if (template != null) {
                templateKeywords.Template = GenerateUploadedTemplates(template);
                templateKeywords.SkipPages = template.SkipPages;
            }
            templateKeywords.TemplateOCRContent = template.DocumentOCRContent;
            return View(templateKeywords);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GenerateKeywords(TemplateKeywords templateKeywords, String virtualPath, String inputKeyword, String templateDocType, String submit) {
            DCEOperations dceOps = new DCEOperations();
            DCE_Templates template = dceOps.GetTemplateByVirtualPath(virtualPath);
            if (template != null) {
                templateKeywords.Template = GenerateUploadedTemplates(template);
                templateKeywords.SkipPages = template.SkipPages;
            }
            templateKeywords.TemplateOCRContent = template.DocumentOCRContent;
            switch (submit) {
                case "System Generation":
                    templateKeywords.GeneratedBySystem = true;
                    List<InputtedKeyword> arr = dceOps.GetKeywordsGeneratedBySystem(templateKeywords.Template);
                    templateKeywords.Keywords = dceOps.GetKeywordsGeneratedBySystem(templateKeywords.Template);
                    break;
                case "Keyword Checking":
                    if (inputKeyword.Trim() == "") {
                        ViewData["GenerateKeywordsReturnMsg"] = "Input Keyword is empty ...";
                    } else {
                        templateKeywords.GeneratedBySystem = true;
                        templateKeywords.Keywords = dceOps.GetKeywordByInput(templateKeywords.Template, inputKeyword);
                    }
                    break;
                case "Added Keywords":
                    return RedirectToAction("GenerateKeywords", new { virtualPath = virtualPath });

                case "Update":
                    DocumentsOperations docOps = new DocumentsOperations();
                    docOps.RenameDocumentTypeByVirtualPath(templateKeywords.Template.VirtualPath, templateDocType);
                    templateKeywords.Template.DocumentType = templateDocType;
                    break;
                default:
                    templateKeywords.GeneratedBySystem = false;
                    templateKeywords.Keywords = GetKeywordsByVirtualPath(virtualPath);
                    break;
            }
            return View(templateKeywords);
        }

        [HttpPost]
        public JsonResult AddKeyword(String fileID, String userName,String keyword, String rank) {
            DCEOperations dceOps = new DCEOperations();
            AuthenticationsAndAuthorizationsOperations aNaOps = new AuthenticationsAndAuthorizationsOperations();
            return new JsonResult() { Data = dceOps.InsertKeyword(new Guid(fileID), aNaOps.GetUserIDByUserName(userName), keyword.Trim(), Convert.ToDouble(rank)) };
        }

        [HttpPost]
        public JsonResult DeleteKeyword(String fileID, String userName, String keyword) {
            DCEOperations dceOps = new DCEOperations();
            AuthenticationsAndAuthorizationsOperations aNaOps = new AuthenticationsAndAuthorizationsOperations();
            return new JsonResult() { Data = dceOps.DeleteKeyword(new Guid(fileID), aNaOps.GetUserIDByUserName(userName), keyword.Trim()) };
        }

        [HttpPost]
        public JsonResult UpdateRankNumber() {
            var fileID = Request.Form["fileID"];
            var userName = Request.Form["userName"];
            var keyword = Request.Form["keyword"];
            var rankVal = Request.Form["rank"];
            DCEOperations dceOps = new DCEOperations();
            dceOps.UpdateRankByKeywordNFileID(new Guid(fileID), keyword, Convert.ToDouble(rankVal));
            return new JsonResult() { Data=true };
        }

        private List<InputtedKeyword> GetKeywordsByVirtualPath(String virtualPath) {
            DCEOperations dceOps = new DCEOperations();
            List<DCE_Keywords> keywords = dceOps.GetKeywordsByFileID(dceOps.GetFileIDFromTemplateByVirtualPath(virtualPath));
            List<InputtedKeyword> inputtedKeywords = new List<InputtedKeyword>();
            if (keywords != null) {
                foreach (DCE_Keywords keyword in keywords) {
                    InputtedKeyword inputtedKeyword = new InputtedKeyword {
                        Keyword = keyword.Keyword,
                        Rank = (decimal)keyword.Rank,
                    };
                    inputtedKeywords.Add(inputtedKeyword);
                }
                return inputtedKeywords;
            }
            return null;
        }

        private UploadedTemplates GenerateUploadedTemplates(DCE_Templates template) {
            if (template != null) {
                UploadedTemplates uploadedTemplate = new UploadedTemplates {
                    Name = template.Name,
                    DocumentType = template.DocumentType,
                    VirtualPath = template.VirtualPath,
                    DateTimeUploaded = template.DateTimeUploaded.ToString(),
                    Description = template.Description,
                    FileID = template.FileID
                };
                return uploadedTemplate;
            }
            return null;
        }

        public void DownloadDCETEmplateJson(String virtualPath) {
            DCEOperations dceOps = new DCEOperations();
            DCE_Templates template = dceOps.GetTemplateByVirtualPath(virtualPath);
            DCETemplateKeywordsInJson retJson = new DCETemplateKeywordsInJson();
            if (template != null) {
                retJson.DocType = template.DocumentType;
                List<DCEKeysList> dceKeysList = new List<DCEKeysList>();
                foreach (InputtedKeyword keyword in GetKeywordsByVirtualPath(virtualPath)) {
                    dceKeysList.Add(new DCEKeysList { Keyword = keyword.Keyword, Rank = keyword.Rank });
                    retJson.Score += keyword.Rank;
                }
                retJson.KeysList = dceKeysList;
            }
            HttpContext httpContext = this.HttpContext.ApplicationInstance.Context;
            UtilityOperations.DownloadJson(JsonConvert.SerializeObject(retJson), template.DocumentType, httpContext);
        }

        public ActionResult DocClassification() {
            return View(new DocClassification());
        }

        [HttpPost]
        public ActionResult DocClassification(HttpPostedFileBase file) {
            DocClassification docCls = new DocClassification();
            try {
                String physicalPath = "";
                if (file.ContentLength > 0) {
                    String fileHashName = (DateTime.Now).ToString("yyyyMMddHHmmssffff")+file.FileName;
                    physicalPath = DecodePath(
                        Path.Combine(
                            UtilityOperations.GetServerMapPath(
                                UtilityOperations.GetDCEDockerRootPath()) + "\\_temp", 
                                Path.GetFileName(fileHashName)));
                    file.SaveAs(physicalPath);
                    String varet = (new DocumentsOperations()).IdentifyDocumentType(physicalPath);
                    FileInfo uploadedFile = new FileInfo(physicalPath);
                    docCls.DocClassifiedOutcome += "<input hidden id=\"UploadedFileName\" value=\"" + uploadedFile.Name + "\"/>";
                    docCls.DocClassifiedOutcome += "<span>File Name : " + (uploadedFile.Name).Substring(18) + "</span><br/>";
                    docCls.DocClassifiedOutcome += "<span>Document Type : " + varet + "</span><br/>";
                    docCls.DocClassifiedOutcome += "<span>File Uploaded @ : " + uploadedFile.CreationTime + "</span><br/>";
                } else {
                    docCls.DocClassifiedMsg = "Error - Please attach the file ...";
                }
            } catch (Exception ex) {
                docCls.DocClassifiedMsg = "Error - File upload failed! "+ ex.Message;
            }

            return View(docCls);
        }
    }

}