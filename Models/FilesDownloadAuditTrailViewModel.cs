using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RMA_Docker.Models {
    public class FilesDownloadAuditTrailViewModel {
        public Guid FileID { get; set; }
        public String UserName { get; set; }
        public String NiceNameOrAreaName { get; set; }
        public System.DateTime DateTimeDownloaded { get; set; }
    }

    public class FilesDownloadAuditTrailJsonModel {
        public String UserName { get; set; }
        public String NiceNameOrAreaName { get; set; }
        public String DateTimeDownloaded { get; set; } 
    }
}