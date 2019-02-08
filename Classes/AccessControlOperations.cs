using RMA_Docker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RMA_Docker.Classes {
    public class AccessControlOperations {
        AuthenticationsAndAuthorizationsOperations aNaOps = new AuthenticationsAndAuthorizationsOperations();

        public Boolean IsAccessible(String userName, String featureName) {
            return UserValidation(GetUserIDByUserName(userName), GetFeatureIDByFeatureName(featureName));
        }

        private Boolean UserValidation(Guid userId, Guid featureId) {
            if (aNaOps.IsSystemAdministratorUser(aNaOps.GetUserNameByUserID(userId))) { return true; }
            return aNaOps.IsUserInAccessProfile(userId, featureId);
        }

        private Guid GetUserIDByUserName(String userName) {
            return aNaOps.GetUserIDByUserName(userName);
        }

        private Guid GetFeatureIDByFeatureName(String featureName) {
            return aNaOps.GetFeatureIDByFeatureName(featureName);
        }

        private List<Guid> GetRoleIDByUserName(String userName) {
            List<aspnet_Roles> item = aNaOps.GetRolesByUserName(userName);
            return item.Select(role => role.RoleId).ToList();
        }

    }

    public class AccessControl {
        AccessControlOperations aco = new AccessControlOperations();
        public Boolean AccessAccount(String userName) { return aco.IsAccessible(userName, "Account"); }
        public Boolean AccessAccountFeatureAssignment(String userName) { return aco.IsAccessible(userName, "Account>Feature Assignment"); }
        public Boolean AccessAccountFeatureManagement(String userName) { return aco.IsAccessible(userName, "Account>Feature Assignment>Feature Management"); }
        public Boolean AccessAccountRoles(String userName) { return aco.IsAccessible(userName, "Account>Roles"); }
        public Boolean AccessAccountNewRole(String userName) { return aco.IsAccessible(userName, "Account>Roles>New Role"); }
        public Boolean AccessAccountUsers(String userName) { return aco.IsAccessible(userName, "Account>Users"); }
        public Boolean AccessAccountUserRegister(String userName) { return aco.IsAccessible(userName, "Account>Users>Register"); }

        public Boolean AccessDCE(String userName) { return aco.IsAccessible(userName, "Online DCE"); }
        public Boolean AccessDCEDocClassification(String userName) { return aco.IsAccessible(userName, "Online DCE>Doc Classification"); }
        public Boolean AccessDCENewTemplate(String userName) { return aco.IsAccessible(userName, "Online DCE>New Template"); }

        public Boolean AccessDMS(String userName) { return aco.IsAccessible(userName, "Online DMS"); }
        public Boolean AccessDMSFiles(String userName) { return aco.IsAccessible(userName, "Online DMS>Files"); }
        public Boolean AccessDMSFolderManagement(String userName) { return aco.IsAccessible(userName, "Online DMS>Folder Management"); }

        public Boolean AccessReports(String userName) { return aco.IsAccessible(userName, "Reports"); }
        public Boolean AccessReportsDownloadUserActivity(String userName) { return aco.IsAccessible(userName, "Reports>Download>Activity Report (User)"); }
        public Boolean AccessReportsDonwloadAllFilesDownloaded(String userName) { return aco.IsAccessible(userName, "Reports>Download>Docs Downloaded Report (All)"); }
        public Boolean AccessRepoersDownloadUserFilesDownloaded(String userName) { return aco.IsAccessible(userName, "Reports>Download>Docs Downloaded Report (User)"); }
        public Boolean AccessReportsViewUserActivity(String userName) { return aco.IsAccessible(userName, "Reports>View>Activity Report (User)"); }
        public Boolean AccessReportsViewAllFilesDownloaded(String userName) { return aco.IsAccessible(userName, "Reports>View>Docs Downloaded Report (All)"); }
        public Boolean AccessReportsViewUserFilesDownloaded (String userName) { return aco.IsAccessible(userName, "Reports>View>Docs Downloaded Report (User)"); }
    }

}