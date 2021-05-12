namespace NoteMarketPlace.Models
{
    using System;
    using System.Web;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    public partial class UserProfileDetail
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
}