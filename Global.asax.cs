using RMA_Docker.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace RMA_Docker {
    public class MvcApplication : System.Web.HttpApplication {

        String systemCreatedAutomation = "Created By System Automation";

        protected void Application_Start() {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            CheckAdminUserAndRolesExists();

            
        }

        //System must check for admin user and Admin role
        private void CheckAdminUserAndRolesExists() {
            String sysAdmin = "System Administrator";
            //String sysUser = "User";
            String sysDCEAdmin = "DCE Administrator";
            String sysDMSAdmin = "DMS Administrator";
            AuthenticationsAndAuthorizationsOperations aNaOps = new AuthenticationsAndAuthorizationsOperations();
            if (!Roles.RoleExists(sysAdmin)) {
                Roles.CreateRole(sysAdmin);
                aNaOps.InsertRoleDescByRoleName(sysAdmin, systemCreatedAutomation);
            }
            //if (!Roles.RoleExists(sysUser)) {
            //    Roles.CreateRole(sysUser);
            //    aNaOps.InsertRoleDescByRoleName(sysUser, systemCreatedAutomation);
            //}
            if (!Roles.RoleExists(sysDCEAdmin)) {
                Roles.CreateRole(sysDCEAdmin);
                aNaOps.InsertRoleDescByRoleName(sysDCEAdmin, systemCreatedAutomation);
            }
            if (!Roles.RoleExists(sysDMSAdmin)) {
                Roles.CreateRole(sysDMSAdmin);
                aNaOps.InsertRoleDescByRoleName(sysDMSAdmin, systemCreatedAutomation);
            }

            String adminPassword = "admin1234";
            MembershipCreateStatus createStatus;
            Membership.CreateUser("sysadmin", adminPassword, "sysadmin@docker.com", null, null, true, null, out createStatus);
            if (createStatus == MembershipCreateStatus.Success) {
                aNaOps.InsertMembershipCommentByUserName("sysadmin", systemCreatedAutomation);
            }
            if (!Roles.IsUserInRole("sysadmin", sysAdmin)) { Roles.AddUserToRole("sysadmin", sysAdmin); }
            if (!Roles.IsUserInRole("sysadmin", sysDCEAdmin)) { Roles.AddUserToRole("sysadmin", sysDCEAdmin); }
            if (!Roles.IsUserInRole("sysadmin", sysDMSAdmin)) { Roles.AddUserToRole("sysadmin", sysDMSAdmin); }

            //Membership.CreateUser("sysuser", adminPassword, "sysuser@docker.com", null, null, true, null, out createStatus);
            //if (createStatus == MembershipCreateStatus.Success) {
            //    aNaOps.InsertMembershipCommentByUserName("sysuser", systemCreatedAutomation);
            //}
            //if (!Roles.IsUserInRole("sysuser", sysUser)) { Roles.AddUserToRole("sysuser", sysUser); }

        }

        //System should pre-create those folder ~/DockerFiles

        //System should pre-create the OCR folders ~/DockerFiles/DCEDocker/OCR and eng.traineddata

        //System should pre-create the folder ~/DockerFiles/DictionarySources and en-US.dic

    }
}
