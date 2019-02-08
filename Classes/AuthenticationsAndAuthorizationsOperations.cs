using RMA_Docker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace RMA_Docker.Classes {
    public class AuthenticationsAndAuthorizationsOperations {

        public void InsertExpiryDate(string userName, DateTime expiryDate) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                aspnet_Users user = (from p in dockerEntities.aspnet_Users
                                     where p.UserName == userName
                                     select p).First();
                if (user.aspnet_Profile == null) {
                    aspnet_Profile profile = new aspnet_Profile();
                    profile.UserId = user.UserId;
                    profile.LastUpdatedDate = DateTime.Now;
                    profile.PropertyNames = "ExpiryDate";
                    profile.PropertyValuesString = expiryDate.ToString();
                    Byte[] fg = new Byte[2];
                    profile.PropertyValuesBinary = fg;
                    dockerEntities.aspnet_Profile.Add(profile);
                } else { user.aspnet_Profile.PropertyValuesString = expiryDate.ToString(); }
                dockerEntities.SaveChanges();
            }
        }

        public DateTime GetExpiryDate(string userName) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                aspnet_Users user = (from p in dockerEntities.aspnet_Users
                                     where p.UserName == userName
                                     select p).First();
                if (user.aspnet_Profile != null) { return Convert.ToDateTime(user.aspnet_Profile.PropertyValuesString); }
                else { return DateTime.MaxValue; }
            }
        }

        public bool InsertRoleOwnership(String userName, String roleName) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                aspnet_Users user = (from p in dockerEntities.aspnet_Users
                                     where p.UserName == userName select p).First();
                RoleProfile roleProfile = new RoleProfile();
                roleProfile.UserId = user.UserId;
                roleProfile.RoleId = GetRoleIDByRoleName(roleName);
                roleProfile.PropertyNames = "RoleOwner";
                roleProfile.PropertyValuesString = roleName;
                roleProfile.LastUpdateDate = DateTime.Now;
                if (!(user.RoleProfiles).Select(role => role.PropertyValuesString.ToLower() == roleName.ToLower()).FirstOrDefault()) {
                    user.RoleProfiles.Add(roleProfile);
                    dockerEntities.SaveChanges();
                    return true;
                }
                return false;
            }
        }

        public bool RemoveRoleOwnership(String userName, String roleName) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                aspnet_Users user = (from p in dockerEntities.aspnet_Users
                                     where p.UserName == userName select p).First();
                RoleProfile roleProfile = (user.RoleProfiles).Where(role=>role.PropertyValuesString.ToLower() == roleName.ToLower()).FirstOrDefault();
                if (roleProfile != null) {
                    user.RoleProfiles.Remove(roleProfile);
                    dockerEntities.SaveChanges();
                    return true;
                }
                return false;
            }
        }

        public bool IsRoleOwner(String userName, String roleName) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                aspnet_Users user = (from p in dockerEntities.aspnet_Users
                                     where p.UserName == userName select p).First();
                bool IsUserARoleOwner = (user.RoleProfiles).Select(role => role.PropertyValuesString.ToLower() == roleName.ToLower()).FirstOrDefault();
                if (IsUserARoleOwner) { return true; }
                return false;
            }
        }

        public void InsertLockMembershipUserByUserName(string userName) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                aspnet_Users user = (from p in dockerEntities.aspnet_Users
                                     where p.UserName == userName
                                     select p).First();
                user.aspnet_Membership.IsLockedOut = true;
                user.aspnet_Membership.LastLockoutDate = DateTime.Now;
                dockerEntities.SaveChanges();
            }
        }

        public void InsertMobileAliasByUserName(String userName, String mobileAlias) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                aspnet_Users user = (from p in dockerEntities.aspnet_Users
                                     where p.UserName == userName
                                     select p).First();
                if (user != null) { user.MobileAlias = mobileAlias; }
                dockerEntities.SaveChanges();
            }
        }

        public void InsertMobilePINByUserName(String userName, String mobilePin) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                aspnet_Users user = (from p in dockerEntities.aspnet_Users
                                     where p.UserName == userName
                                     select p).First();
                if (user.aspnet_Membership != null) { user.aspnet_Membership.MobilePIN = mobilePin; }
                dockerEntities.SaveChanges();
            }
        }

        public void InsertRoleDescByRoleName(String roleName, String roleDesc) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                aspnet_Roles role = (from p in dockerEntities.aspnet_Roles
                                     where p.RoleName == roleName
                                     select p).First();
                if (role != null) { role.Description = roleDesc; }
                dockerEntities.SaveChanges();
            }
        }

        public void InsertMembershipCommentByUserName(String userName, String Comment) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                aspnet_Users user = (from p in dockerEntities.aspnet_Users
                                     where p.UserName == userName
                                     select p).First();
                if (user != null) { user.aspnet_Membership.Comment = Comment; }
                dockerEntities.SaveChanges();
            }
        }

        public String getMobilPINByUserName(String userName) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                aspnet_Users user = (from p in dockerEntities.aspnet_Users
                                     where p.UserName == userName
                                     select p).First();
                if (user.aspnet_Membership != null) { return user.aspnet_Membership.MobilePIN; }
                else { return ""; }
            }
        }

        public String GetRoleDescByRoleName(String roleName) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                var query = (from p in dockerEntities.aspnet_Roles
                             where p.RoleName == roleName
                             select p).First();
                return query.Description;
            }
        }

        public bool UpdateRole(RoleViewModel roleViewModel, String oldRoleName) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                var role = (from p in dockerEntities.aspnet_Roles
                            where p.RoleName == oldRoleName
                            select p).First();
                if (role.RoleName == roleViewModel.OldRoleName) {
                    role.RoleName = roleViewModel.NewRoleName;
                    role.LoweredRoleName = roleViewModel.NewRoleName.ToLower();
                    role.Description = roleViewModel.Description;
                    dockerEntities.SaveChanges();
                    return true;
                }
            }
            return false;
        }

        public bool IsSystemAdministratorUser(String userName) {
            String[] inSystemAdminRoleUsers = Roles.GetUsersInRole("System Administrator");
            if (inSystemAdminRoleUsers.Contains(userName)) { return true; }
            return false;
        }

        public bool IsDMSAdministratorUser(String userName) {
            String[] inRoleUsers = Roles.GetUsersInRole("DMS Administrator");
            if (inRoleUsers.Contains(userName)) { return true; }
            return false;
        }

        public bool IsDCEAdministratorUser(String userName) {
            String[] inRoleUsers = Roles.GetUsersInRole("DCE Administrator");
            if (inRoleUsers.Contains(userName)) { return true; }
            return false;
        }

        public String GetUserNameByUserID(Guid userID) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                String userName = (from p in dockerEntities.aspnet_Users
                                   where p.UserId == userID
                                   select p.UserName).First();
                return userName;
            }
        }

        public Guid GetUserIDByUserName(String userName) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                var query = (from p in dockerEntities.aspnet_Users
                             where p.UserName == userName select p).First();
                return query.UserId;
            }
        }

        public Guid GetRoleIDByRoleName(String roleName) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                var roleID = (from p in dockerEntities.aspnet_Roles
                             where p.RoleName == roleName select p.RoleId).First();
                return roleID;
            }
        }

        public List<aspnet_Roles> GetRolesByUserName(String userName) {
            using (DockerDBEntities dockerDBEntities = new DockerDBEntities()) {
                var query = (from p in dockerDBEntities.aspnet_Users
                             where p.UserName == userName select p).First();
                return query.aspnet_Roles.ToList();
            }
        }

        public String GetRoleNameByRoleID(Guid roleID) {
            using (DockerDBEntities dockerDBEntities = new DockerDBEntities()) {
                var roleName = (from p in dockerDBEntities.aspnet_Roles
                             where p.RoleId == roleID
                             select p.RoleName).First();
                return roleName;
            }
        }

        public String GetMobileAliasByUserName(String userName) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                var query = (from p in dockerEntities.aspnet_Users
                             where p.UserName == userName
                             select p).First();
                return query.MobileAlias;
            }
        }

        public bool GetIsAnonymousByUserName(String userName) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                var query = (from p in dockerEntities.aspnet_Users
                             where p.UserName == userName
                             select p).First();
                return query.IsAnonymous;
            }
        }

        public List<UserLoginAuditTrail> GetUserActivityAuditTrailsBySpecificUser(Guid userID) {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                dockerEntities.Configuration.LazyLoadingEnabled = false;
                List<UserLoginAuditTrail> query = (from p in dockerEntities.UserLoginAuditTrails
                                                   where p.UserID == userID
                                                   select p).OrderByDescending(i=>i.DateTimeLogged).ToList<UserLoginAuditTrail>();
                return query;
            }
        }

        public int GetTotalUsersCount() {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                int count = (from p in dockerEntities.aspnet_Users
                             select p).Count();
                return count;
            }
        }

        public int GetTotalRolesCount() {
            using (DockerDBEntities dockerEntities = new DockerDBEntities()) {
                int count = (from p in dockerEntities.aspnet_Roles
                             select p).Count();
                return count;
            }
        }

        #region Feature Assignment 
        public List<FeatureProfile> GetALLRegisteredFeatures() {
            using (DockerDBEntities dockerDBEntities = new DockerDBEntities()) {
                List<FeatureProfile> fpList = (from p in dockerDBEntities.FeatureProfiles
                                                orderby p.FeatureName select p).ToList();
                return fpList;
            }
        }

        public Boolean InsertNewFeatureProfile(String featureName, String remarks) {
            using (DockerDBEntities dockerDBEntities = new DockerDBEntities()) {
                FeatureProfile fp = (from p in dockerDBEntities.FeatureProfiles
                                     where p.FeatureName == featureName
                                     select p).FirstOrDefault();
                if (fp == null) {
                    fp = new FeatureProfile();
                    fp.FeatureId = Guid.NewGuid();
                    fp.FeatureName = featureName;
                    fp.FeatureRemarks = remarks;
                    fp.LastUpdateDate = DateTime.Now;
                    dockerDBEntities.FeatureProfiles.Add(fp);
                    dockerDBEntities.SaveChanges();
                    return true;
                }
                return false;
            }
        }

        public Boolean UpdateFeatureProfile(String oldFeatureName, String newFeatureName, String remarks) {
            using (DockerDBEntities dockerDBEntities = new DockerDBEntities()) {
                FeatureProfile fp = (from p in dockerDBEntities.FeatureProfiles
                                     where p.FeatureName == oldFeatureName
                                     select p).FirstOrDefault();
                if (fp != null) {
                    fp.FeatureName = newFeatureName;
                    fp.FeatureRemarks = remarks;
                    fp.LastUpdateDate = DateTime.Now;
                    dockerDBEntities.SaveChanges();
                    return true;   
                }
            }
            return false;
        }

        public Boolean DeleteFeatureProfile(String featureName) {
            using (DockerDBEntities dockerDBEntities = new DockerDBEntities()) {
                FeatureProfile fp = (from p in dockerDBEntities.FeatureProfiles
                                     where p.FeatureName == featureName
                                     select p).FirstOrDefault();
                if (fp != null) {
                    List<FeatureAccessProfile> accessProfilesList = (from p in dockerDBEntities.FeatureAccessProfiles
                                                                     where p.FeatureProfile.FeatureName == featureName select p).ToList();
                    foreach (var item in accessProfilesList) { dockerDBEntities.FeatureAccessProfiles.Remove(item); }
                    dockerDBEntities.FeatureProfiles.Remove(fp);
                    dockerDBEntities.SaveChanges();
                    return true;
                }
            }
            return false;
        }

        public List<FeatureAccessProfileModel> GetAllFeatureAccessProfileModel() {
            using (DockerDBEntities dockerDBEntities = new DockerDBEntities()) {
                List<FeatureAccessProfile> fap = (from p in dockerDBEntities.FeatureAccessProfiles select p).ToList();
                List<FeatureAccessProfileModel> fapmList = new List<FeatureAccessProfileModel>();
                foreach (var item in fap) {
                    FeatureAccessProfileModel fapm = new FeatureAccessProfileModel();
                    if (item.FeatureProfile != null) { fapm.FeatureName = item.FeatureProfile.FeatureName; } else { fapm.FeatureName = ""; }
                    if (item.aspnet_Users != null) { fapm.UserName = item.aspnet_Users.UserName; } else { fapm.UserName = ""; }
                    if (item.aspnet_Roles != null) { fapm.RoleName = item.aspnet_Roles.RoleName; } else { fapm.RoleName = ""; }
                    fapm.LastUpdateDate = item.LastUpdateDate.ToString("dd/MM/yyyy HH:mm");
                    fapmList.Add(fapm);
                }
                return fapmList;
            }
        }

        public List<FeatureAccessProfile> GetFeatureAccessProfiles() {
            using (DockerDBEntities dockerDBEntities = new DockerDBEntities()) {
                List<FeatureAccessProfile> fap = (from p in dockerDBEntities.FeatureAccessProfiles select p).ToList();
                return fap;
            }
        }

        public Boolean InsertFeatureAccessProfile(String featureName, String userName, String roleName) {
            using (DockerDBEntities dockerDBEntities = new DockerDBEntities()) {
                FeatureAccessProfile fap = (from p in dockerDBEntities.FeatureAccessProfiles
                                            where p.FeatureProfile.FeatureName == featureName
                                            && (p.aspnet_Users.UserName == userName || p.aspnet_Roles.RoleName == roleName)
                                            select p).FirstOrDefault();
                if (fap == null) {
                    fap = new FeatureAccessProfile();
                    fap.FeatureAccessProfileId = Guid.NewGuid();
                    fap.LastUpdateDate = DateTime.Now;
                    fap.FeatureProfile = (from p in dockerDBEntities.FeatureProfiles
                                          where p.FeatureName == featureName select p).First();
                    if (!String.IsNullOrEmpty(userName)) {
                        fap.aspnet_Users = (from p in dockerDBEntities.aspnet_Users
                                            where p.UserName == userName select p).First();
                        fap.UserId = fap.aspnet_Users.UserId;
                    }
                    if (!String.IsNullOrEmpty(roleName)) {
                        fap.aspnet_Roles = (from p in dockerDBEntities.aspnet_Roles
                                            where p.RoleName == roleName select p).First();
                        fap.RoleId = fap.aspnet_Roles.RoleId;
                    }
                    fap.FeatureId = fap.FeatureProfile.FeatureId;
                    dockerDBEntities.FeatureAccessProfiles.Add(fap);
                    dockerDBEntities.SaveChanges();
                    return true;   
                }
            }
            return false;
        }

        public Boolean DeleteFeatureAccessProfile(String featureName, String userName, String roleName) {
            using (DockerDBEntities dockerDBEntities = new DockerDBEntities()) {
                FeatureAccessProfile fap = (from p in dockerDBEntities.FeatureAccessProfiles
                                            where p.FeatureProfile.FeatureName == featureName
                                            && (p.aspnet_Users.UserName == userName || p.aspnet_Roles.RoleName == roleName)
                                            select p).First();
                dockerDBEntities.FeatureAccessProfiles.Remove(fap);
                dockerDBEntities.SaveChanges();
            }
            return false;
        }

        public Boolean IsUserInAccessProfile(Guid userId, Guid featureId) {
            using (DockerDBEntities dockerDBEntities = new DockerDBEntities()) {
                aspnet_Users user = (from p in dockerDBEntities.aspnet_Users
                                    where p.UserId == userId select p).First();
                if (user.FeatureAccessProfiles != null) {
                    foreach (FeatureAccessProfile ap in user.FeatureAccessProfiles) {
                        if (ap.FeatureId == featureId) { return true; }
                    }
                }
                if (user.aspnet_Roles != null) {
                    foreach (aspnet_Roles ar in user.aspnet_Roles) {
                        foreach (FeatureAccessProfile ap in ar.FeatureAccessProfiles) {
                            if (ap.FeatureId == featureId) { return true; }
                        }
                    }
                }
            }
            return false;
        }

        public Guid GetFeatureIDByFeatureName(String featureName) {
            using (DockerDBEntities dockerDBEntities = new DockerDBEntities()) {
                Guid featureID = (from p in dockerDBEntities.FeatureProfiles
                                  where p.FeatureName == featureName
                                  select p.FeatureId).FirstOrDefault();
                return featureID;
            }
        }
        #endregion


    }
}