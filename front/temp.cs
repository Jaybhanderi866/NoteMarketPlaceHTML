//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NoteMarketPlace.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    public partial class User
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public User()
        {
            this.DownloadedNotes = new HashSet<DownloadedNote>();
            this.DownloadedNotes1 = new HashSet<DownloadedNote>();
            this.Feedbacks = new HashSet<Feedback>();
            this.NoteDetails = new HashSet<NoteDetail>();
            this.Statistics = new HashSet<Statistic>();
            this.SpamReports = new HashSet<SpamReport>();
            this.UserProfileDetails = new HashSet<UserProfileDetail>();
        }
    
        public int UserId { get; set; }
        public int UserRolesID { get; set; }
        [Required]
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public string EmailID { get; set; }

        [Required]
        [Display(Name = "Password")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 4)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The Password and Confirm Password do not match.")]
        public string Password2 { get; set; }

        public bool RememberMe { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsDetailsSubmitted { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public bool IsActive { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DownloadedNote> DownloadedNotes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DownloadedNote> DownloadedNotes1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Feedback> Feedbacks { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NoteDetail> NoteDetails { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Statistic> Statistics { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SpamReport> SpamReports { get; set; }
        public virtual UserRole UserRole { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserProfileDetail> UserProfileDetails { get; set; }
    }
}




//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NoteMarketPlace.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Web;

    public partial class UserProfileDetail
    {
        public int ID { get; set; }
        public int UserID { get; set; }

        [Required]
        [Display(Name = "DOB")]
        public Nullable<System.DateTime> DOB { get; set; }
        public string Gender { get; set; }

        
        public string PhoneNumber { get; set; }
        public string ProfilePicture { get; set; }

        [Required]
        [Display(Name = "Address 1")]
        public string Address1 { get; set; }

        public string Address2 { get; set; }

        [Required]
        [Display(Name = "City")]
        public string City { get; set; }

        [Required]
        [Display(Name = "State")]
        public string State { get; set; }

        [Required]
        [Display(Name = "ZipCode")]
        public string ZipCode { get; set; }

        [Required]
        [Display(Name = "Country")]
        public string Country { get; set; }
        public string University { get; set; }
        
        public string CountryCode { get; set; }

        public string College { get; set; }

        public HttpPostedFileBase File { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public bool IsActive { get; set; }
        public string CountryCode { get; set; }
    
        public virtual User User { get; set; }
    }
}

var errors = ModelState.Where(x => x.Value.Errors.Count > 0).Select(x => new { x.Key, x.Value.Errors }).ToArray();





 if (userdetail.File != null)
                    {
                        var AllowedExtentions = new[] { ".Jpg", ".png", ".jpg", "jpeg" };
                        string extension = Path.GetExtension(userdetail.File.FileName);
                        if (AllowedExtentions.Contains(extension))
                        {

                            if (userdetail.File.ContentLength < 10 * 1024 * 1024)
                            {
                                string fileName = userId.ToString() + v_user.FirstName + extension;
                                string path = Path.Combine(Server.MapPath("~/Uploads/ProfilePicture/"), fileName);
                                userdetail.File.SaveAs(path);
                                userdetail.ProfilePicture = fileName;
                            }
                            else
                            {
                                ViewBag.fileMsg = "file size is morethan 10 MB.";
                                return View(userdetail);
                            }
                        }
                        else
                        {
                            ViewBag.fileMsg = "Please choose only Image file";
                            return View(userdetail);
                        }
                    }