using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace RMA_Docker.Classes {
    public class UtilityOperations {

        public static String GetDockerRootPath() { return "~/DockerFiles/FileDocker"; }

        public static String GetDCEDockerRootPath() { return "~/DockerFiles/DCEDocker"; }

        public static String GetDockerCommonFolderPath() { return "~/DockerFiles/CommonDocker"; } 

        public static String GetServerMapPath(String path) {
            return HttpContext.Current.Server.MapPath(path);
        }

        public static String GetDockerRootPath(HttpServerUtilityBase server) {
            String rootPath = server.MapPath(GetDockerRootPath());
            return rootPath;
        }

        public static String GetVirtualPath(String physicalPath) {
            String applicationPath = System.Web.Hosting.HostingEnvironment.MapPath("~/");
            String url = physicalPath.Substring(applicationPath.Length).Replace('\\', '/').Insert(0, "~/");
            return (url);
        }

        public static void DownloadFiles(List<String> archives, HttpContext httpContext) {
            if (archives.Count == 0) { return; }
            FileAttributes attr = System.IO.File.GetAttributes(archives[0]);
            if (archives.Count == 1 && ((attr & FileAttributes.Directory) != FileAttributes.Directory)) {
                String filename = Path.GetFileName(archives[0]);
                httpContext.Response.Buffer = true;
                httpContext.Response.Clear();
                httpContext.Response.AddHeader("content-disposition", "attachment; filename=" + EncodeSpaceString(filename));
                httpContext.Response.ContentType = "application/octet-stream";
                httpContext.Response.WriteFile(archives[0]);
            } else {
                String zipName = String.Format((new DirectoryInfo(archives[0])).Name.ToString()+"-archive-{0}.zip", DateTime.Now.ToString("yyyy-MMM-dd-HHmmss"));
                httpContext.Response.Buffer = true;
                httpContext.Response.Clear();
                httpContext.Response.AddHeader("content-disposition", "attachment; filename=" + EncodeSpaceString(zipName));
                httpContext.Response.ContentType = "application/zip";
                using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile()) {
                    foreach (String path in archives) {
                        try {
                            FileAttributes attr1 = System.IO.File.GetAttributes(path); 
                            if ((attr1 & FileAttributes.Directory) == FileAttributes.Directory) { 
                                zip.AddDirectory(path, Path.GetFileNameWithoutExtension(path));
                            } else { zip.AddFile(path, ""); }
                        } catch (Exception) {}
                    }
                    zip.Save(httpContext.Response.OutputStream);
                }
            }
        }

        public static void DownloadJson(String json, String fileName, HttpContext httpContext) {
            httpContext.Response.Buffer = true;
            httpContext.Response.Clear();
            httpContext.Response.AddHeader("content-disposition", "attachment; filename=" + fileName.Replace(" ", "_") + ".json");
            httpContext.Response.ContentType = "application/json; charset=utf-8";
            httpContext.Response.Write(json);
            httpContext.Response.End();
        }

        public static List<String> StringToList(String jlist, List<String> list) {
            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            list = serializer.Deserialize<List<String>>(jlist);
            return list;
        }

        public static void LogDocumentsDownloaded(List<String> virtualPaths) {
            String userID = System.Web.Security.Membership.GetUser().ProviderUserKey.ToString();
            DocumentsOperations docOps = new DocumentsOperations();
            foreach (String path in virtualPaths) {
                Guid fileID = docOps.GetFileIDByVirtualPath(path);
                (new AuditTrailOperations()).InsertFilesDownloadAuditTrails(fileID, new Guid(userID));
            }
        }

        public static DateTime GetExpiryDate() {
            String userName = System.Web.Security.Membership.GetUser().UserName;
            AuthenticationsAndAuthorizationsOperations AuthenticationsAndAuthorizationsOperations = new AuthenticationsAndAuthorizationsOperations();
            DateTime expirydate = AuthenticationsAndAuthorizationsOperations.GetExpiryDate(userName);
            return expirydate;
        }

        private static String EncodeSpaceString(String inputStr) {
            return inputStr.Replace(" ","_");
        }

        public static String DecodePath(String path, HttpServerUtilityBase server) {
            if (String.IsNullOrEmpty(path) || path == "/") { path = UtilityOperations.GetDockerRootPath(server); }
            String decodePath = Models.FileModel.Decode(path);
            return decodePath;
        }
    }
}