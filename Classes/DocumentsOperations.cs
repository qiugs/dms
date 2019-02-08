using aspnet_mvc_razor_app.Classes;
using RMA_Docker.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;

namespace RMA_Docker.Classes {

    public class DocumentsOperations {

        #region AuthorizationRelated
        public void InsertUsersFilesAuthorizations(Guid userID, Guid fileID) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                File file = (from p in dockerEntities.Files
                             where p.ID == fileID
                             select p).First();
                aspnet_Users user = (from p in dockerEntities.aspnet_Users
                                     where p.UserId == userID
                                     select p).First();
                if (!file.aspnet_Users.Contains(user)) {
                    file.aspnet_Users.Add(user);
                    dockerEntities.SaveChanges();
                }
            }
        }

        public void RemoveUsersFilesAuthorizations(Guid userID, Guid fileID) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                aspnet_Users user = (from p in dockerEntities.aspnet_Users
                                     where p.UserId == userID
                                     select p).First();
                foreach (File file in user.Files.ToList()) { if (file.ID == fileID) { user.Files.Remove(file); break; } }
                dockerEntities.SaveChanges();
            }
        }

        public void RemoveUsersFilesAuthorizationsByUserName(Guid userID) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                aspnet_Users user = (from p in dockerEntities.aspnet_Users
                                     where p.UserId == userID
                                     select p).First();
                foreach (File file in user.Files.ToList()) { user.Files.Remove(file); }
                foreach (FeatureAccessProfile fap in user.FeatureAccessProfiles.ToList()) { user.FeatureAccessProfiles.Remove(fap); }
                dockerEntities.SaveChanges();
            }
        }

        public void InsertRolesFilesAuthorizations(Guid roleID, Guid fileID) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                File file = (from p in dockerEntities.Files
                             where p.ID == fileID
                             select p).First();
                aspnet_Roles role = (from p in dockerEntities.aspnet_Roles
                                     where p.RoleId == roleID
                                     select p).First();
                if (!file.aspnet_Roles.Contains(role)) {
                    file.aspnet_Roles.Add(role);
                    dockerEntities.SaveChanges();
                }
            }
        }

        public void RemoveRolesFilesAuthorizations(Guid roleID, Guid fileID) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                aspnet_Roles role = (from p in dockerEntities.aspnet_Roles
                                     where p.RoleId == roleID
                                     select p).First();
                foreach (File file in role.Files.ToList()) { if (file.ID == fileID) { role.Files.Remove(file); break; } }
                dockerEntities.SaveChanges();
            }
        }

        public void RemoveRolesFilesAuthorizationsByRoleID(Guid roleID) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                aspnet_Roles role = (from p in dockerEntities.aspnet_Roles
                                     where p.RoleId == roleID
                                     select p).First();
                foreach (File file in role.Files.ToList()) { role.Files.Remove(file); }
                foreach (FeatureAccessProfile fap in role.FeatureAccessProfiles.ToList()) { role.FeatureAccessProfiles.Remove(fap); }
                dockerEntities.SaveChanges();
            }
        }

        public void PopulateNiceNameAndDescription(IList<FileModel> files) {
            List<String> virtualPaths = new List<String>();
            foreach (var item in files) { virtualPaths.Add(UtilityOperations.GetVirtualPath(item.FullPath)); }
            DocumentsOperations documentsOperations = new DocumentsOperations();
            List<File> filelist = documentsOperations.GetFilesByVirtualPaths(virtualPaths);
            foreach (var item in files) {
                List<File> tempfilelist = filelist.Where(x => x.Name == item.Name).ToList();
                if (tempfilelist.Count != 0) {
                    item.NiceNameOrAreaName = tempfilelist[0].NiceNameOrAreaName;
                    item.Description = tempfilelist[0].Description;
                }
            }
        }

        public IList<FileModel> FilterBasedOnAuthorizationsAndPopulateNiceNameAndDescription(IList<FileModel> files, IList<File> filelist) {
            IList<FileModel> resultantfiles = new List<FileModel>();
            List<String> virttualPaths = new List<String>();
            foreach (var item in files) {
                String virtualPath = UtilityOperations.GetVirtualPath(item.FullPath);
                List<File> tempfilelist = filelist.Where(x => x.VirtualPath == virtualPath).ToList();
                if (tempfilelist.Count != 0) {
                    item.NiceNameOrAreaName = tempfilelist[0].NiceNameOrAreaName;
                    item.Description = tempfilelist[0].Description;
                    resultantfiles.Add(item);
                }
            }
            return resultantfiles;
        }

        public String[] GetAllUsersFolderInArray(String rootPath) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                String[] usersPath = (from p in dockerEntities.aspnet_Users
                                      select p.UserName).Select(user => rootPath + user).ToArray();
                return usersPath;
            }
        }
        #endregion

        #region FileOperations
        public List<File> GetFilesByUserName(String userName) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                dockerEntities.Configuration.LazyLoadingEnabled = false;
                List<File> fileSet1 = (from p in dockerEntities.aspnet_Users
                                       from q in p.Files
                                       where p.UserName == userName
                                       select q).ToList<File>();
                String[] roles = System.Web.Security.Roles.GetRolesForUser(userName);
                List<File> fileSet2 = (from p in dockerEntities.aspnet_Roles
                                       from q in p.Files
                                       where roles.Contains(p.RoleName)
                                       select q).ToList<File>();
                fileSet1.AddRange(fileSet2);
                return fileSet1;
            }
        }

        public List<String> GetVirtualPathsBySearchValue(String searchValue, String userName = null) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                dockerEntities.Configuration.LazyLoadingEnabled = false;
                List<String> vPaths = null;
                searchValue = searchValue.Replace(" ", "%").Trim();
                String queryStr = "";
                if (String.IsNullOrEmpty(userName)) {               
                    queryStr = "SELECT VirtualPath FROM Files WHERE lower(Name) LIKE lower('%"+ searchValue + "%')";
                } else {
                    queryStr = "SELECT f.VirtualPath FROM Files f INNER JOIN UsersFilesAuthorizations ufa ON f.ID = ufa.FileID " +
                                "INNER JOIN aspnet_Users au ON ufa.UserID = au.UserId WHERE lower(f.Name) LIKE lower('%"+ searchValue + 
                                "%') AND au.LoweredUserName = lower('"+userName+"') UNION SELECT f.VirtualPath FROM Files f " +
                                "INNER JOIN RolesFilesAuthorizations rfa ON f.ID = rfa.FileID INNER JOIN aspnet_Roles ar ON rfa.RoleID = ar.RoleId " +
                                "INNER JOIN aspnet_UsersInRoles auir ON ar.RoleId = auir.RoleId INNER JOIN aspnet_Users au ON auir.UserId = au.UserId " +
                                "WHERE lower(f.Name) LIKE lower('%"+ searchValue + "%') AND au.LoweredUserName = lower('"+ userName + "')";
                }
                vPaths = dockerEntities.Database.SqlQuery<String>(queryStr).ToList();
                return vPaths;
            }
        }

        public List<String> GetVirtualPathsByTagName(Guid userID, String tagName) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                dockerEntities.Configuration.LazyLoadingEnabled = false;
                List<String> vPaths = (from user in dockerEntities.aspnet_Users
                                       from file in user.Files
                                       from tag in file.TagsOnFiles
                                       where user.UserId == userID && tag.TagName == tagName
                                       select file.VirtualPath).ToList();
                return vPaths;
            }
        }

        private List<FileModel> GetFilesByFileList(List<File> files) {
            List<FileModel> fileModels = new List<FileModel>();
            foreach (File file in files) {
                FileModel dbFiles = new FileModel();
                dbFiles.Description = file.Description;
                dbFiles.VirtualPath = file.VirtualPath;
                dbFiles.NiceNameOrAreaName = file.NiceNameOrAreaName;
                dbFiles.UploadedDT = file.DateTimeUploaded;
                dbFiles.Name = file.Name;
                fileModels.Add(dbFiles);
            }
            return fileModels;
        }

        public List<File> GetFilesByVirtualPaths(List<String> virtualPaths) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                dockerEntities.Configuration.LazyLoadingEnabled = false;
                List<File> query = (from p in dockerEntities.Files
                                    where virtualPaths.Contains(p.VirtualPath)
                                    select p).ToList<File>();
                return query;
            }
        }

        public File GetFileByVirtualPath(String virtualPath) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                dockerEntities.Configuration.LazyLoadingEnabled = false;
                File file = (from p in dockerEntities.Files
                             where p.VirtualPath == virtualPath
                             select p).FirstOrDefault();
                if (file == null) { return null; }
                return file;
            }
        }

        public List<String> GetUsersNameByFileID(Guid fileID) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                dockerEntities.Configuration.LazyLoadingEnabled = false;
                List<String> users = (from p in dockerEntities.aspnet_Users
                                      where p.Files.Any(file => file.ID == fileID)
                                      select p.UserName).ToList();
                return users;
            }
        }

        public List<String> GetRolesNameByFileID(Guid fileID) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                dockerEntities.Configuration.LazyLoadingEnabled = false;
                List<String> roles = (from p in dockerEntities.aspnet_Roles
                                      where p.Files.Any(file => file.ID == fileID)
                                      select p.RoleName).ToList();
                return roles;
            }
        }

        public List<File> GetFilesStartsWithVirtualPath(String virtualPath) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                dockerEntities.Configuration.LazyLoadingEnabled = false;
                List<File> files = (from p in dockerEntities.Files
                                    where p.VirtualPath.StartsWith(virtualPath)
                                    select p).ToList();
                return files;
            }
        }

        public Guid GetFileIDByVirtualPath(String virtualPath) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                var query = (from p in dockerEntities.Files
                             where p.VirtualPath == virtualPath
                             select p).First();
                return query.ID;
            }
        }

        public String GetFileNiceNameOrAreaName(Guid FileID) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                String niceName = (from p in dockerEntities.Files
                                   where p.ID == FileID
                                   select p.NiceNameOrAreaName).First();
                return niceName;
            }
        }

        public String GetFileName(Guid FileID) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                String name = (from p in dockerEntities.Files
                               where p.ID == FileID
                               select p.Name).First();
                return name;
            }
        }

        public int GetTotalFilesAndFolders() {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                int count = (from p in dockerEntities.Files
                             select p).Count();
                return count;
            }
        }

        public void InsertNewFolder(String virtualPath, String folderPath, String userName, String folderName) {
            virtualPath += "/" + folderName;
            if (GetFileByVirtualPath(virtualPath) == null) {
                InsertFileEncodeFileName(folderName, virtualPath, "", "", userName, false);
            }
            System.IO.Directory.CreateDirectory(folderPath);
        }

        public void InsertFile(String fileName, String virtualPath, String niceNameOrAreaName, String description, String userName) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                File file = new File();
                file.ID = Guid.NewGuid();
                file.Name = fileName;
                file.NiceNameOrAreaName = niceNameOrAreaName;
                file.VirtualPath = virtualPath;
                file.DateTimeUploaded = DateTime.Now;
                file.Description = description;
                if (!String.IsNullOrEmpty(userName) && userName != "") {
                    aspnet_Users user = (from p in dockerEntities.aspnet_Users
                                         where p.UserName == userName
                                         select p).First();
                    file.aspnet_Users.Add(user);
                }
                dockerEntities.Files.Add(file);
                dockerEntities.SaveChanges();
            }
        }

        public void InsertFileEncodeFileName(String fileName, String virtualPath, String niceNameOrAreaName, String description, String userName, bool encryptFileName) {
            if (encryptFileName) { fileName = new Algorithm().EncryptString(fileName); }
            InsertFile(fileName, virtualPath, niceNameOrAreaName, description, userName);
        }

        public void InsertNewUserFolder(HttpServerUtilityBase server, String userName) {
            String virtualPath = UtilityOperations.GetDockerRootPath();
            String decodePath = UtilityOperations.DecodePath(UtilityOperations.GetServerMapPath(virtualPath), server);
            String folderPath = System.IO.Path.Combine(decodePath, userName);
            if (folderPath != "\\") {
                if (!System.IO.Directory.Exists(folderPath)) {
                    InsertNewFolder(virtualPath, folderPath, userName, userName);
                }
            }
        }

        public void RenameFile(String newname, String previousVirtualPath, String newVirtualPath) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                List<File> files = (from p in dockerEntities.Files
                                    where p.VirtualPath.StartsWith(previousVirtualPath)
                                    select p).ToList();
                foreach (File file in files) {
                    file.Name = newname;
                    file.VirtualPath = (file.VirtualPath).Replace(previousVirtualPath, newVirtualPath);
                }
                dockerEntities.SaveChanges();
            }
        }

        public void RenameDocumentTypeByVirtualPath(String virtualPath, String newDocType) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                File file = (from p in dockerEntities.Files
                             where p.VirtualPath == virtualPath
                             select p).First();
                file.DCE_Templates.DocumentType = newDocType;
                dockerEntities.SaveChanges();
            }
        }

        public void DeleteFile(String previousVirtualPath) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                List<File> files = (from p in dockerEntities.Files
                                    where p.VirtualPath.StartsWith(previousVirtualPath)
                                    select p).ToList();
                if (files.Count > 0) {
                    foreach (File file in files) {
                        file.aspnet_Users.Clear();
                        file.aspnet_Roles.Clear();
                        if (file.FileBeenOCR != null) { dockerEntities.FileBeenOCRs.Remove(file.FileBeenOCR); }
                        dockerEntities.Files.Remove(file);
                    }
                }
                dockerEntities.SaveChanges();
            }
        }

        public List<TagsOnFile> GetTagsByFileID(Guid fileID) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                List<TagsOnFile> tags = (from p in dockerEntities.TagsOnFiles
                                         where p.FileID == fileID
                                         select p).ToList();
                return tags;
            }
        }

        public List<String> GetAllTagsByUserId(Guid userID) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                var files = (from p in dockerEntities.aspnet_Users
                             where p.UserId == userID
                             select p.Files).FirstOrDefault();
                List<String> tagsName = new List<string>();
                if (files != null) {
                    foreach (File file in files) {
                        foreach (TagsOnFile tag in file.TagsOnFiles) { tagsName.Add(tag.TagName); }
                    }
                }
                return tagsName.Distinct().ToList();
            }
        }

        public void InsertTagsByFileID(Guid fileID, IEnumerable<String> tags) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                File file = (from p in dockerEntities.Files
                             where p.ID == fileID
                             select p).First();
                dockerEntities.TagsOnFiles.RemoveRange(file.TagsOnFiles);
                foreach (var webTag in tags) {
                    file.TagsOnFiles.Add(new TagsOnFile {
                        FileID = fileID,
                        TagID = Guid.NewGuid(),
                        TagName = webTag,
                        LastModifiedDateTime = DateTime.Now
                    });
                }
                dockerEntities.SaveChanges();
            }
        }

        public void DeleteTagsByFileID(Guid fileID) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                File file = (from p in dockerEntities.Files
                             where p.ID == fileID
                             select p).First();
                dockerEntities.TagsOnFiles.RemoveRange(file.TagsOnFiles);
                dockerEntities.SaveChanges();
            }
        }
        #endregion

        #region OCR Documents
        public void InsertFileBeenOCR(Guid fileID, String docType) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                FileBeenOCR fileOCR = new FileBeenOCR();
                fileOCR.FileID = fileID;
                fileOCR.DocumentType = docType;
                fileOCR.DateTimeRecognised = DateTime.Now;
                File dbFile = (from p in dockerEntities.Files
                               where p.ID == fileID
                               select p).First();
                dbFile.FileBeenOCR = fileOCR;
                dockerEntities.SaveChanges();
            }
        }

        public FileBeenOCR GetFileBeenOCR(Guid fileID) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                FileBeenOCR fileOCR = (from p in dockerEntities.FileBeenOCRs
                                       where p.FileID == fileID
                                       select p).FirstOrDefault();
                return fileOCR;
            }
        }

        public bool IsFileBeenOCRByFileName(String fileName) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                File file = (from p in dockerEntities.FileBeenOCRs
                             where p.File.Name == fileName
                             select p.File).FirstOrDefault();
                if (file != null) { return true; } else { System.Diagnostics.Debug.WriteLine("= Null=" + false); }
            }
            return false;
        }

        public String IdentifyDocumentType(String physicalPath) {
            OCRUtilities ocrUtil = new OCRUtilities();
            return IndexOfDocumentTypeFromTemplates((ocrUtil.GetPDFPageContent(physicalPath, new List<int>()))[0]);
        }

        private String IndexOfDocumentTypeFromTemplates(String ocrContent) {
            DCEOperations dceOps = new DCEOperations();
            ocrContent = dceOps.RegexTextToString(ocrContent);
            var templates = dceOps.GetTemplatesFromDB();
            List<IdentificationDocTypeCalculation> templCal = new List<IdentificationDocTypeCalculation>();
            if (templates.Count > 0) {
                foreach (var template in templates) {
                    var keywords = dceOps.GetKeywordsByFileID(template.FileID);
                    if (keywords != null) {
                        IdentificationDocTypeCalculation docTypeCal = new IdentificationDocTypeCalculation();
                        docTypeCal.DocumentType = template.DocumentType;
                        docTypeCal.KeywordsTotalCount = keywords.Count;
                        foreach (var keyvalue in keywords) {
                            docTypeCal.RankTotal += keyvalue.Rank;
                            if (ocrContent.Contains(keyvalue.Keyword)) {
                                docTypeCal.KeywordMatchesAccumulation++;
                                docTypeCal.RankAccumulation += keyvalue.Rank;
                            }
                        }
                        templCal.Add(docTypeCal);
                    }
                }
            }
            var getMaxKeyworkCount = templCal.OrderByDescending(rate => rate.KeywordAppearanceRate).FirstOrDefault();
            var getMaxRank = templCal.OrderByDescending(rate => rate.RankComputeRate).FirstOrDefault();
            if (getMaxKeyworkCount != null && getMaxRank != null) {
                if (getMaxKeyworkCount.KeywordAppearanceRate > 70.0f || getMaxRank.RankComputeRate > 70.0f) {
                    if (getMaxRank.RankComputeRate > getMaxKeyworkCount.KeywordAppearanceRate) { return getMaxKeyworkCount.DocumentType; }
                    if (getMaxRank.RankComputeRate < getMaxKeyworkCount.KeywordAppearanceRate) { return getMaxRank.DocumentType; }
                }
            }
            return "N.A.";
        }
        #endregion

    }
}