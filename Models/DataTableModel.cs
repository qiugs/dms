using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RMA_Docker.Models {
    public class DataTableModel { }

    public class ReportsViewModel {
        public string DataType { get; set; }
        public int DataCount { get; set; }
        public IEnumerable<FilesDownloadAuditTrailViewModel> filesDownloadAuditTrailViewModels { get; set; }
        public IEnumerable<UserLoginAuditTrailViewModel> userLoginAuditTrailViewModels { get; set; }
    }



}