using System;
using System.Web;

namespace NoteMarketPlace.Models
{
    public class Members 
    {
        public int MemberID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailID { get; set; }
        public string Password { get; set; }
        public Nullable<System.DateTime> JoinDate { get; set; }
        public int TotalEarnings { get; set; }
        public int TotalExpenses { get; set; }
        public int UnderReviewNotes { get; set; }
        public int PublishedNotes { get; set; }
        public int RejectedNotes { get; set; }
        public int DownloadNotes { get; set; }

    }
}
