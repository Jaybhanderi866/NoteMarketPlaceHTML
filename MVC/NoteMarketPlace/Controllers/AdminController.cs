using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using NoteMarketPlace.Models;
using PagedList;
using PagedList.Mvc;

namespace NoteMarketPlace.Controllers
{
    public class AdminController : Controller
    {
        static int adminId = 0 ;
        NoteMarketPlaceEntities db = new NoteMarketPlaceEntities();

        public ActionResult Login(string emailID, string Password)
        {
            var admin = db.Users.SingleOrDefault(m => m.EmailID == emailID && m.Password == Password);
            if (admin != null)
            {
                adminId = admin.UserId;
                return RedirectToAction("Dashboard", "Admin");
            }
            else
            {
                return RedirectToAction("Login", "Home",new { adminID = 0});
            }
        }

        public ActionResult ChangePassword()
        {
            if (adminId == 0)
            {
                return RedirectToAction("Login", "Home");
            }
            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(ChangePassword changePassword)
        {
            if (adminId == 0)
            {
                return RedirectToAction("Login", "Home");
            }
            if (!ModelState.IsValid)
            {
                return View();
            }
            User v_user = db.Users.SingleOrDefault(m => m.UserId == adminId && m.Password.Equals(changePassword.Password1));
            try
            {
                if (v_user != null)
                {
                    v_user.Password = changePassword.Password2;
                    v_user.Password2 = changePassword.Password2;
                    v_user.UserId = adminId;
                    v_user.EmailID = v_user.EmailID;
                    v_user.FirstName = v_user.FirstName;
                    v_user.LastName = v_user.LastName;
                    v_user.IsDetailsSubmitted = v_user.IsDetailsSubmitted;
                    v_user.IsEmailVerified = v_user.IsEmailVerified;
                    v_user.ModifiedDate = DateTime.Now;
                    v_user.IsActive = v_user.IsActive;
                    db.SaveChanges();
                    return RedirectToAction("Login", "Home");
                }
                else
                {
                    ModelState.AddModelError("Password1", "Your password not match!");
                    return View();
                }
            }
            catch (Exception)
            {
                return View();
            }

        }

        public ActionResult Logout()
        {
            if (adminId == 0)
            {
                return RedirectToAction("Login", "Admin");
            }
            adminId = 0;
            return RedirectToAction("Login", "Home", new { adminID = 0});
        }

        public ActionResult Dashboard(int? page, int? userId, string search, string sortBy, string month, string NoteID, string Remark)
        {
            if (adminId == 0)
            {
                return RedirectToAction("Login", "Admin");
            }

            var AdminProfile = db.Users.SingleOrDefault(m => m.UserId == adminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.AdminProfile;

            if (!string.IsNullOrEmpty(NoteID) && !string.IsNullOrEmpty(Remark))
            {
                
             
                try
                {
                    NoteDetail noteDetail = db.NoteDetails.SingleOrDefault(m => m.NoteID.ToString().Equals(NoteID) && m.IsActive == true);

                    noteDetail.Remark = Remark;
                    noteDetail.NoteStatus = 6;
                    noteDetail.ModifiedDate = DateTime.Now;
                    noteDetail.ModifiedBy = adminId;
                    noteDetail.IsActive = false;

                    db.SaveChanges();

                    User user = db.Users.SingleOrDefault(m => m.UserId == noteDetail.UserID && m.IsActive == true);
                    MailMessage mail = new MailMessage("marketplacenote@gmail.com", "jaybhanderi866@gmail.com");//,user.EmailID.ToString());               
                    mail.Subject = "Sorry! We need to remove your notes from our portal.";
                    string Body = "Hello " + user.FirstName + " " + user.LastName + ",<br><br>We want to inform you that, your note " + noteDetail.Title + " has been removed from the portal.<br>Please find our remarks as below -<br>" + Remark + "<br><br>Regards,<br>Notes Marketplace";
                    mail.Body = Body;
                    mail.IsBodyHtml = true;

                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.UseDefaultCredentials = false;

                    smtp.Credentials = new System.Net.NetworkCredential("marketplacenote@gmail.com", "Note@123");
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
                catch (Exception)
                {
                    return HttpNotFound();
                }
            }

            List<NoteDetail> noteDetails = new List<NoteDetail>();
            List<NoteDetail> notes = db.NoteDetails.Where(m => m.NoteStatus == 7 && m.IsActive == true).ToList();
            foreach (var item in notes)
            {
                NoteDetail note = new NoteDetail();
                note.NoteID = item.NoteID;
                note.Title = item.Title;
                note.NoteCategory = item.NoteCategory;
                note.notefilename = db.NoteAttachements.SingleOrDefault(m=>m.NoteID == item.NoteID).FileName;
                note.NoteSize = item.NoteSize;
                note.SellPrice = item.SellPrice;
                note.PublishedDate = item.PublishedDate;

                User user = db.Users.SingleOrDefault(m => m.UserId == item.UserID);
                note.Publisher = user.FirstName + " " + user.LastName;

                note.NumberOfDownload = db.BuyerRequests.Where(m => m.NoteID == item.NoteID && m.Status == true).Count();
                noteDetails.Add(note);
            }

            noteDetails = noteDetails.OrderByDescending(m => m.NumberOfDownload).ToList();

            if (!string.IsNullOrEmpty(search))
            {
                noteDetails = noteDetails.Where(m => m.Title.ToLower().StartsWith(search.ToLower()) || m.Category.Equals(search) || m.SellPrice.Equals(search) || m.Publisher.StartsWith(search) || m.PublishedDate.Value.ToString("dd-MM-yyyy,hh:mm").StartsWith(search)).ToList();
            }
            if (!string.IsNullOrEmpty(month))
            {
                if (month == "all")
                {
                    noteDetails = noteDetails.ToList();
                }
                else
                {
                    noteDetails = noteDetails.Where(m => m.PublishedDate.Value.ToString("Y").Equals(month)).ToList();
                }

            }
            else
            {
                //noteDetails = noteDetails.Where(m => m.PublishedDate.Value.ToString("Y").Equals(DateTime.Now.ToString("Y"))).ToList();
            }
            if (!string.IsNullOrEmpty(sortBy))
            {
                if (sortBy == "title")
                {
                    noteDetails = noteDetails.OrderBy(m => m.Title).ToList();
                }
                else if (sortBy == "category")
                {
                    noteDetails = noteDetails.OrderBy(m => m.NoteCategory.Name).ToList();
                }
                else if (sortBy == "size")
                {
                    noteDetails = noteDetails.OrderBy(m => m.NoteSize).ToList();
                }
                else if (sortBy == "price")
                {
                    noteDetails = noteDetails.OrderBy(m => m.SellPrice).ToList();
                }
                else if (sortBy == "publisher")
                {
                    noteDetails = noteDetails.OrderBy(m => m.Publisher).ToList();
                }
                else if (sortBy == "date")
                {
                    noteDetails = noteDetails.OrderBy(m => m.PublishedDate).ToList();
                }
                else if (sortBy == "noOfdownload")
                {
                    noteDetails = noteDetails.OrderBy(m => m.NumberOfDownload).ToList();
                }
            }

            var dates = DateTime.Now.Date.AddDays(-7);
            ViewBag.InReview = db.NoteDetails.Where(m => m.NoteStatus == 4 && m.IsActive == true).Count();
            ViewBag.Download = db.BuyerRequests.Where(m => m.Status == true).Count();
            ViewBag.NewRegistration = db.Users.Where(m => m.CreatedDate >= dates && m.IsActive == true).Count();

            return View(noteDetails.ToPagedList(page ?? 1, 5));
        }

        public ActionResult NoteDetail(string noteId)
        {
            if (adminId == 0)
            {
                return RedirectToAction("Login", "Home");
            }
            var AdminProfile = db.Users.SingleOrDefault(m => m.UserId == adminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.AdminProfile;

            NoteDetail notedetail = db.NoteDetails.SingleOrDefault(m => m.NoteID.ToString().Equals(noteId));
            int avg = 0, count = 0;
            var abc = db.Feedbacks.Where(m => m.NoteID == notedetail.NoteID && m.IsActive == true).ToList();
            if (abc != null)
            {
                count = abc.Count();
                if (count != 0)
                {
                    avg = abc.ToList().Sum(m => m.Ratings) / count;
                }
            }

            if (notedetail.IsPaid == false)
                ViewBag.btnMsg = "Download";
            else
                ViewBag.btnMsg = "Download/$" + notedetail.SellPrice.ToString();

            ViewBag.Review = count;
            ViewBag.Rating = avg;
            ViewBag.Spam = db.SpamReports.Where(m => m.NoteID == notedetail.NoteID).Count();

            List<Feedback> noteReviews = db.Feedbacks.Where(m => m.NoteID.ToString().Equals(noteId) && m.IsActive == true).ToList();
            
            ViewBag.Customer = noteReviews.OrderByDescending(m => m.Ratings);

            return View(notedetail);
        }

        public ActionResult DeleteReview(string noteId, string userId)
        {
            if (noteId != null && userId != null)
            {
                Feedback noteReview = db.Feedbacks.SingleOrDefault(m => m.UserID.ToString().Equals(userId) && m.NoteID.ToString().Equals(noteId));
                noteReview.IsActive = false;
                db.SaveChanges();
                return RedirectToAction("NoteDetail", "Admin", new { noteId = noteId });
            }
            return HttpNotFound();
        }


        public ActionResult NoteUnderReview(int? page, string search, string sortBy, string sellerName, string buttenValue, string noteId, string Remark)
        {
            if (adminId == 0)
            {
                return RedirectToAction("Login", "Home");
            }
            var AdminProfile = db.Users.SingleOrDefault(m => m.UserId == adminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.AdminProfile;

            if (!string.IsNullOrEmpty(buttenValue))
            {
                if (buttenValue == "approve")
                {
                    NoteDetail notes = db.NoteDetails.SingleOrDefault(m => m.NoteID.ToString().Equals(noteId));
                    notes.NoteStatus = 7;
                    notes.PublishedDate = DateTime.Now;
                    notes.ActionedBy = adminId;
                    notes.ModifiedBy = adminId;
                    db.SaveChanges();
                }

                if (buttenValue == "reject")
                {
                    
                     NoteDetail noteDetail = db.NoteDetails.SingleOrDefault(m => m.NoteID.ToString().Equals(noteId) && m.IsActive == true);

                     noteDetail.Remark = Remark;
                     noteDetail.NoteStatus = 6;
                     noteDetail.ModifiedDate = DateTime.Now;
                     noteDetail.ActionedBy = adminId;
                     noteDetail.ModifiedBy = adminId;
                     noteDetail.IsActive = false;
                     db.SaveChanges();
                    
                }

                if (buttenValue == "inreview")
                {
                    NoteDetail notes = db.NoteDetails.SingleOrDefault(m => m.NoteID.ToString().Equals(noteId));
                    notes.NoteStatus = 4;
                    notes.PublishedDate = DateTime.Now;
                    notes.ActionedBy = adminId;
                    notes.ModifiedBy = adminId;
                    db.SaveChanges();
                }
            }

            
            List<NoteDetail> noteDetails = db.NoteDetails.Where(m =>( m.NoteStatus == 2 || m.NoteStatus == 4 ) && m.IsActive == true).ToList();
            noteDetails = noteDetails.OrderByDescending(m => m.CreatedDate).ToList();
            foreach (var item in noteDetails)
            {
                User user = db.Users.SingleOrDefault(m => m.UserId == item.UserID);
                item.Publisher = user.FirstName + " " + user.LastName;
            }

            ViewBag.sellerName = noteDetails.Select(m => m.Publisher).Distinct();

            if (!string.IsNullOrEmpty(search))
            {
                noteDetails = noteDetails.Where(m => m.Title.ToLower().StartsWith(search.ToLower()) || m.Category.Equals(search) || m.Publisher.ToLower().StartsWith(search.ToLower()) || m.CreatedDate.ToString().StartsWith(search) || m.NoteStatu.Name.ToLower().Equals(search.ToLower())).ToList();
            }
            if (!string.IsNullOrEmpty(sortBy))
            {
                if (sortBy == "title")
                {
                    noteDetails = noteDetails.OrderBy(m => m.Title).ToList();
                }
                if (sortBy == "category")
                {
                    noteDetails = noteDetails.OrderBy(m => m.Category).ToList();
                }
                else if (sortBy == "seller")
                {
                    noteDetails = noteDetails.OrderBy(m => m.Publisher).ToList();
                }
                else if (sortBy == "date")
                {
                    noteDetails = noteDetails.OrderBy(m => m.CreatedDate).ToList();
                }
                else if (sortBy == "status")
                {
                    noteDetails = noteDetails.OrderBy(m => m.NoteStatu.Name).ToList();
                }
            }
            if (!string.IsNullOrEmpty(sellerName))
            {
                noteDetails = noteDetails.Where(m => m.Publisher == sellerName).ToList();
            }

            return View(noteDetails.ToPagedList(page ?? 1, 5));
        }

        public ActionResult DownloadNote(int? page, string search, string sortBy, string note, string seller, string buyer)
        {
            if (adminId == 0)
            {
                return RedirectToAction("Login", "Home");
            }

            var AdminProfile = db.Users.SingleOrDefault(m => m.UserId == adminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.AdminProfile;

            List<BuyerRequest> noteList = db.BuyerRequests.Where(m => m.Status == true).ToList();
            
            ViewBag.notes = noteList.Select(m => m.NoteTitle).Distinct();
            ViewBag.buyers = noteList.Select(m => m.BuyerID).Distinct();
            ViewBag.sellers = noteList.Select(m => m.SellerID).Distinct();

            noteList = noteList.OrderByDescending(m => m.ApprovedDate).ToList();
            if (!string.IsNullOrEmpty(search))
            {
                noteList = noteList.Where(m => m.NoteTitle.ToLower().StartsWith(search.ToLower()) || m.NoteDetail.NoteCategory.Name.ToLower().Equals(search.ToLower()) || (m.User.FirstName + " " + m.User.LastName).ToLower().StartsWith(search.ToLower()) || (m.User1.FirstName + " " + m.User1.LastName).ToLower().StartsWith(search.ToLower()) || m.SellType.ToLower().Equals(search.ToLower()) || m.Price.ToString().Equals(search) || m.ApprovedDate.Value.ToString("dd-MM-yyyy,hh:mm").StartsWith(search)).ToList();
            }
            if (!string.IsNullOrEmpty(note))
            {
                noteList = noteList.Where(m => m.NoteTitle.Equals(note)).ToList();
            }
            if (!string.IsNullOrEmpty(seller))
            {
                noteList = noteList.Where(m => (m.User1.FirstName + " " + m.User1.LastName).Equals(seller)).ToList();
            }
            if (!string.IsNullOrEmpty(buyer))
            {
                noteList = noteList.Where(m => (m.User.FirstName + " " + m.User.LastName).Equals(buyer)).ToList();
            }
            if (!string.IsNullOrEmpty(sortBy))
            {
                if (sortBy == "title")
                {
                    noteList = noteList.OrderBy(m => m.NoteTitle).ToList();
                }
                else if (sortBy == "category")
                {
                    noteList = noteList.OrderBy(m => m.NoteDetail.NoteCategory.Name).ToList();
                }
                else if (sortBy == "buyer")
                {
                    noteList = noteList.OrderBy(m => m.User.FirstName).ToList();
                    noteList = noteList.OrderBy(m => m.User.LastName).ToList();
                }
                else if (sortBy == "seller")
                {
                    noteList = noteList.OrderBy(m => m.User1.FirstName).ToList();
                    noteList = noteList.OrderBy(m => m.User1.LastName).ToList();
                }
                else if (sortBy == "price")
                {
                    noteList = noteList.OrderBy(m => m.Price).ToList();
                }
                else if (sortBy == "date")
                {
                    noteList = noteList.OrderBy(m => m.ApprovedDate).ToList();
                }

            }
            return View(noteList.ToPagedList(page ?? 1, 5));
        }

        public ActionResult PublishedNote(int? page, string search, string sortBy, string seller, string NoteID, string Remark)
        {
            if (adminId == 0)
            {
                return RedirectToAction("Login", "Home");
            }

            var AdminProfile = db.Users.SingleOrDefault(m => m.UserId == adminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.AdminProfile;

            if (!string.IsNullOrEmpty(NoteID) && !string.IsNullOrEmpty(Remark))
            {


                try
                {
                    NoteDetail noteDetail = db.NoteDetails.SingleOrDefault(m => m.NoteID.ToString().Equals(NoteID) && m.IsActive == true);

                    noteDetail.Remark = Remark;
                    noteDetail.NoteStatus = 6;
                    noteDetail.ModifiedDate = DateTime.Now;
                    noteDetail.ModifiedBy = adminId;
                    noteDetail.IsActive = false;

                    db.SaveChanges();

                    User user = db.Users.SingleOrDefault(m => m.UserId == noteDetail.UserID && m.IsActive == true);
                    MailMessage mail = new MailMessage("marketplacenote@gmail.com", "jaybhanderi866@gmail.com");//,user.EmailID.ToString());               
                    mail.Subject = "Sorry! We need to remove your notes from our portal.";
                    string Body = "Hello " + user.FirstName + " " + user.LastName + ",<br><br>We want to inform you that, your note " + noteDetail.Title + " has been removed from the portal.<br>Please find our remarks as below -<br>" + Remark + "<br><br>Regards,<br>Notes Marketplace";
                    mail.Body = Body;
                    mail.IsBodyHtml = true;

                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.UseDefaultCredentials = false;

                    smtp.Credentials = new System.Net.NetworkCredential("marketplacenote@gmail.com", "Note@123");
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
                catch (Exception)
                {
                    return HttpNotFound();
                }
            }

            List<NoteDetail> noteDetails = new List<NoteDetail>();
            List<NoteDetail> notes = db.NoteDetails.Where(m => m.NoteStatus == 7 && m.IsActive == true).ToList();
            foreach (var item in notes)
            {
                NoteDetail note = new NoteDetail();
                note.NoteID = item.NoteID;
                note.UserID = item.UserID;
                note.Title = item.Title;
                note.NoteCategory = item.NoteCategory;
                note.notefilename = db.NoteAttachements.SingleOrDefault(m => m.NoteID == item.NoteID).FileName;
                note.NoteSize = item.NoteSize;
                note.SellPrice = item.SellPrice;
                note.PublishedDate = item.PublishedDate;

                User Publisher = db.Users.SingleOrDefault(m => m.UserId == item.UserID);
                note.Publisher = Publisher.FirstName + " " + Publisher.LastName;

                User Modifier = db.Users.SingleOrDefault(m => m.UserId == item.ModifiedBy && m.IsActive == true);
                note.Modifier = Modifier.FirstName + " " + Modifier.LastName;

                note.NumberOfDownload = db.BuyerRequests.Where(m => m.NoteID == item.NoteID && m.Status == true).Count();
                noteDetails.Add(note);
            }

            noteDetails = noteDetails.OrderByDescending(m => m.NumberOfDownload).ToList();

            ViewBag.sellerName = noteDetails.Select(m => m.Publisher).Distinct();

            if (!string.IsNullOrEmpty(search))
            {
                noteDetails = noteDetails.Where(m => m.Title.ToLower().StartsWith(search.ToLower()) || m.Category.Equals(search) || m.SellPrice.Equals(search) || m.Publisher.StartsWith(search) || m.PublishedDate.Value.ToString("dd-MM-yyyy,hh:mm").StartsWith(search)).ToList();
            }
            if (!string.IsNullOrEmpty(seller))
            {
                noteDetails = noteDetails.Where(m => m.Publisher == seller).ToList();
            }
            if (!string.IsNullOrEmpty(sortBy))
            {
                if (sortBy == "title")
                {
                    noteDetails = noteDetails.OrderBy(m => m.Title).ToList();
                }
                else if (sortBy == "category")
                {
                    noteDetails = noteDetails.OrderBy(m => m.NoteCategory.Name).ToList();
                }
                else if (sortBy == "size")
                {
                    noteDetails = noteDetails.OrderBy(m => m.NoteSize).ToList();
                }
                else if (sortBy == "price")
                {
                    noteDetails = noteDetails.OrderBy(m => m.SellPrice).ToList();
                }
                else if (sortBy == "seller")
                {
                    noteDetails = noteDetails.OrderBy(m => m.Publisher).ToList();
                }
                else if (sortBy == "date")
                {
                    noteDetails = noteDetails.OrderBy(m => m.PublishedDate).ToList();
                }
                
            }

          
            return View(noteDetails.ToPagedList(page ?? 1, 5));
        }


        public ActionResult RejectedNote(int? page, string search, string sortBy, string seller, string NoteID, string Remark)
        {
            if (adminId == 0)
            {
                return RedirectToAction("Login", "Home");
            }

            var AdminProfile = db.Users.SingleOrDefault(m => m.UserId == adminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.AdminProfile;

            if (!string.IsNullOrEmpty(NoteID))
            {
                NoteDetail noteDetail = db.NoteDetails.SingleOrDefault(m => m.NoteID.ToString().Equals(NoteID));
                
                noteDetail.NoteStatus = 7;
                noteDetail.ModifiedDate = DateTime.Now;
                noteDetail.ModifiedBy = adminId;
                noteDetail.PublishedDate = DateTime.Now;
                noteDetail.IsActive = true;

                db.SaveChanges();

            }

            List<NoteDetail> noteDetails = new List<NoteDetail>();
            List<NoteDetail> notes = db.NoteDetails.Where(m => m.NoteStatus == 5 ).ToList();
            foreach (var item in notes)
            {
                NoteDetail note = new NoteDetail();
                note.NoteID = item.NoteID;
                note.UserID = item.UserID;
                note.Title = item.Title;
                note.NoteCategory = item.NoteCategory;
                note.notefilename = db.NoteAttachements.SingleOrDefault(m => m.NoteID == item.NoteID).FileName;
                note.ModifiedDate = item.ModifiedDate;
                note.Remark = item.Remark;

                User Publisher = db.Users.SingleOrDefault(m => m.UserId == item.UserID);
                note.Publisher = Publisher.FirstName + " " + Publisher.LastName;

                User Modifier = db.Users.SingleOrDefault(m => m.UserId == item.ModifiedBy && m.IsActive == true);
                note.Modifier = Modifier.FirstName + " " + Modifier.LastName;

                noteDetails.Add(note);
            }

            ViewBag.sellerName = noteDetails.Select(m => m.Publisher).Distinct();

            if (!string.IsNullOrEmpty(search))
            {
                noteDetails = noteDetails.Where(m => m.Title.ToLower().StartsWith(search.ToLower()) || m.Category.Equals(search) || m.SellPrice.Equals(search) || m.Publisher.StartsWith(search) || m.PublishedDate.Value.ToString("dd-MM-yyyy,hh:mm").StartsWith(search)).ToList();
            }
            if (!string.IsNullOrEmpty(seller))
            {
                noteDetails = noteDetails.Where(m => m.Publisher == seller).ToList();
            }
            if (!string.IsNullOrEmpty(sortBy))
            {
                if (sortBy == "title")
                {
                    noteDetails = noteDetails.OrderBy(m => m.Title).ToList();
                }
                else if (sortBy == "category")
                {
                    noteDetails = noteDetails.OrderBy(m => m.NoteCategory.Name).ToList();
                }
                else if (sortBy == "seller")
                {
                    noteDetails = noteDetails.OrderBy(m => m.Publisher).ToList();
                }
                else if (sortBy == "date")
                {
                    noteDetails = noteDetails.OrderBy(m => m.PublishedDate).ToList();
                }

            }

            return View(noteDetails.ToPagedList(page ?? 1, 5));
        }


        public ActionResult Member(int? page, string search, string sortBy, string EmailID, string Password)
        {
            if (adminId == 0)
            {
                return RedirectToAction("Login", "Home");
            }
            var AdminProfile = db.Users.SingleOrDefault(m => m.UserId == adminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.AdminProfile;

            if (!string.IsNullOrEmpty(EmailID) && !string.IsNullOrEmpty(Password))
            {
                User user = db.Users.SingleOrDefault(m => m.EmailID.ToString().Equals(EmailID));
                user.IsActive = false;
                user.Password2 = user.Password;

                List<NoteDetail> noteDetails = db.NoteDetails.Where(m => m.UserID == user.UserId && m.NoteStatus != 7).ToList();
                foreach (var note in noteDetails)
                {
                    note.IsActive = false;
                }
                db.SaveChanges();

                return RedirectToAction("Member", "Admin");
            }
            List<User> users = db.Users.Where(m =>m.UserRolesID == 1 && m.IsActive == true).ToList();
            List<Members> memberList = new List<Members>();
            foreach (var user in users)
            {
                Members member = new Members();
                member.MemberID = user.UserId;
                member.FirstName = user.FirstName;
                member.LastName = user.LastName;
                member.EmailID = user.EmailID;
                member.Password = user.Password;
                member.JoinDate = user.CreatedDate;

                member.UnderReviewNotes = db.NoteDetails.Where(m => m.UserID == user.UserId && m.NoteStatus == 4 && m.IsActive == true).Count(); ;
                member.PublishedNotes = db.NoteDetails.Where(m => m.UserID == user.UserId && m.NoteStatus == 7 && m.IsActive == true).Count(); ;


                List<BuyerRequest> noteRequests = db.BuyerRequests.Where(m => m.SellerID == user.UserId && m.Status == true).ToList();
                int count1 = 0;
                foreach (var noteReq in noteRequests)
                {
                    if (noteReq != null)
                        count1 += (int)db.NoteDetails.SingleOrDefault(m => m.UserID == user.UserId && m.NoteID == noteReq.NoteID).SellPrice;
                }

                List<BuyerRequest> noteRequests2 = db.BuyerRequests.Where(m => m.BuyerID == user.UserId && m.Status == true).ToList();
                int count2 = 0;
                foreach (var noteReq in noteRequests2)
                {
                    if (noteReq != null)
                        count2 += (int)db.NoteDetails.SingleOrDefault(m => m.NoteID == noteReq.NoteID).SellPrice;
                }
                member.DownloadNotes = noteRequests2.Count();
                member.TotalEarnings = count1;
                member.TotalExpenses = count2;

                memberList.Add(member);
            }

            memberList = memberList.OrderBy(m => m.JoinDate).ToList();

            if (!string.IsNullOrEmpty(search))
            {
                memberList = memberList.Where(m => m.FirstName.ToLower().Equals(search.ToLower()) || m.LastName.ToLower().Equals(search.ToLower()) || m.EmailID.ToLower().StartsWith(search.ToLower()) || m.JoinDate.Value.ToString().StartsWith(search) || m.TotalExpenses.ToString().Equals(search) || m.TotalEarnings.ToString().Equals(search.ToLower())).ToList();
            }
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                if (sortBy == "firstName")
                {
                    memberList = memberList.OrderBy(m => m.FirstName).ToList();
                }
                else if (sortBy == "lastName")
                {
                    memberList = memberList.OrderBy(m => m.LastName).ToList();
                }
                else if (sortBy == "email")
                {
                    memberList = memberList.OrderBy(m => m.EmailID).ToList();
                }
                else if (sortBy == "date")
                {
                    memberList = memberList.OrderBy(m => m.JoinDate).ToList();
                }
                else if (sortBy == "underReviewNote")
                {
                    memberList = memberList.OrderBy(m => m.UnderReviewNotes).ToList();
                }
                else if (sortBy == "publishedNote")
                {
                    memberList = memberList.OrderBy(m => m.PublishedNotes).ToList();
                }
                else if (sortBy == "downloadNote")
                {
                    memberList = memberList.OrderBy(m => m.DownloadNotes).ToList();
                }
                else if (sortBy == "expenses")
                {
                    memberList = memberList.OrderBy(m => m.TotalExpenses).ToList();
                }
                else if (sortBy == "earning")
                {
                    memberList = memberList.OrderBy(m => m.TotalEarnings).ToList();
                }
            }
            return View(memberList.ToPagedList(page ?? 1, 5));
        }
        
    
        public ActionResult SpamReport(int? page, string search, string sortBy, string noteId, string userId)
        {
            if (adminId == 0)
            {
                return RedirectToAction("Login", "Home");
            }

            var AdminProfile = db.Users.SingleOrDefault(m => m.UserId == adminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.AdminProfile;

            if (!string.IsNullOrEmpty(noteId) && !string.IsNullOrEmpty(userId))
            {
                var spamList = db.SpamReports.SingleOrDefault(m => m.NoteID.ToString().Equals(noteId) && m.UserID.ToString().Equals(userId));
                spamList.IsActive = false;
                db.SaveChanges();
                return RedirectToAction("SpamReport", "Admin");
            }

            List<SpamReport> spamReports = db.SpamReports.Where(m => m.IsActive == true).ToList();

            foreach (var item in spamReports)
            {
                item.NoteDetail.Title = db.NoteDetails.SingleOrDefault(m => m.NoteID == item.NoteID).Title;
                item.NoteDetail.NoteCategory = db.NoteDetails.SingleOrDefault(m => m.NoteID == item.NoteID).NoteCategory;
            }

            if (!string.IsNullOrEmpty(search))
            {
                spamReports = spamReports.Where(m => m.NoteDetail.Title.ToLower().StartsWith(search.ToLower()) || m.NoteDetail.NoteCategory.Name.ToLower().Equals(search.ToLower()) || m.CreatedDate.Value.ToString("dd-MM-yyyy,hh:mm").StartsWith(search)).ToList();
            }
            if (!string.IsNullOrEmpty(sortBy))
            {
                if (sortBy == "title")
                {
                    spamReports = spamReports.OrderBy(m => m.NoteDetail.Title).ToList();
                }
                else if (sortBy == "category")
                {
                    spamReports = spamReports.OrderBy(m => m.NoteDetail.NoteCategory.Name).ToList();
                }
                else if (sortBy == "reportedBy")
                {
                    spamReports = spamReports.OrderBy(m => m.User.FirstName).ToList();
                    spamReports = spamReports.OrderBy(m => m.User.LastName).ToList();
                }
                else if (sortBy == "date")
                {
                    spamReports = spamReports.OrderBy(m => m.CreatedDate).ToList();
                }

            }
            return View(spamReports.ToPagedList(page ?? 1, 5));
        } 
        public ActionResult MemberDetail(int? page, string UserId, string sortBy)
        {
            if (adminId == 0)
            {
                return RedirectToAction("Login", "Home");
            }

            if (string.IsNullOrEmpty(UserId))
            {
                return HttpNotFound();
            }
            var AdminProfile = db.Users.SingleOrDefault(m => m.UserId == adminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.AdminProfile;

            UserProfileDetail userDetail = db.UserProfileDetails.SingleOrDefault(m => m.UserID.ToString().Equals(UserId) && m.User.IsActive == true);
            if (userDetail == null)
            {
                return HttpNotFound();
            }
            List<NoteDetail> noteDetails = db.NoteDetails.Where(m => m.UserID.ToString().Equals(UserId) && m.NoteStatus != 1 && m.IsActive == true).ToList();
            noteDetails = noteDetails.OrderBy(m => m.CreatedDate).ToList();

            if (!string.IsNullOrEmpty(sortBy))
            {
                if (sortBy == "title")
                {
                    noteDetails = noteDetails.OrderBy(m => m.Title).ToList();
                }
                else if (sortBy == "category")
                {
                    noteDetails = noteDetails.OrderBy(m => m.Category).ToList();
                }
                else if (sortBy == "status")
                {
                    noteDetails = noteDetails.OrderBy(m => m.NoteStatu.Name).ToList();
                }
                else if (sortBy == "date")
                {
                    noteDetails = noteDetails.OrderBy(m => m.CreatedDate).ToList();
                }
                else if (sortBy == "publishdate")
                {
                    noteDetails = noteDetails.OrderBy(m => m.PublishedDate).ToList();
                }
            }

            MemberDetails memberDetail = new MemberDetails();
            memberDetail.UserProfile = userDetail;
            memberDetail.pagedList = noteDetails.ToPagedList(page ?? 1, 5);
            memberDetail.noteRequest = db.BuyerRequests.ToList();

            return View(memberDetail);
        }

        public ActionResult ManageAdministrator(int? page, string search, string sortBy, string ID)
        {
            if (db.Users.SingleOrDefault(m=>m.UserId == adminId ).UserRolesID != 3)
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            var AdminProfile = db.Users.SingleOrDefault(m => m.UserId == adminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.AdminProfile;

            if (!string.IsNullOrEmpty(ID))
            {
                var admin = db.Users.FirstOrDefault(m => m.UserId.ToString().Equals(ID));
                if (admin != null)
                {
                    admin.Password2 = admin.Password;
                    admin.IsActive = false;
                    db.SaveChanges();
                }
            }

            List<User> adminDetails = db.Users.Where(m => m.UserRolesID == 2 && m.IsActive == true).OrderByDescending(m => m.ModifiedDate).ToList();

            if (!string.IsNullOrEmpty(search))
            {
                adminDetails = adminDetails.Where(m => m.FirstName.ToLower().StartsWith(search.ToLower()) || m.LastName.ToLower().Equals(search.ToLower()) || m.EmailID.ToLower().StartsWith(search.ToLower()) || m.PhoneNumber.StartsWith(search) || m.ModifiedDate.Value.ToString("dd-MM-yyyy,hh:mm").StartsWith(search)).ToList();
            }
            if (!string.IsNullOrEmpty(sortBy))
            {
                if (sortBy == "firstName")
                {
                    adminDetails = adminDetails.OrderBy(m => m.FirstName).ToList();
                }
                else if (sortBy == "lastName")
                {
                    adminDetails = adminDetails.OrderBy(m => m.LastName).ToList();
                }
                else if (sortBy == "email")
                {
                    adminDetails = adminDetails.OrderBy(m => m.EmailID).ToList();
                }
                else if (sortBy == "phone")
                {
                    adminDetails = adminDetails.OrderBy(m => m.PhoneNumber).ToList();
                }
                else if (sortBy == "date")
                {
                    adminDetails = adminDetails.OrderBy(m => m.ModifiedDate).ToList();
                }
            }
            return View(adminDetails.ToPagedList(page ?? 1, 5));
        }



        public ActionResult AddAdministrator(string ID)
        {
            if (db.Users.SingleOrDefault(m => m.UserId == adminId).UserRolesID != 3)
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            var AdminProfile = db.Users.SingleOrDefault(m => m.UserId == adminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.AdminProfile;

            List<Country> country = db.Countries.ToList();
            ViewBag.country = country;

            User adminDetail = new User();
            if (!string.IsNullOrEmpty(ID))
            {
                adminDetail = db.Users.FirstOrDefault(m => m.UserId.ToString().Equals(ID));
            }

            return View(adminDetail);
        }

        [HttpPost]
        public ActionResult AddAdministrator(User adminDetail)
        {
            if (db.Users.SingleOrDefault(m => m.UserId == adminId).UserRolesID != 3)
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            List<Country> country = db.Countries.ToList();
            ViewBag.country = country;

            if (!ModelState.IsValid)
            {
                return View(adminDetail);
            }

            var AdminProfile = db.Users.SingleOrDefault(m => m.UserId == adminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.AdminProfile;

            var admin = db.Users.SingleOrDefault(m => m.UserId == adminDetail.UserId);
            if (admin == null)
            {
                adminDetail.Password = "123456";
                adminDetail.Password2 = "123456";
                adminDetail.UserRolesID = 2;
                adminDetail.ModifiedBy = adminId;
                adminDetail.ModifiedDate = DateTime.Now;
                adminDetail.IsActive = true;
                var temp = db.Users.FirstOrDefault(m => m.EmailID.ToLower().Equals(adminDetail.EmailID.ToLower()));
                if (temp != null)
                {
                    ModelState.AddModelError("Email", "This email is exist!");
                    return View(adminDetail);
                }
                db.Users.Add(adminDetail);
            }
            else
            {
                admin.FirstName = adminDetail.FirstName;
                admin.LastName = adminDetail.LastName;
                admin.PhoneNumber = adminDetail.PhoneNumber;
                admin.Password = adminDetail.Password;
                admin.Password2 = adminDetail.Password2;
            }

            ModelState.Clear();

            db.SaveChanges();

            return RedirectToAction("ManageAdministrator", "Admin");
        }

        public ActionResult ManageSystemConfiguration()
        {
            if (db.Users.SingleOrDefault(m => m.UserId == adminId).UserRolesID != 3)
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            var AdminProfile = db.Users.SingleOrDefault(m => m.UserId == adminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.AdminProfile;

            SystemConfiguration temp = db.SystemConfigurations.SingleOrDefault();
            return View(temp);
        }

        [HttpPost]
        public ActionResult ManageSystemConfiguration(SystemConfiguration systemConfiguration)
        {
            if (systemConfiguration.FileProfilePicture != null)
            {
                if (systemConfiguration.FileProfilePicture.ContentLength > 10 * 1024 * 1024)
                {
                    ViewBag.fileMsg = "file size is morethan 10 MB.";
                    return View(systemConfiguration);
                }
                else
                {
                    string fileName = "default.jpg";
                    string path = Path.Combine(Server.MapPath("~/Uploads/ProfilePicture/"), fileName);
                    systemConfiguration.FileProfilePicture.SaveAs(path);
                    systemConfiguration.DefaultProfilePicture = fileName;
                }
            }
            if (systemConfiguration.FileNotePreview != null)
            {
                if (systemConfiguration.FileNotePreview.ContentLength > 10 * 1024 * 1024)
                {
                    ViewBag.fileMsg = "file size is morethan 10 MB.";
                    return View(systemConfiguration);
                }
                else
                {
                    string fileName = "default.jpg";
                    string path = Path.Combine(Server.MapPath("~/Uploads/BookPicture/"), fileName);
                    systemConfiguration.FileNotePreview.SaveAs(path);
                    systemConfiguration.DefaultNotePreview = fileName;
                }
            }

            SystemConfiguration temp = db.SystemConfigurations.SingleOrDefault();

            if (temp != null)
            {
                temp.EmailID1 = systemConfiguration.EmailID1;
                temp.EmailID2 = systemConfiguration.EmailID2;
                temp.PhoneNumber = systemConfiguration.PhoneNumber;
                temp.FacebookUrl = systemConfiguration.FacebookUrl;
                temp.TwitterUrl = systemConfiguration.TwitterUrl;
                temp.LinkedInUrl = systemConfiguration.LinkedInUrl;
                temp.ModifiedDate = DateTime.Now;
                temp.ModifiedBy = adminId;
            }
            else
            {
                systemConfiguration.CreatedDate = DateTime.Now;
                systemConfiguration.CreatedBy = adminId;
                db.SystemConfigurations.Add(systemConfiguration);
            }
            db.SaveChanges();

            return RedirectToAction("Dashboard", "Admin");
        }

        public ActionResult ManageCategory(int? page, string search, string sortBy)
        {
            if (adminId == 0)
            {
                return RedirectToAction("Login", "Admin");
            }
            var AdminProfile = db.Users.SingleOrDefault(m => m.UserId == adminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.AdminProfile;

            List<NoteCategory> categories = new List<NoteCategory>();
            List<NoteCategory> category = db.NoteCategories.OrderByDescending(m => m.CreatedDate).ToList();
            foreach (var temp in category)
            {
                var admin = db.Users.SingleOrDefault(m => m.UserId == temp.CreatedBy);
                temp.AddedBy = admin.FirstName + " " + admin.LastName;
                categories.Add(temp);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                categories = categories.Where(m => m.Name.ToLower().Equals(search.ToLower()) || m.Description.ToLower().StartsWith(search.ToLower()) || m.CreatedDate.Value.ToString("dd-MM-yyyy,hh:mm").StartsWith(search) || m.AddedBy.ToLower().StartsWith(search.ToLower())).ToList();
            }
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                if (sortBy == "category")
                {
                    categories = categories.OrderBy(m => m.Name).ToList();
                }
                else if (sortBy == "description")
                {
                    categories = categories.OrderBy(m => m.Description).ToList();
                }
                else if (sortBy == "date")
                {
                    categories = categories.OrderBy(m => m.CreatedDate).ToList();
                }
                else if (sortBy == "addedBy")
                {
                    categories = categories.OrderBy(m => m.AddedBy).ToList();
                }
            }

            return View(categories.ToPagedList(page ?? 1, 5));
        }

        public ActionResult AddCategory()
        {
            if (adminId == 0)
            {
                return RedirectToAction("Login", "Admin");
            }
            var AdminProfile = db.Users.SingleOrDefault(m => m.UserId == adminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.AdminProfile;
            return View(new NoteCategory());
        }

        [HttpPost]
        public ActionResult AddCategory(NoteCategory category)
        {
            var AdminProfile = db.Users.SingleOrDefault(m => m.UserId == adminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.AdminProfile;

            if (!ModelState.IsValid)
            {
                return View(category);
            }

            try
            {
                NoteCategory temp = db.NoteCategories.FirstOrDefault(m => m.Name.ToLower().Equals(category.Name.ToLower()));
                if (temp == null)
                {
                    category.CreatedBy = adminId;
                    category.CreatedDate = DateTime.Now;
                    category.IsActive = true;
                    db.NoteCategories.Add(category);
                }
                else
                {
                    temp.ModifiedBy = adminId;
                    temp.ModifiedDate = DateTime.Now;
                }

                db.SaveChanges();
                return RedirectToAction("ManageCategory", "Admin");
            }
            catch (Exception)
            {
                return HttpNotFound();
            }
        }

        public ActionResult EditCategory(String categoryId)
        {
            if (!string.IsNullOrEmpty(categoryId))
            {
                NoteCategory temp = db.NoteCategories.FirstOrDefault(m => m.ID.ToString().Equals(categoryId));
                if (temp != null)
                {
                    return View("AddCategory", temp);
                }
            }
            return HttpNotFound();
        }

        [HttpPost]
        public ActionResult EditCategory(NoteCategory category)
        {
            if (adminId != 0)
            {
                if (category != null)
                {
                    NoteCategory temp = db.NoteCategories.FirstOrDefault(m => m.ID == category.ID);
                    if (temp != null)
                    {
                        temp.Name = category.Name;
                        temp.Description = category.Description;
                        temp.ModifiedBy = adminId;
                        temp.ModifiedDate = DateTime.Now;
                        temp.IsActive = true;
                    }
                    db.SaveChanges();
                    return RedirectToAction("ManageCategory", "Admin");
                }
            }
            return HttpNotFound();
        }

        public ActionResult DeleteCategory(string categoryId)
        {
            if (adminId == 0)
            {
                return RedirectToAction("Login", "Admin");
            }
            if (!string.IsNullOrEmpty(categoryId))
            {
                NoteCategory temp = db.NoteCategories.FirstOrDefault(m => m.ID.ToString().Equals(categoryId));
                if (temp != null)
                {
                    temp.IsActive = false;
                }
                db.SaveChanges();
                return RedirectToAction("ManageCategory", "Admin");
            }
            return HttpNotFound();
        }


        public ActionResult ManageType(int? page, string search, string sortBy)
        {
            if (adminId == 0)
            {
                return RedirectToAction("Login", "Admin");
            }
            var AdminProfile = db.Users.SingleOrDefault(m => m.UserId == adminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.AdminProfile;

            List<NoteType> types = new List<NoteType>();
            List<NoteType> type = db.NoteTypes.OrderByDescending(m => m.CreatedDate).ToList();
            foreach (var temp in type)
            {
                var admin = db.Users.SingleOrDefault(m => m.UserId == temp.CreatedBy);
                temp.Addedby = admin.FirstName + " " + admin.LastName;
                types.Add(temp);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                types = types.Where(m => m.Name.ToLower().Equals(search.ToLower()) || m.Description.ToLower().StartsWith(search.ToLower()) || m.CreatedDate.Value.ToString("dd-MM-yyyy,hh:mm").StartsWith(search) || m.Addedby.ToLower().StartsWith(search.ToLower())).ToList();
            }
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                if (sortBy == "type")
                {
                    types = types.OrderBy(m => m.Name).ToList();
                }
                else if (sortBy == "description")
                {
                    types = types.OrderBy(m => m.Description).ToList();
                }
                else if (sortBy == "date")
                {
                    types = types.OrderBy(m => m.CreatedDate).ToList();
                }
                else if (sortBy == "addedBy")
                {
                    types = types.OrderBy(m => m.Addedby).ToList();
                }
            }

            return View(types.ToPagedList(page ?? 1, 5));
        }  


        public ActionResult AddType()
        {
            if (adminId == 0)
            {
                return RedirectToAction("Login", "Admin");
            }
            var AdminProfile = db.Users.SingleOrDefault(m => m.UserId == adminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.AdminProfile;
            return View(new NoteType());
        }

        [HttpPost]
        public ActionResult AddType(NoteType type)
        {
            var AdminProfile = db.Users.SingleOrDefault(m => m.UserId == adminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.AdminProfile;

            if (!ModelState.IsValid)
            {
                return View(type);
            }

            try
            {
                NoteType temp = db.NoteTypes.FirstOrDefault(m => m.Name.ToLower().Equals(type.Name.ToLower()));
                if (temp == null)
                {
                    type.CreatedBy = adminId;
                    type.CreatedDate = DateTime.Now;
                    type.IsActive = true;
                    db.NoteTypes.Add(type);
                }
                else
                {
                    temp.ModifiedBy = adminId;
                    temp.ModifiedDate = DateTime.Now;
                }

                db.SaveChanges();
                return RedirectToAction("ManageType", "Admin");
            }
            catch (Exception)
            {
                return HttpNotFound();
            }
        }

        public ActionResult EditType(String typeId)
        {
            if (!string.IsNullOrEmpty(typeId))
            {
                NoteType temp = db.NoteTypes.FirstOrDefault(m => m.ID.ToString().Equals(typeId));
                if (temp != null)
                {
                    return View("AddType", temp);
                }
            }
            return HttpNotFound();
        }

        [HttpPost]
        public ActionResult EditType(NoteType type)
        {
            if (adminId != 0)
            {
                if (type != null)
                {
                    NoteType temp = db.NoteTypes.FirstOrDefault(m => m.ID == type.ID);
                    if (temp != null)
                    {
                        temp.Name = type.Name;
                        temp.Description = type.Description;
                        temp.ModifiedBy = adminId;
                        temp.ModifiedDate = DateTime.Now;
                        temp.IsActive = true;
                    }
                    db.SaveChanges();
                    return RedirectToAction("ManageType", "Admin");
                }
            }
            return HttpNotFound();
        }

        public ActionResult DeleteType(string typeId)
        {
            if (adminId == 0)
            {
                return RedirectToAction("Login", "Admin");
            }
            if (!string.IsNullOrEmpty(typeId))
            {
                NoteType temp = db.NoteTypes.FirstOrDefault(m => m.ID.ToString().Equals(typeId));
                if (temp != null)
                {
                    temp.IsActive = false;
                }
                db.SaveChanges();
                return RedirectToAction("ManageType", "Admin");
            }
            return HttpNotFound();
        }

        public ActionResult ManageCountry(int? page, string search, string sortBy)
        {
            if (adminId == 0)
            {
                return RedirectToAction("Login", "Admin");
            }
            var AdminProfile = db.Users.SingleOrDefault(m => m.UserId == adminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.AdminProfile;

            List<Country> countries = new List<Country>();
            List<Country> country = db.Countries.OrderByDescending(m => m.CreatedDate).ToList();
            foreach (var temp in country)
            {
                var admin = db.Users.SingleOrDefault(m => m.UserId == temp.CreatedBy);
                temp.Addedby = admin.FirstName + " " + admin.LastName;
                countries.Add(temp);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                countries = countries.Where(m => m.Name.ToLower().Equals(search.ToLower()) || m.CountryCode.Equals(search) || m.CreatedDate.Value.ToString("dd-MM-yyyy,hh:mm").StartsWith(search) || m.Addedby.ToLower().StartsWith(search.ToLower())).ToList();
            }
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                if (sortBy == "country")
                {
                    countries = countries.OrderBy(m => m.Name).ToList();
                }
                else if (sortBy == "countryCode")
                {
                    countries = countries.OrderBy(m => m.CountryCode).ToList();
                }
                else if (sortBy == "date")
                {
                    countries = countries.OrderBy(m => m.CreatedDate).ToList();
                }
                else if (sortBy == "addedBy")
                {
                    countries = countries.OrderBy(m => m.Addedby).ToList();
                }
            }

            return View(countries.ToPagedList(page ?? 1, 5));
        }

        public ActionResult AddCountry()
        {
            if (adminId == 0)
            {
                return RedirectToAction("Login", "Admin");
            }
            var AdminProfile = db.Users.SingleOrDefault(m => m.UserId == adminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.AdminProfile;
            return View(new Country());
        }

        [HttpPost]
        public ActionResult AddCountry(Country country)
        {
            var AdminProfile = db.Users.SingleOrDefault(m => m.UserId == adminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.AdminProfile;

            if (!ModelState.IsValid)
            {
                return View(country);
            }

            try
            {
                Country temp = db.Countries.FirstOrDefault(m => m.Name.ToLower().Equals(country.Name.ToLower()));
                if (temp == null)
                {
                    country.CreatedBy = adminId;
                    country.CreatedDate = DateTime.Now;
                    country.IsActice = true;
                    db.Countries.Add(country);
                }
                else
                {
                    temp.ModifiedBy = adminId;
                    temp.ModifiedDate = DateTime.Now;
                }

                db.SaveChanges();
                return RedirectToAction("ManageCountry", "Admin");
            }
            catch (Exception)
            {
                return HttpNotFound();
            }
        }

        public ActionResult EditCountry(String countryId)
        {
            if (!string.IsNullOrEmpty(countryId))
            {
                Country temp = db.Countries.FirstOrDefault(m => m.ID.ToString().Equals(countryId));
                if (temp != null)
                {
                    return View("AddCountry", temp);
                }
            }
            return HttpNotFound();
        }

        [HttpPost]
        public ActionResult EditCountry(Country country)
        {
            if (adminId != 0)
            {
                if (country != null)
                {
                    Country temp = db.Countries.FirstOrDefault(m => m.ID == country.ID);
                    if (temp != null)
                    {
                        temp.Name = country.Name;
                        temp.CountryCode = country.CountryCode;
                        temp.ModifiedBy = adminId;
                        temp.ModifiedDate = DateTime.Now;
                      //  temp.IsActive = true;
                    }
                    db.SaveChanges();
                    return RedirectToAction("ManageCountry", "Admin");
                }
            }
            return HttpNotFound();
        }

        public ActionResult DeleteCountry(string countryId)
        {
            if (adminId == 0)
            {
                return RedirectToAction("Login", "Admin");
            }
            if (!string.IsNullOrEmpty(countryId))
            {
                Country temp = db.Countries.FirstOrDefault(m => m.ID.ToString().Equals(countryId));
                if (temp != null)
                {
                  //  temp.IsActive = false;
                }
                db.SaveChanges();
                return RedirectToAction("ManageCountry", "Admin");
            }
            return HttpNotFound();
        }

        public ActionResult AdminProfile()
        {
            var AdminProfile = db.Users.SingleOrDefault(m => m.UserId == adminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.AdminProfile;

            List<Country> country = db.Countries.ToList();
            ViewBag.country = country;

            User adminDetail = db.Users.SingleOrDefault(m => m.UserId == adminId);
            if (adminDetail == null)
            {
                adminDetail = new User();
            }
            return View(adminDetail);
        }

        [HttpPost]
        public ActionResult AdminProfile(User adminDetail)
        {
            var AdminProfile = db.Users.SingleOrDefault(m => m.UserId == adminId);
            if (AdminProfile != null)
                ViewBag.AdminPicture = AdminProfile.AdminProfile;

            List<Country> country = db.Countries.ToList();
            ViewBag.country = country;

            adminDetail.IsActive = true;
            if (adminDetail.AdminProfileFile != null)
            {
                if (adminDetail.AdminProfileFile.ContentLength > 10 * 1024 * 1024)
                {
                    ViewBag.fileMsg = "file size is morethan 10 MB.";
                    return View(adminDetail);
                }
                else
                {
                    string extension = Path.GetExtension(adminDetail.AdminProfileFile.FileName);
                    string fileName = adminId.ToString() + DateTime.Now.ToString("yyyyMMddHHmmss") + extension;
                    string path = Path.Combine(Server.MapPath("~/Uploads/ProfilePicture/"), fileName);
                    adminDetail.AdminProfileFile.SaveAs(path);
                    adminDetail.AdminProfile = fileName;
                }
            }

            try
            {
                var admin = db.Users.SingleOrDefault(m => m.UserId == adminId);
                if (admin == null)
                {
                    admin.Password2 = admin.Password;
                    db.Users.Add(adminDetail);
                }
                else
                {
                    admin.FirstName = adminDetail.FirstName;
                    admin.LastName = adminDetail.LastName;
                    admin.Password2 = admin.Password;
                    admin.SecondaryEmailID = adminDetail.SecondaryEmailID;
                    admin.PhoneNumber = adminDetail.PhoneNumber;
                    admin.AdminProfile = adminDetail.AdminProfile;
                    admin.ModifiedDate = DateTime.Now;
                }

                db.SaveChanges();
                return RedirectToAction("Dashboard", "Admin");

            }
            catch (Exception)
            {
                return View(adminDetail);
            }
        }
    }
}