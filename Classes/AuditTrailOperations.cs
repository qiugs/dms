using RMA_Docker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RMA_Docker.Classes {
    public class AuditTrailOperations {

        public void InsertUserLoginAuditTrail(Guid userID) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                UserLoginAuditTrail userLognAuditTrail = new UserLoginAuditTrail();
                userLognAuditTrail.UserID = userID;
                userLognAuditTrail.UserName = (from p in dockerEntities.aspnet_Users where p.UserId == userID select p.UserName).First();
                userLognAuditTrail.DateTimeLogged = DateTime.Now;
                dockerEntities.UserLoginAuditTrails.Add(userLognAuditTrail);
                dockerEntities.SaveChanges();
            }
        }

        public void InsertFilesDownloadAuditTrails(Guid FileID, Guid UserID) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                FilesDownloadAuditTrail filesDownloadAuditTrail = new FilesDownloadAuditTrail();
                filesDownloadAuditTrail.UserID = UserID;
                filesDownloadAuditTrail.UserName = (from p in dockerEntities.aspnet_Users where p.UserId == UserID select p.UserName).First();
                filesDownloadAuditTrail.FileID = FileID;
                filesDownloadAuditTrail.FileName = (from p in dockerEntities.Files where p.ID == FileID select p.Name).First();
                filesDownloadAuditTrail.DateTimeDownloaded = DateTime.Now;
                dockerEntities.FilesDownloadAuditTrails.Add(filesDownloadAuditTrail);
                dockerEntities.SaveChanges();
            }
        }

        public List<FilesDownloadAuditTrail> GetTotalFilesDownloadedAuditTrails() {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                dockerEntities.Configuration.LazyLoadingEnabled = false;
                List<FilesDownloadAuditTrail> query = (from p in dockerEntities.FilesDownloadAuditTrails
                                                       select p).OrderByDescending(i => i.DateTimeDownloaded).ToList<FilesDownloadAuditTrail>();
                return query;
            }
        }

        public List<FilesDownloadAuditTrail> GetFilesDownloadedAuditTrailsBySpecificUser(String userName) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                dockerEntities.Configuration.LazyLoadingEnabled = false;
                Guid userID = (from p in dockerEntities.aspnet_Users
                               where p.UserName == userName
                               select p.UserId).First();
                List<FilesDownloadAuditTrail> query = (from p in dockerEntities.FilesDownloadAuditTrails
                                                       where p.UserID == userID
                                                       select p).OrderByDescending(i => i.DateTimeDownloaded).ToList<FilesDownloadAuditTrail>();
                return query;
            }
        }


    }
}