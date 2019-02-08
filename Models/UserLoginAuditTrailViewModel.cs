using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RMA_Docker.Models {
    public class UserLoginAuditTrailViewModel {
        public String UserID { get; set; }
        public String UserName { get; set; }
        public DateTime DateTimeLogged { get; set; } 
    }

    public class UserLoginAuditTrailJsonModel {
        public String UserID { get; set; }
        public String UserName { get; set; }
        public String DateTimeLogged { get; set; }
    }
}