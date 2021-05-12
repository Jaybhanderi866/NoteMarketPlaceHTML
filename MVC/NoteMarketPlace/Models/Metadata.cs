namespace NoteMarketPlace.Models
{
    using System;
    using System.Web;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    public class UserMetadata
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public UserMetadata()
        {
            this.BuyerRequests = new HashSet<BuyerRequest>();
            this.BuyerRequests1 = new HashSet<BuyerRequest>();
            this.Feedbacks = new HashSet<Feedback>();
            this.NoteDetails = new HashSet<NoteDetail>();
           
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
        public virtual ICollection<BuyerRequest> BuyerRequests { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BuyerRequest> BuyerRequests1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

        public virtual ICollection<Feedback> Feedbacks { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NoteDetail> NoteDetails { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        
        public virtual ICollection<SpamReport> SpamReports { get; set; }
        public virtual UserRole UserRole { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserProfileDetail> UserProfileDetails { get; set; }
    }

    public class UserProfileDetailMetadata
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public Nullable<System.DateTime> DOB { get; set; }
        public string Gender { get; set; }
        [Required]
        [Display(Name = "Phone Number")]
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
        public string CountryCode { get; set; }
        public string University { get; set; }
        public string College { get; set; }
        public HttpPostedFileBase File { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public bool IsActive { get; set; }

        public virtual User User { get; set; }
    }

    public class UserRoleMetadata
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public UserRoleMetadata()
        {
            this.Users = new HashSet<User>();
        }

        public int UserRolesID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public bool IsActive { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<User> Users { get; set; }
    }

    public class NoteDetailMetadata
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public NoteDetailMetadata()
        {
            this.BuyerRequests = new HashSet<BuyerRequest>();
            this.Feedbacks = new HashSet<Feedback>();
            this.NoteAttachements = new HashSet<NoteAttachement>();
            this.SpamReports = new HashSet<SpamReport>();
        }

        public int NoteID { get; set; }
        public int UserID { get; set; }
        public Nullable<int> ActionedBy { get; set; }
        public int NoteStatus { get; set; }
        [Required]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int Category { get; set; }
        public string BookPicture { get; set; }
        public Nullable<int> NoteType { get; set; }
        public Nullable<int> NumberOfPages { get; set; }

        [Required]
        [Display(Name = "Note Description")]
        public string NotesDescription { get; set; }
        public string InstitutionName { get; set; }
        public Nullable<int> Country { get; set; }
        public string Course { get; set; }
        public string CourseCode { get; set; }
        public string Professor { get; set; }

        [Required]
        [Display(Name = "Sell For")]
        public bool IsPaid { get; set; }

        public Nullable<int> SellPrice { get; set; }
        public string NotePreview { get; set; }
        public Nullable<int> NoteSize { get; set; }
        public Nullable<System.DateTime> PublishedDate { get; set; }
        public string Remark { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public bool IsActive { get; set; }

        public HttpPostedFileBase PictureFile { get; set; }
        public HttpPostedFileBase NoteFile { get; set; }
        public HttpPostedFileBase PreviewFile { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BuyerRequest> BuyerRequests { get; set; }
        public virtual Country Country1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

        public virtual ICollection<Feedback> Feedbacks { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NoteAttachement> NoteAttachements { get; set; }
        public virtual NoteCategory NoteCategory { get; set; }
        public virtual NoteStatu NoteStatu { get; set; }
        public virtual NoteType NoteType1 { get; set; }
        public virtual User User { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SpamReport> SpamReports { get; set; }
    }

    public class FeedbackMetadata
    {
        public int ID { get; set; }
        public int NoteID { get; set; }
        public int UserID { get; set; }

        [Required]
        [Display(Name ="Ratings")]
        public int Ratings { get; set; }
        public string Comments { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public bool IsActive { get; set; }

        public virtual NoteDetail NoteDetail { get; set; }
        public virtual User User { get; set; }
    }

    public class ContactUMetadata
    {
        public int ID { get; set; }

        [Required]
        [Display(Name = "First name")]
        public string FullName { get; set; }

        [Required]
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }

        [Required]
        [Display(Name = "Subject")]
        public string Subject { get; set; }

        [Required]
        [Display(Name = "Comments")]
        public string Comments { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public bool IsActive { get; set; }
    }
}
