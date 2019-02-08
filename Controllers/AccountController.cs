using RMA_Docker.Classes;
using RMA_Docker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace RMA_Docker.Controllers {

    public class AccountController : Controller {
        String dateTimeFormat = "dd/MM/yyyy HH:mm";

        [AllowAnonymous]
        public ActionResult Logon(string returnUrl) {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public ActionResult Logon(LogOnModel logonModel, string returnUrl) {
            if (ModelState.IsValid) {
                if (Membership.ValidateUser(logonModel.UserName, logonModel.Password)) {
                    FormsAuthentication.SetAuthCookie(logonModel.UserName, logonModel.RememberMe);
                    RecordUserLogin(logonModel.UserName);
                    return RedirectToAction("Index", "Home");
                } else {
                    ModelState.AddModelError("", "The user name or password provided is incorrect!");
                }
            }
            return View(logonModel);
        }

        public ActionResult Logoff(String userName) {
            FormsAuthentication.SignOut();
            //FormsAuthentication.RedirectFromLoginPage(userName, false);
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Register() {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel registerModel) {
            if (ModelState.IsValid) {
                MembershipCreateStatus createStatus;
                Membership.CreateUser(registerModel.UserName, registerModel.Password, registerModel.Email, null, null, true, null, out createStatus);
                if (createStatus == MembershipCreateStatus.Success) {
                    DocumentsOperations docOps = new DocumentsOperations();
                    docOps.InsertNewUserFolder(Server, registerModel.UserName);
                    TempData["RegisterSuccess"] = "Created user successful!";
                } else { ModelState.AddModelError("", ErrorCodeToString(createStatus)); }
            }
            return View(registerModel);
        }

        public ActionResult ChangePassword(String userName, String action) {
            ChangePasswordModel changePasswordModel = new ChangePasswordModel();
            changePasswordModel.UserName = userName;
            return View(changePasswordModel);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordModel changePasswordModel) {
            if (ModelState.IsValid) {
                bool changePasswordStatus = false;
                try {
                    MembershipUser user = Membership.GetUser(User.Identity.Name, true);
                    changePasswordStatus = user.ChangePassword(changePasswordModel.OldPassword, changePasswordModel.NewPassword);
                } catch (Exception ex) {
                    //System.Diagnostics.Debug.WriteLine("=" + User.Identity.Name + "ChangePassword:" + ex);
                }
                if (changePasswordStatus) {
                    //return RedirectToAction("ChangePasswordSuccess");
                    TempData["Success"] = "Your password has been changed successfully.";
                } else {
                    ModelState.AddModelError("", "The Current Passsword is incorrect or the new password is invalid!");
                }
            }
            return View(changePasswordModel);
        }

        public ActionResult ResetPassword(String userName) {
            MembershipUser user = Membership.GetUser(userName, true);
            TempData["ResettedPassword"] = user.ResetPassword();
            return RedirectToAction("EditUser", new { userName = userName });
        }

        [Authorize]
        public ActionResult ListUsers() {
            return View();
        }

        [Authorize]
        [HttpPost]
        public JsonResult ListUsers(String userName) {
            MembershipUserCollection users = Membership.GetAllUsers();
            List<ListUsersViewModel> listRoles = new List<ListUsersViewModel>();
            AuthenticationsAndAuthorizationsOperations aNaOps = new AuthenticationsAndAuthorizationsOperations();
            DocumentsOperations documentsOperations = new DocumentsOperations();
            foreach (MembershipUser user in users) {
                listRoles.Add(new ListUsersViewModel {
                    UserName = user.UserName,
                    MobileAlias = aNaOps.GetMobileAliasByUserName(user.UserName),
                    Email = user.Email,
                    LastActivityDate = (user.LastActivityDate).ToString(dateTimeFormat),
                    ExpiredDate = (aNaOps.GetExpiryDate(user.UserName)).ToString(dateTimeFormat)
                });
            }
            return Json(new { Total = listRoles.Count, Data = listRoles });
        }

        [Authorize]
        public ActionResult DeleteUser_Controller(String userName) {
            DocumentsOperations docOps = new DocumentsOperations();
            AuthenticationsAndAuthorizationsOperations aNaOps = new AuthenticationsAndAuthorizationsOperations();
            docOps.RemoveUsersFilesAuthorizationsByUserName(aNaOps.GetUserIDByUserName(userName));
            bool delUserStatus = Membership.DeleteUser(userName, true);
            return RedirectToAction("ListUsers");
        }

        [Authorize]
        public ActionResult EditUser(String userName) {
            MembershipUser user = Membership.GetUser(userName, true);
            UserEditModel userEditModel = new UserEditModel();
            userEditModel.UserName = user.UserName;
            userEditModel.LastPasswordChangedDate = user.LastPasswordChangedDate;
            userEditModel.LastLoginDate = user.LastLoginDate;
            userEditModel.IsLockedOut = user.IsLockedOut;
            userEditModel.LastLockoutDate = user.LastLockoutDate;
            userEditModel.IsApproved = user.IsApproved;
            userEditModel.Email = user.Email;
            userEditModel.CreationDate = user.CreationDate;
            userEditModel.GetRolesForUser = Roles.GetRolesForUser(userName);
            userEditModel.Comment = user.Comment;
            
            AuthenticationsAndAuthorizationsOperations anaOPs = new AuthenticationsAndAuthorizationsOperations();
            userEditModel.ExpiredDate = anaOPs.GetExpiryDate(userName);
            userEditModel.PhonePIN = anaOPs.getMobilPINByUserName(userName);

            DocumentsOperations documentsOperations = new DocumentsOperations();
            userEditModel.PhoneAlias = anaOPs.GetMobileAliasByUserName(userName);
            return View(userEditModel);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUser(UserEditModel user) {
            if (ModelState.IsValid) {
                MembershipUser msuser = Membership.GetUser(user.UserName);
                AuthenticationsAndAuthorizationsOperations authenNAuthorOps = new AuthenticationsAndAuthorizationsOperations();
                if (user.IsLockedOut) { authenNAuthorOps.InsertLockMembershipUserByUserName(user.UserName); } 
                else { msuser.UnlockUser(); }
                msuser.IsApproved = user.IsApproved;
                msuser.Email = user.Email;
                msuser.Comment = user.Comment;
                authenNAuthorOps.InsertExpiryDate(user.UserName, Convert.ToDateTime(user.ExpiredDate));
                authenNAuthorOps.InsertMobileAliasByUserName(user.UserName, user.PhoneAlias);
                authenNAuthorOps.InsertMobilePINByUserName(user.UserName, user.PhonePIN);
                Membership.UpdateUser(msuser);
                TempData["EditUserUpdateSuccess"] = "Update User (" + user.UserName + ") successfully.";
            }
            return View(user);
        }

        [Authorize]
        public ActionResult ListRoles() {
            return View();
        }

        [Authorize]
        [HttpPost]
        public JsonResult ListRoles(String RoleName) {
            String[] roles = Roles.GetAllRoles();
            AuthenticationsAndAuthorizationsOperations authenNAuthorOps = new AuthenticationsAndAuthorizationsOperations();
            List<RoleViewModel> listRoles = new List<RoleViewModel>();
            foreach (String role in roles) {
                listRoles.Add(new RoleViewModel { 
                                NewRoleName = role,
                                OldRoleName = role, 
                                Description = authenNAuthorOps.GetRoleDescByRoleName(role)});
            }
            return Json(new { Total = listRoles.Count, Data = listRoles });
        }

        [Authorize]
        public ActionResult CreateRole() {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateRole(RoleViewModel roleViewModel) {
            if (ModelState.IsValid) {
                if (!Roles.RoleExists(roleViewModel.NewRoleName)) {
                    Roles.CreateRole(roleViewModel.NewRoleName);
                    AuthenticationsAndAuthorizationsOperations aNaOps = new AuthenticationsAndAuthorizationsOperations();
                    aNaOps.InsertRoleDescByRoleName(roleViewModel.NewRoleName, roleViewModel.Description);
                    return RedirectToAction("ListRoles");
                } else { TempData["RoleExists"] = "The Role Name \""+ roleViewModel.NewRoleName + "\" is exists in the system."; }
            }
            return View(roleViewModel);
        }

        [Authorize]
        public ActionResult DeleteRole(String roleName) {
            String[] usersInRole = Roles.GetUsersInRole(roleName);
            if (usersInRole.Count() > 0) { Roles.RemoveUsersFromRole(usersInRole, roleName); }
            DocumentsOperations docOps = new DocumentsOperations();
            AuthenticationsAndAuthorizationsOperations aNaOps = new AuthenticationsAndAuthorizationsOperations();
            docOps.RemoveRolesFilesAuthorizationsByRoleID(aNaOps.GetRoleIDByRoleName(roleName));
            var delRoleStatus = Roles.DeleteRole(roleName, false);
            return RedirectToAction("ListRoles");
        }

        [Authorize]
        public ActionResult EditRole(String roleName) {
            String[] roles = Roles.GetAllRoles();
            RoleViewModel roleViewModel = new RoleViewModel();
            AuthenticationsAndAuthorizationsOperations anaOps = new AuthenticationsAndAuthorizationsOperations();
            foreach (String role in roles) {
                if (role == roleName) {
                    roleViewModel.OldRoleName = role;
                    roleViewModel.NewRoleName = role;
                    roleViewModel.Description = anaOps.GetRoleDescByRoleName(roleName);
                }
            }
            return View(roleViewModel);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditRole(RoleViewModel roleViewModel) {
            if (ModelState.IsValid) {
                AuthenticationsAndAuthorizationsOperations authenNAuthorOps = new AuthenticationsAndAuthorizationsOperations();
                if (authenNAuthorOps.UpdateRole(roleViewModel, roleViewModel.OldRoleName)) {
                    TempData["EditRole"] = "The Role " + roleViewModel.OldRoleName + " has been updated to " + roleViewModel.NewRoleName + ".";
                    return RedirectToAction("EditRole", new { roleName = roleViewModel.NewRoleName });
                } else {
                    TempData["EditRole"] = "The Role " + roleViewModel.OldRoleName + " fail to update, please info system administrator for assist.";
                }
            }
            return View(roleViewModel);
        }

        [Authorize]
        [HttpPost]
        public JsonResult GetUsersRoleJson(String roleName) {
            AuthenticationsAndAuthorizationsOperations anaOps = new AuthenticationsAndAuthorizationsOperations();
            List<ListUsersViewModel> usersInRoleList = new List<ListUsersViewModel>();
            List<ListUsersViewModel> usersNOTInRoleList = new List<ListUsersViewModel>();
            if (!String.IsNullOrEmpty(roleName) && Roles.RoleExists(roleName)) {
                String[] inRoleUsers = Roles.GetUsersInRole(roleName);
                MembershipUserCollection allUsers = Membership.GetAllUsers();
                foreach (MembershipUser user in allUsers) {
                    ListUsersViewModel listUsersViewModel = new ListUsersViewModel();
                    if (inRoleUsers.Contains(user.UserName)) {
                        listUsersViewModel.Email = user.Email;
                        listUsersViewModel.ExpiredDate = (anaOps.GetExpiryDate(user.UserName)).ToString(dateTimeFormat);
                        listUsersViewModel.LastActivityDate = (user.LastActivityDate).ToString(dateTimeFormat);
                        listUsersViewModel.MobileAlias = anaOps.GetMobileAliasByUserName(user.UserName);
                        listUsersViewModel.UserName = user.UserName;
                        listUsersViewModel.ThisRoleOwner = anaOps.IsRoleOwner(user.UserName, roleName);
                        usersInRoleList.Add(listUsersViewModel);
                    }
                    if (!inRoleUsers.Contains(user.UserName)) {
                        listUsersViewModel.Email = user.Email;
                        listUsersViewModel.ExpiredDate = (anaOps.GetExpiryDate(user.UserName)).ToString(dateTimeFormat);
                        listUsersViewModel.LastActivityDate = (user.LastActivityDate).ToString(dateTimeFormat);
                        listUsersViewModel.MobileAlias = anaOps.GetMobileAliasByUserName(user.UserName);
                        listUsersViewModel.UserName = user.UserName;
                        usersNOTInRoleList.Add(listUsersViewModel);
                    }
                }
            }
            return Json(new { 
                TotalInRole = usersInRoleList.Count, DataInRole = usersInRoleList, 
                TotalNotInRole = usersNOTInRoleList.Count, DataNotInRole = usersNOTInRoleList
            });
        }

        [Authorize]
        public ActionResult AddUserToRole(String userName, String roleName) {
            Roles.AddUserToRole(userName, roleName);
            return RedirectToAction("EditRole", new { roleName = roleName });
        }

        [Authorize]
        public ActionResult RemoveUserFromRole(String userName, String roleName) {
            Roles.RemoveUserFromRole(userName, roleName);
            return RedirectToAction("EditRole", new { roleName = roleName });
        }

        [Authorize]
        public ActionResult AssignOwnerShip(String userName, String roleName) {
            MembershipUser user =  Membership.GetUser(userName);
            AuthenticationsAndAuthorizationsOperations aNaOps = new AuthenticationsAndAuthorizationsOperations();
            aNaOps.InsertRoleOwnership(userName, roleName);
            return RedirectToAction("EditRole", new { roleName = roleName });
        }

        [Authorize]
        public ActionResult UnassignOwnerShip(String userName, String roleName) {
            MembershipUser user = Membership.GetUser(userName);
            AuthenticationsAndAuthorizationsOperations aNaOps = new AuthenticationsAndAuthorizationsOperations();
            aNaOps.RemoveRoleOwnership(userName, roleName);
            return RedirectToAction("EditRole", new { roleName = roleName });
        }

        private void RecordUserLogin(String userName) {
            string userID = System.Web.Security.Membership.GetUser(userName).ProviderUserKey.ToString();
            AuthenticationsAndAuthorizationsOperations aNaOps = new AuthenticationsAndAuthorizationsOperations();
            (new AuditTrailOperations()).InsertUserLoginAuditTrail(new Guid(userID));
        }

        [Authorize]
        public ActionResult FeatureAssignment() {
            FeatureAssignmentModel fam = new FeatureAssignmentModel();
            AuthenticationsAndAuthorizationsOperations aNaOps = new AuthenticationsAndAuthorizationsOperations();
            fam.accessProfile = aNaOps.GetAllFeatureAccessProfileModel();

            List<System.Web.Mvc.SelectListItem> DropdownUser = new List<SelectListItem>();
            foreach (MembershipUser user in Membership.GetAllUsers()) {DropdownUser.Add(new SelectListItem { Text = user.UserName, Value = user.UserName });}
            fam.DropdownUser = DropdownUser;
            List<System.Web.Mvc.SelectListItem> DropdownRole = new List<SelectListItem>();
            foreach (String role in Roles.GetAllRoles()) {DropdownRole.Add(new SelectListItem { Text = role, Value = role });}
            fam.DropdownRole = DropdownRole;
            List<System.Web.Mvc.SelectListItem> DropdownFeature = new List<SelectListItem>();
            foreach (FeatureProfile fp in aNaOps.GetALLRegisteredFeatures()) {DropdownFeature.Add(new SelectListItem { Text = fp.FeatureName, Value = fp.FeatureName });}
            fam.DropdownFeature = DropdownFeature;
            
            return View(fam);
        }

        [Authorize]
        [HttpPost]
        public ActionResult FeatureAssignment(FeatureAssignmentModel fam, String submit, String userSelection, String roleSelection, String featureSelection) {
            AuthenticationsAndAuthorizationsOperations aNaOps = new AuthenticationsAndAuthorizationsOperations();
            switch (submit) {
                case "Create": aNaOps.InsertFeatureAccessProfile(featureSelection, userSelection, roleSelection); break;
                case "Delete": aNaOps.DeleteFeatureAccessProfile(featureSelection, userSelection, roleSelection); break;
                default: break;
            }
            return RedirectToAction("FeatureAssignment");
        }

        [Authorize]
        public ActionResult FeaturesManagement() {
            FeatureProfileModel fpm = new FeatureProfileModel();
            fpm.FeatureName = "";
            fpm.Remarks = "";
            fpm.OldFeatureName = "";
            fpm.FeatureProfileList = ReturnAllFeatureProfile();
            return View(fpm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FeaturesManagement(FeatureProfileModel fpm, String submit) {
            AuthenticationsAndAuthorizationsOperations aNaOps = new AuthenticationsAndAuthorizationsOperations();
            switch (submit) {
                case "Delete":
                    if (!aNaOps.DeleteFeatureProfile(fpm.FeatureName)) {
                        TempData["FeaturesManagementErrorMsg"] = "Error - The feature name not found in the list.";
                    }
                    break;
                case "Create":
                    if (!aNaOps.InsertNewFeatureProfile(fpm.FeatureName, fpm.Remarks)) {
                        TempData["FeaturesManagementErrorMsg"] = "Error - The feature name already exists, please change a name and try again.";
                    }
                    break;
                case "Update":
                    if (!String.IsNullOrEmpty(fpm.OldFeatureName)) {
                        if (!aNaOps.UpdateFeatureProfile(fpm.OldFeatureName, fpm.FeatureName, fpm.Remarks)) {
                            TempData["FeaturesManagementErrorMsg"] = "Error - The feature name not found in the list.";
                        }
                    }
                    break;
                default: break;
            }
            return RedirectToAction("FeaturesManagement");
        }

        private List<FeatureProfile> ReturnAllFeatureProfile() {
            AuthenticationsAndAuthorizationsOperations aNaOps = new AuthenticationsAndAuthorizationsOperations();
            return aNaOps.GetALLRegisteredFeatures();
        }

        #region ErrorCodeToString - MembershipCreateStatus Error list
        private static string ErrorCodeToString(MembershipCreateStatus createStatus) {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for a full list of status codes.
            switch (createStatus) {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion
    }
}