using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RMA_Docker.Models
{
    public class AccountModels {}

    public class ChangePasswordModel {
        public String UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public String OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public String NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public String ConfirmPassword { get; set; }
    }

    public class LogOnModel {
        [Required]
        [Display(Name = "User Name")]
        public String UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public String Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterModel {
        [Required]
        [Display(Name = "User Name")]
        public String UserName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email Address")]
        public String Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public String Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public String ConfirmPassword { get; set; }
    }

    public class RoleViewModel {
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Role Name")]
        [StringLength(250, ErrorMessage = "The {0} must be at least {2} ~ 250 characters long.", MinimumLength = 3)]
        public String NewRoleName { get; set; }
        public String OldRoleName { get; set; }
        public String Description { get; set; }
    }

    public class ListUsersViewModel {
        public String UserName { get; set; }
        public String MobileAlias { get; set; }
        public String Email { get; set; }
        public String LastActivityDate { get; set; }
        public String ExpiredDate { get; set; }
        public bool ThisRoleOwner { get; set; } = false;
    }

    public class UserEditModel {
        [Required]
        [Display(Name = "User Name")]
        public String UserName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email Address")]
        public String Email { get; set; }

        /* Gets or sets application-specific information for the membership user. */
        public String Comment { get; set; }

        [Display(Name = "In Role(s)")]
        public String[] GetRolesForUser { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = true)]
        [Display(Name = "Creation Date")]
        public DateTime CreationDate { get; set; }

        /* Gets or sets whether the membership user can be authenticated. */
        [Display(Name = "Approved")]
        public bool IsApproved { get; set; }

        [StringLength(16, ErrorMessage = "The {0} must be at least {2} ~ 16 characters long.", MinimumLength = 5)]
        [Display(Name = "Mobile Alias")]
        public String PhoneAlias { get; set; }

        [Display(Name = "Mobile No.")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\(?([0-9]{8})$", ErrorMessage = "Not a valid Singapore phone number")]
        public String PhonePIN { get; set; }

        /* Gets or sets the date and time when the user was last authenticated. */
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = true)]
        [Display(Name = "Last Login Date")]
        public DateTime LastLoginDate { get; set; }

        [Display(Name = "Locked Out")]
        public bool IsLockedOut { get; set; }

        /* Gets the most recent date and time that the membership user was locked out or expired */
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = true)]
        [Display(Name = "Lock Out Date")]
        public DateTime LastLockoutDate { get; set; }

        /* Gets the date and time when the membership user's password was last updated. */
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = true)]
        [Display(Name = "Last Password Changed Date")]
        public DateTime LastPasswordChangedDate { get; set; }
        public String Password { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = true)]
        [DataType(DataType.DateTime)]
        [Display(Name = "Expired Date")]
        public DateTime ExpiredDate { get; set; }
    }

    #region Feature Assignment
    public class FeatureAssignmentModel {
        public List<System.Web.Mvc.SelectListItem> DropdownUser { get; set; }
        public List<System.Web.Mvc.SelectListItem> DropdownRole { get; set; }
        public List<System.Web.Mvc.SelectListItem> DropdownFeature { get; set; }
        public List<FeatureAccessProfileModel> accessProfile { get; set; }
    }

    public class FeatureAccessProfileModel {
        public String UserName { get; set; }
        public String RoleName { get; set; }
        public String FeatureName { get; set; }
        public String LastUpdateDate { get; set; }
    }

    public class FeatureProfileModel {
        public String FeatureName { get; set; }
        public String OldFeatureName { get; set; } 
        public String Remarks { get; set; }
        public List<FeatureProfile> FeatureProfileList { get; set; }
    }
    #endregion

}