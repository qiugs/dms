using System;   
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RMA_Docker.Models {
    public class DMSModel { }

    public class CommonFolderManagement {
        [Display(Name = "Folder Name")]
        public String FolderName { get; set; }
        public List<CommonFolderAssignment> CommonFolderAssignment { get; set; }
    }

    public class CommonFolderAssignment {
        [Display(Name = "Folder Name")]
        public String FolderName { get; set; }
        public String CreatedAt { get; set; }
        public String VirtualPath { get; set; }
        [Display(Name = "User")]
        public List<String> UsersName { get; set; }
        [Display(Name = "Role")]
        public List<String> RolesName { get; set; }
        public List<String> ExcludedUsers { get; set; } = null;
        public List<String> ExcludedRoles { get; set; } = null;
    }

}