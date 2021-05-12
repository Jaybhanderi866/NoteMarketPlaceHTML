using NoteMarketPlace.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Helpers;


namespace NoteMarketPlace.Controllers
{
    public class HomeController : Controller
    {
        static int userId = 0;
        NoteMarketPlaceEntities db = new NoteMarketPlaceEntities();

        public ActionResult Home()
        {
            ViewBag.isRegister = false;
            
            if (userId != 0)
            {
                
                var userprofile = db.UserProfileDetails.SingleOrDefault(m => m.UserID == userId);
                if (db.Users.SingleOrDefault(m => m.UserId == userId).UserRolesID == 1)
                    ViewBag.isRegister = true;
                else
                    return RedirectToAction("Login", "Admin", new { emailID = userprofile.User.EmailID, Password = userprofile.User.Password });

                if (userprofile != null)
                    ViewBag.profilePicture = userprofile.ProfilePicture;
            }
            return View();
        }
        public ActionResult SignUp()
        {
            if (userId == 0)
            {
                ViewBag.isCreated = false;
                return View();
            }
            else
            {
                return RedirectToAction("Home", "Home");
            }
        }

        [HttpPost]
        public ActionResult SignUp(User user)
        {
            ViewBag.iscreated = false;
            if (!ModelState.IsValid)
            {
                return View();
            }
            if (db.Users.Any(m => m.EmailID == user.EmailID))
            {
                ModelState.AddModelError("EmailID", "This Email Address is exist!");
                return View(user);
            }
            else
            {
                UserRole userRole = db.UserRoles.SingleOrDefault(m => m.UserRolesID == 1);
                user.UserRolesID = 1;
                user.IsActive = true;
                user.IsDetailsSubmitted = false;
                user.IsEmailVerified = false;
                user.CreatedDate = DateTime.Now.Date;
                user.ModifiedDate = DateTime.Now.Date;
                user.UserRole = userRole;
                db.Users.Add(user);


                try
                {
                    db.SaveChanges();

                    MailMessage mail = new MailMessage("marketplacenote@gmail.com", "jaybhanderi866@gmail.com");//,user.EmailID.ToString());               
                    mail.Subject = "Note MarketPlace - Email Verification";
                    string Body = "Hello " + user.FirstName + " " + user.LastName + ",<br><br>Thank you for signing up with us.Please click on below link to verify your email address and to do login.<br>https://localhost:44336/Home/EmailVerification?emailId=" + user.EmailID + "<br><br>Regards,<br>Notes Marketplace";
                    mail.Body = Body;
                    mail.IsBodyHtml = true;

                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.UseDefaultCredentials = false;

                    smtp.Credentials = new System.Net.NetworkCredential("marketplacenote@gmail.com", "Note@123");
                    smtp.EnableSsl = true;
                    smtp.Send(mail);

                    ViewBag.isCreated = true;
                    return View();
                }
                catch (Exception)
                {
                    return View(user);
                }
            }
        }

        public ActionResult EmailVerification(string emailId)
        {


            if (userId == 0)
            {
                try
                {
                    User v_user = db.Users.SingleOrDefault(m => m.EmailID.Equals(emailId.ToString()));
                    if (v_user == null)
                        return HttpNotFound();
                    else
                        return View(v_user);
                }
                catch (Exception)
                {
                    return HttpNotFound();
                }
            }
            else
            {
                return RedirectToAction("Home", "Home");
            }
        }

        [HttpPost]
        public ActionResult EmailVerification(int id)
        {
            User v_user = db.Users.SingleOrDefault(m => m.UserId == id);
            try
            {
                if (v_user != null)
                {
                    v_user.FirstName = v_user.FirstName;
                    v_user.LastName = v_user.LastName;
                    v_user.Password2 = v_user.Password;
                    v_user.IsDetailsSubmitted = false;
                    v_user.IsEmailVerified = true;
                    v_user.IsActive = true;
                    db.SaveChanges();
                    return RedirectToAction("Login", "Home");
                }
                else
                    return HttpNotFound();

            }
            catch (Exception)
            {
                return View(v_user);
            }
        }

        public ActionResult Login(int? adminID)
        {
            if (userId == 0 || adminID == 0)
            {
                HttpCookie cookie = Request.Cookies["RememberMe"];
                User v_user = new User();
                if (cookie != null)
                {
                    v_user.EmailID = cookie["emailId"];
                    v_user.Password = cookie["password"];
                    if (cookie["emailId"].Length != 0)
                    {
                        v_user.RememberMe = true;
                    }
                    else
                    {
                        v_user.RememberMe = false;
                    }
                }
                return View(v_user);
            }
            else
            {
                return RedirectToAction("Home", "Home");
            }
        }

        [HttpPost]
        public ActionResult Login(User user)
        {
            HttpCookie cookie = new HttpCookie("RememberMe");
            if (user.RememberMe == true)
            {
                cookie["emailId"] = user.EmailID;
                cookie["password"] = user.Password;
                cookie.Expires = DateTime.Now.AddMonths(2);
                Response.Cookies.Add(cookie);
            }
            else
            {
                cookie["emailId"] = null;
                cookie["password"] = null;
                Response.Cookies.Add(cookie);
            }

            User auth = db.Users.Where(m => m.EmailID.Equals(user.EmailID) && m.Password.Equals(user.Password)).SingleOrDefault();
            if (auth != null)
            {
                if(auth.UserRolesID == 1) 
                {
                    if (auth.IsActive == true)
                    {
                        if (auth.IsEmailVerified == true)
                        {
                            userId = auth.UserId;
                            if (auth.IsDetailsSubmitted == true)
                            {
                                return RedirectToAction("Home", "Home");
                            }
                            else
                            {
                                return RedirectToAction("UserProfile", "Home");
                            }
                        }
                        else
                        {
                            ViewBag.auth_msg = "Please verify your Email Address!";
                            return View(user);
                        }
                    }
                    else
                    {
                        ViewBag.auth_msg = "You are not a Member!";
                    }
                }
                else
                {
                    return RedirectToAction("Login", "Admin", new { emailID = auth.EmailID, Password = auth.Password });
                }
                
            }
            else
            {
                ViewBag.auth_msg = "Enter valid username or password!";
            }
            return View(user);
        }

        public ActionResult ForgotPassword()
        {

            if (userId == 0)
            {
                ViewBag.isAuth = false;
                return View();
            }
            else
            {
                return RedirectToAction("Home", "Home");
            }
        }

        [HttpPost]
        public ActionResult ForgotPassword(User user)
        {
            User v_user = db.Users.SingleOrDefault(m => m.EmailID.Equals(user.EmailID.ToString()));
            if (v_user != null)
            {
                try
                {
                    MailMessage mail = new MailMessage("marketplacenote@gmail.com", "jaybhanderi866@gmail.com");//user.EmailID.ToString());
                    mail.Subject = "Notes MarketPlace New Generated Password";
                    string password = Membership.GeneratePassword(10, 3);
                    string Body = "Hello " + v_user.FirstName + " " + v_user.LastName + ",<br><br>We have generated a new password for you.<br>Password :  " + password + "<br><br>Regards,<br>Notes Marketplace";
                    mail.Body = Body;
                    mail.IsBodyHtml = true;

                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.UseDefaultCredentials = false;

                    smtp.Credentials = new System.Net.NetworkCredential("marketplacenote@gmail.com", "Note@123");
                    smtp.EnableSsl = true;
                    smtp.Send(mail);

                    v_user.Password = password;
                    v_user.Password2 = password;
                    db.SaveChanges();
                    ViewBag.isAuth = true;
                    ViewBag.Auth_msg = "Your password has been changed successfully and newly generated password is sent on your registered email address.";
                    return View();

                }
                catch (Exception)
                {
                    return HttpNotFound();
                }
            }
            else
            {
                ViewBag.isAuth = false;
                ViewBag.msg = "This email address does not exist!";
                return View();
            }
        }

        public ActionResult FAQ()
        {
            ViewBag.isRegister = false;
            if (userId != 0)
            {
                ViewBag.isRegister = true;
                var userprofile = db.UserProfileDetails.SingleOrDefault(m => m.UserID == userId);
                if (userprofile != null)
                    ViewBag.profilePicture = userprofile.ProfilePicture;
            }
            return View();
        }

        public ActionResult ContactUs()
        {
            ViewBag.IsEmailReadOnly = false;
            ViewBag.IsRegister = false;
            if (userId != 0)
            {
                ViewBag.ISRegister = true;
                User v_user = db.Users.SingleOrDefault(m => m.UserId == userId);
                ContactU contact = new ContactU();
                contact.FullName = v_user.FirstName + " " + v_user.LastName;
                contact.EmailAddress = v_user.EmailID;
                ViewBag.IsEmailReadOnly = true;
                var userprofile = db.UserProfileDetails.SingleOrDefault(m => m.UserID == userId);
                if (userprofile != null)
                    ViewBag.profilePicture = userprofile.ProfilePicture;

                return View(contact);

            }
            return View();
        }

        [HttpPost]
        public ActionResult ContactUs(ContactU contact)
        {
            ViewBag.IsEmailReadOnly = true;
            ViewBag.IsRegister = true;
            if (!ModelState.IsValid)
            {
                return View();
            }
            try
            {
                MailMessage mail = new MailMessage("marketplacenote@gmail.com", "jaybhanderi866@gmail.com");
                mail.Subject = contact.FullName + " - " + contact.Subject;
                string Body = "Hello,<br><br>" + contact.Comments + "<br><br>Regards,<br>" + contact.FullName;
                mail.Body = Body;
                mail.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential("marketplacenote@gmail.com", "Note@123");
                smtp.EnableSsl = true;
                smtp.Send(mail);

                return RedirectToAction("ContactUs","Home");
            }
            catch (Exception)
            {
                return View();
            }

        }

        public ActionResult SearchNote(int? page, string search, string type, string category, string university, string course, string country, string rating)
        {
            ViewBag.IsRegister = false;
            if (userId != 0)
            {
                ViewBag.IsRegister = true;
                var userprofile = db.UserProfileDetails.SingleOrDefault(m => m.UserID == userId);
                if (userprofile != null)
                    ViewBag.profilePicture = userprofile.ProfilePicture;
            }
            List<NoteDetail> notedetail = db.NoteDetails.Where(m=>m.NoteStatus == 7 && m.IsActive == true).ToList();
            if (!string.IsNullOrEmpty(search))
            {
                notedetail = notedetail.Where(m => m.Title.ToLower().StartsWith(search.ToLower())).ToList();
            }
            if (!string.IsNullOrEmpty(type))
            {
                notedetail = notedetail.Where(m => m.NoteType1.Name != null && m.NoteType1.Name.Equals(type) ).ToList();
            }
            if (!string.IsNullOrEmpty(category))
            {
                notedetail = notedetail.Where(m =>m.NoteCategory.Name !=null && m.NoteCategory.Name.Equals(category)).ToList();
            }
            if (!string.IsNullOrEmpty(university))
            {
                notedetail = notedetail.Where(m => m.InstitutionName != null && m.InstitutionName.ToLower() == university.ToLower()).ToList();
            }
            if (!string.IsNullOrEmpty(course))
            {
                notedetail = notedetail.Where(m =>m.Course != null && m.Course.ToLower() == course.ToLower()).ToList();
            }
            if (!string.IsNullOrEmpty(country))
            {
                notedetail = notedetail.Where(m => m.Country1.Name != null && m.Country1.Name.Equals(country)).ToList();
            }
            if (!string.IsNullOrEmpty(rating))
            {
                List<NoteDetail> newList = new List<NoteDetail>();
                foreach (var notes in notedetail)
                {
                    int avg = 0;
                    var abc = db.Feedbacks.Where(m => m.NoteID == notes.NoteID);
                    if (abc != null)
                    {
                        var count = abc.ToList().Count();
                        if (count != 0)
                        {
                            avg = abc.ToList().Sum(m => m.Ratings) / count;
                        }
                    }
                    if (avg >= int.Parse(rating))
                    {
                        newList.Add(notes);
                    }
                }
                notedetail = newList;
            }

            ViewBag.noteCount = notedetail.Count();

            return View(notedetail.ToPagedList(page ?? 1, 9));
        }

        public ActionResult UserProfile()
        {
            if (userId != 0)
            {
                var userprofile = db.UserProfileDetails.SingleOrDefault(m => m.UserID == userId);
                if (userprofile != null)
                {
                    userprofile.CountryCode = userprofile.PhoneNumber.Substring(0, 1);
                    userprofile.PhoneNumber = userprofile.PhoneNumber.Substring(2);
                    ViewBag.profilePicture = userprofile.ProfilePicture;
                }
                else
                    userprofile = new UserProfileDetail();

                User v_user = db.Users.SingleOrDefault(m => m.UserId == userId);
                userprofile.User = v_user;
                List<Country> country = db.Countries.ToList();
                ViewBag.country = country;
                return View(userprofile);
            }
            return RedirectToAction("Login", "Home");
        }

        [HttpPost]
        public ActionResult UserProfile(UserProfileDetail userdetail)
        {

            List<Country> country = db.Countries.ToList();
            ViewBag.country = country;
            var userprofile = db.UserProfileDetails.FirstOrDefault(m => m.UserID == userId);

            if (userprofile != null)
                ViewBag.profilePicture = userprofile.ProfilePicture;


            // var errors = ModelState.Where(x => x.Value.Errors.Count > 0).Select(x => new { x.Key, x.Value.Errors }).ToArray();

            if (userdetail.Country == "")
            {
                return View(userdetail);
            }
            try
            {

                User v_user = db.Users.FirstOrDefault(m => m.UserId == userId);
                v_user.FirstName = userdetail.User.FirstName;
                v_user.LastName = userdetail.User.LastName;
                v_user.Password2 = v_user.Password;
                v_user.IsDetailsSubmitted = true;
                v_user.IsActive = true;

                if (userdetail.Gender.ToLower() == "select your gender")
                    userdetail.Gender = "";
                userdetail.UserID = userId;
                userdetail.PhoneNumber = userdetail.CountryCode + userdetail.PhoneNumber;
                userdetail.ModifiedDate = DateTime.Now.Date;
                userdetail.IsActive = true;
                userdetail.User = v_user;


                if (userdetail.File != null)
                {
                    var AllowedExtentions = new[] { ".Jpg", ".png", ".jpg", ".jpeg" };
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

                if (db.UserProfileDetails.SingleOrDefault(m => m.UserID == userId) == null)
                    db.UserProfileDetails.Add(userdetail);
                else
                {
                    UserProfileDetail v_userdetail = db.UserProfileDetails.FirstOrDefault(m => m.UserID == userId);
                    v_userdetail.DOB = userdetail.DOB;
                    v_userdetail.Gender = userdetail.Gender;
                    v_userdetail.Address1 = userdetail.Address1;
                    v_userdetail.Address2 = userdetail.Address2;
                    v_userdetail.City = userdetail.City;
                    v_userdetail.State = userdetail.State;
                    v_userdetail.ZipCode = userdetail.ZipCode;
                    v_userdetail.Country = userdetail.Country;
                    v_userdetail.CountryCode = userdetail.CountryCode;
                    v_userdetail.PhoneNumber = userdetail.PhoneNumber;
                    v_userdetail.College = userdetail.College;
                    v_userdetail.University = userdetail.University;
                    v_userdetail.ModifiedDate = DateTime.Now;
                    v_userdetail.IsActive = true;
                    v_userdetail.UserID = userdetail.UserID;
                    v_userdetail.User = userdetail.User;
                    if (userdetail.File != null)
                    {
                        v_userdetail.ProfilePicture = userdetail.ProfilePicture;
                    }
                }

                db.SaveChanges();

                return RedirectToAction("SearchNote", "Home");

            }
            catch (Exception)
            {
                return View(userdetail);
            }
        }

        public ActionResult LogOut()
        {
            userId = 0;
            ViewBag.isRegister = false;
            return RedirectToAction("Home", "Home");

        }

        public ActionResult Dashboard()
        {
            if (userId == 0)
            {
                return RedirectToAction("Login", "Home");
            }
            var userprofile = db.UserProfileDetails.SingleOrDefault(m => m.UserID == userId);
            if (userprofile != null)
                ViewBag.profilePicture = userprofile.ProfilePicture;

            List<NoteDetail> notes = db.NoteDetails.Where(m => m.UserID == userId && m.IsActive == true).ToList();
            int count = 0;
            foreach (var note in notes)
            {
                NoteStatu status = db.NoteStatus.SingleOrDefault(m => m.NotesStatusID == note.NoteStatus);
                count += db.NoteDetails.Where(m => m.UserID == userId && status.Name == "Rejected").Count();
            }

            List<BuyerRequest> BuyerRequests = db.BuyerRequests.Where(m => m.SellerID == userId && m.Status == true).ToList();
            int count2 = 0;
            foreach (var noteReq in BuyerRequests)
            {
                if (noteReq != null)
                    count2 += (int)db.NoteDetails.SingleOrDefault(m => m.UserID == userId && m.NoteID == noteReq.NoteID).SellPrice;
            }

            ViewBag.SoldNotes = db.BuyerRequests.Where(m => m.SellerID == userId && m.Status == true).Count();
            ViewBag.moneyEarned = count2;
            ViewBag.downloads = db.BuyerRequests.Where(m => m.BuyerID == userId && m.Status == true).Count();
            ViewBag.rejected = count;
            ViewBag.buyerRequest = db.BuyerRequests.Where(m => m.SellerID == userId && m.Status == false).Count();

            return View();
        }

        public static string date1 = "Asc";
        public static string title1 = "Asc";
        public static string category1 = "Asc";
        public static string status1 = "Asc";

        public ActionResult InProgress(int? page, string search, string sortBy, string noteId)
        {
            if (userId == 0)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                List<NoteDetail> notedetails = db.NoteDetails.Where(m => m.UserID == userId && m.IsActive == true && m.NoteStatus != 7).OrderByDescending(m => m.CreatedDate).ToList();
                if (!string.IsNullOrEmpty(noteId))
                {
                    NoteDetail notes = notedetails.FirstOrDefault(m => m.NoteID.ToString().Equals(noteId.ToString()));
                    if (notes != null)
                    {
                        notes.IsActive = false;

                    }
                    db.SaveChanges();
                    notedetails = db.NoteDetails.Where(m => m.UserID == userId && m.IsActive == true && ( m.NoteStatus == 1 || m.NoteStatus == 2 || m.NoteStatus == 4 )).OrderByDescending(m => m.CreatedDate).ToList();

                }
                if (!string.IsNullOrEmpty(search))
                {
                    notedetails = notedetails.Where(m => m.Title.ToLower().StartsWith(search.ToLower()) || m.NoteCategory.Name.ToLower().StartsWith(search.ToLower()) || m.NoteStatu.Name.ToLower().Equals(search.ToLower())).ToList();
                }

                if (!string.IsNullOrEmpty(sortBy))
                {
                    if (sortBy == "date")
                    {
                        if(date1 == "Asc")
                        {
                            notedetails = notedetails.OrderBy(m => m.CreatedDate).ToList();
                            date1 = "Desc";
                        }
                        else
                        {
                            notedetails = notedetails.OrderByDescending(m => m.CreatedDate).ToList();
                            date1 = "Asc";
                        }

                    }
                    else if (sortBy == "title")
                    {
                        if (title1 == "Asc")
                        {
                            notedetails = notedetails.OrderBy(m => m.Title).ToList();
                            title1 = "Desc";
                        }
                        else
                        {
                            notedetails = notedetails.OrderByDescending(m => m.Title).ToList();
                            title1 = "Asc";
                        }
                        
                    }
                    else if (sortBy == "category")
                    {
                        if (category1 == "Asc")
                        {
                            notedetails = notedetails.OrderBy(m => m.NoteCategory.Name).ToList();
                            category1 = "Desc";
                        }
                        else
                        {
                            notedetails = notedetails.OrderByDescending(m => m.NoteCategory.Name).ToList();
                            category1 = "Asc";
                        }
                    }
                    else if (sortBy == "status")
                    {
                        if (status1 == "Asc")
                        {
                            notedetails = notedetails.OrderBy(m => m.NoteStatu.Name).ToList();
                            status1 = "Desc";
                        }
                        else
                        {
                            notedetails = notedetails.OrderByDescending(m => m.NoteStatu.Name).ToList();
                            status1 = "Asc";
                        }
                    }
                }
                return PartialView(notedetails.ToPagedList(page ?? 1, 5));
            }
            
        }

        public static string date2 = "Asc";
        public static string title2 = "Asc";
        public static string category2 = "Asc";
        public static string selltype = "Asc";
        public static string price = "Asc";
        public ActionResult Published(int? page2, string search2, string sortBy2)
        {
            if(userId == 0)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                List<NoteDetail> notedetails = db.NoteDetails.Where(m => m.UserID == userId && m.IsActive == true && m.NoteStatus == 7).OrderByDescending(m => m.CreatedDate).ToList();
                if (!string.IsNullOrEmpty(search2))
                {
                    notedetails = notedetails.Where(m => m.Title.ToLower().StartsWith(search2.ToLower()) || m.NoteCategory.Name.ToLower().StartsWith(search2.ToLower()) || m.SellPrice.ToString().Equals(search2.ToString())).ToList();
                }

                if (!string.IsNullOrEmpty(sortBy2))
                {
                    if (sortBy2 == "date")
                    {
                        if (date2 == "Asc")
                        {
                            notedetails = notedetails.OrderBy(m => m.PublishedDate).ToList();
                            date2 = "Desc";
                        }
                        else
                        {
                            notedetails = notedetails.OrderByDescending(m => m.PublishedDate).ToList();
                            date2 = "Asc";
                        }
                    }
                    else if (sortBy2 == "title")
                    {
                        if (title2 == "Asc")
                        {
                            notedetails = notedetails.OrderBy(m => m.Title).ToList();
                            title2 = "Desc";
                        }
                        else
                        {
                            notedetails = notedetails.OrderByDescending(m => m.Title).ToList();
                            title2 = "Asc";
                        }
                    }
                    else if (sortBy2 == "category")
                    {
                        if (category2 == "Asc")
                        {
                            notedetails = notedetails.OrderBy(m => m.NoteCategory.Name).ToList();
                            category2 = "Desc";
                        }
                        else
                        {
                            notedetails = notedetails.OrderByDescending(m => m.NoteCategory.Name).ToList();
                            category2 = "Asc";
                        }
                    }
                    else if (sortBy2 == "selltype")
                    {
                        if (selltype == "Asc")
                        {
                            notedetails = notedetails.OrderBy(m => m.NoteType1.Name).ToList();
                            selltype = "Desc";
                        }
                        else
                        {
                            notedetails = notedetails.OrderByDescending(m => m.NoteType1.Name).ToList();
                            selltype = "Asc";
                        }
                    }
                    else if (sortBy2 == "price")
                    {
                        if (price == "Asc")
                        {
                            notedetails = notedetails.OrderBy(m => m.SellPrice).ToList();
                            price = "Desc";
                        }
                        else
                        {
                            notedetails = notedetails.OrderByDescending(m => m.SellPrice).ToList();
                            price = "Asc";
                        }
                    }
                }
                return PartialView(notedetails.ToPagedList(page2 ?? 1, 5));

            }
            
        }

        public ActionResult AddNote(string noteId)
        {
            if (userId == 0)
            {
                return RedirectToAction("Login", "Home");
            }
            var userprofile = db.UserProfileDetails.SingleOrDefault(m => m.UserID == userId);
            if (userprofile != null)
                ViewBag.profilePicture = userprofile.ProfilePicture;

            NoteDetail noteDetail = new NoteDetail();
            if (!string.IsNullOrEmpty(noteId))
            {
                noteDetail = db.NoteDetails.SingleOrDefault(m => m.NoteID.ToString().Equals(noteId));

                if (noteDetail.UserID != userId)
                {
                    return HttpNotFound();
                }

            }

            List<string> categories = db.NoteCategories.Select(m => m.Name).Distinct().ToList();
            List<string> types = db.NoteTypes.Select(m => m.Name).Distinct().ToList();
            List<string> countries = db.Countries.Select(m => m.Name).Distinct().ToList();
            ViewBag.category = categories;
            ViewBag.type = types;
            ViewBag.country = countries;

            return View(noteDetail);
        }

        public string title;

        [HttpPost]
        public ActionResult AddNote(NoteDetail noteDetail, string submit)
        {
            switch (submit)
            {
                case "save":
                    noteDetail.NoteStatus = 1 ;
                    break;
                case "publish":
                    noteDetail.NoteStatus = 2;
                    break;
            }
            

            User user = db.Users.SingleOrDefault(m => m.UserId == userId);
            NoteAttachement noteAttachement = new NoteAttachement();
            noteDetail.UserID = userId;
            noteDetail.IsActive = true;
            noteDetail.CreatedDate = DateTime.Now;
            noteDetail.CreatedBy = userId;
            noteDetail.ModifiedDate = DateTime.Now;
            noteDetail.ModifiedBy = userId;
            noteDetail.User = user;

            title = noteDetail.Title;

            if (db.NoteDetails.Where(m => m.Title.ToLower().Equals(noteDetail.Title.ToLower())).Count() != 0 && noteDetail.NoteID == 0)
            {
                ModelState.AddModelError("Title", "This title is already exists!");
                return View(noteDetail);
            }
            if (noteDetail.PictureFile != null)
            {
             

                var AllowedExtentions = new[] { ".Jpg", ".png", ".jpg", ".jpeg" };
                string extension = Path.GetExtension(noteDetail.PictureFile.FileName);
                if (AllowedExtentions.Contains(extension))
                {

                    if (noteDetail.PictureFile.ContentLength < 10 * 1024 * 1024)
                    {
                        WebImage img = new WebImage(noteDetail.PictureFile.InputStream);
                        if (img.Height > 253)
                            img.Resize(450, 253);
                        string fileName = userId.ToString() + "NotePic" + noteDetail.NoteID + DateTime.Now.ToString("yyyyMMddHHmmssfff") + extension;
                        string path = Path.Combine(Server.MapPath("~/Uploads/BookPicture/"), fileName);
                        img.Save(path);
                        noteDetail.BookPicture = fileName;
                    }
                    else
                    {
                        ViewBag.fileMsg = "file size is morethan 10 MB.";
                        return View(noteDetail);
                    }
                }
                else
                {
                    ViewBag.fileMsg = "Please choose only Image file";
                    return View(noteDetail);
                }
            }
            else
            {
                noteDetail.BookPicture = "default.jpg";
            }
            if (noteDetail.NoteFile != null)
            {
                string extension = Path.GetExtension(noteDetail.NoteFile.FileName);
                if (extension.ToLower().Equals(".pdf"))
                {
                    string notefileName = userId.ToString() + "Note" + noteDetail.NoteID + DateTime.Now.ToString("yyyyMMddHHmmssfff") + extension;
                    string notepath = Path.Combine(Server.MapPath("~/Uploads/Notes/"), notefileName);
                    noteDetail.NoteFile.SaveAs(notepath);
                    
                    noteAttachement.FileName = notefileName;
                    noteAttachement.FilePath = notepath;
                    noteAttachement.IsActive = true;
                   // noteAttachement.NoteDetail = noteDetail;
                    noteAttachement.ModifiedBy = noteDetail.UserID;
                    noteAttachement.ModifiedDate = DateTime.Now;
                    noteDetail.NoteSize = noteDetail.NoteFile.ContentLength;
  
                }
                else
                {
                    ViewBag.fileMsg = "File must be in PDF only!";
                    return View(noteDetail);
                }
            }
            else
            {
                ViewBag.fileMsg = "The Upload Notes field is required.";
                return View(noteDetail);
            }


            if (noteDetail.PreviewFile != null)
            {
                string extension = Path.GetExtension(noteDetail.PreviewFile.FileName);
                if (extension.Equals(".pdf"))
                {
                    string fileName = userId.ToString() + "NotePreview" + noteDetail.NoteID + DateTime.Now.ToString("yyyyMMddHHmmssfff") + extension;
                    string path = Path.Combine(Server.MapPath("~/Uploads/NotePreview"), fileName);
                    noteDetail.PreviewFile.SaveAs(path);
                    noteDetail.NotePreview = fileName;
                }
                else
                {
                    ViewBag.previewMsg = "File must be in PDF only!";
                    return View(noteDetail);
                }
            }
            else if (noteDetail.IsPaid == true)
            {
                if (noteDetail.SellPrice == null)
                {
                    ViewBag.sellpriceMsg = "SellPrice is required";
                    return View(noteDetail);
                }
                    
                if(noteDetail.PreviewFile == null)
                {
                    ViewBag.previewMsg = "Note preview is required!";
                    return View(noteDetail);
                }
               
            }
            else if(noteDetail.IsPaid==false)
            {
                noteDetail.SellPrice = 0;
            }


           try
           {
                if (noteDetail.NoteStatus == 2)
                {
                    User v_user = db.Users.SingleOrDefault(m => m.UserId == userId);
                    MailMessage mail = new MailMessage("marketplacenote@gmail.com", "jaybhanderi866@gmail.com");//,user.EmailID.ToString());               
                    mail.Subject = v_user.FirstName + " sent his note for review";
                    string Body = "Hello Admins,<br><br>We want to inform you that, " + v_user.FirstName + " sent his note<br>" + noteDetail.Title + " for review. Please look at the notes and take required actions.<br><br>Regards,<br>Notes Marketplace";

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

                if (noteDetail.NoteID == 0)
                {
                    db.NoteDetails.Add(noteDetail);

                    db.SaveChanges();

                    NoteDetail note = db.NoteDetails.SingleOrDefault(m => m.Title.ToString().Equals(title));
                    noteAttachement.NoteID = note.NoteID;

                    db.NoteAttachements.Add(noteAttachement);

                    db.SaveChanges();
                }
                else
                {
                    NoteDetail noteDetail1 = db.NoteDetails.SingleOrDefault(m => m.NoteID == noteDetail.NoteID);

                   
                    noteDetail1.Title = noteDetail.Title;
                    noteDetail1.Category = noteDetail.Category;
                    noteDetail1.BookPicture = noteDetail.BookPicture;
                    noteDetail1.NoteFile = noteDetail.NoteFile;
                    noteDetail1.NoteType = noteDetail.NoteType;
                    noteDetail1.NumberOfPages = noteDetail.NumberOfPages;
                    noteDetail1.NotesDescription = noteDetail.NotesDescription;
                    noteDetail1.InstitutionName = noteDetail.InstitutionName;
                    noteDetail1.Country = noteDetail.Country;
                    noteDetail1.Course = noteDetail.Course;
                    noteDetail1.CourseCode = noteDetail.CourseCode;
                    noteDetail1.Professor = noteDetail.Professor;
                    noteDetail1.SellPrice = noteDetail.SellPrice;
                    noteDetail1.NotePreview = noteDetail.NotePreview;
                    noteDetail1.NoteSize = noteDetail.NoteSize;
                    noteDetail1.PublishedDate = DateTime.Now;
                    noteDetail1.NoteStatus = 2;
                    noteDetail1.CreatedDate = noteDetail.CreatedDate;
                    noteDetail1.ModifiedDate = DateTime.Now;
                    noteDetail1.IsActive = noteDetail.IsActive;
                    noteDetail1.IsPaid = noteDetail.IsPaid;
                    

                    
                    NoteAttachement notAttachement = db.NoteAttachements.SingleOrDefault(m => m.NoteID == noteDetail1.NoteID);

                    notAttachement.FileName = noteAttachement.FileName;
                    notAttachement.FilePath = noteAttachement.FilePath;
                    notAttachement.IsActive = true;
                    notAttachement.ModifiedBy = noteDetail.UserID;
                    notAttachement.ModifiedDate = DateTime.Now;

                    db.SaveChanges();
                }
                
                return RedirectToAction("Dashboard", "Home");
           }
           catch (Exception)
           {
                return View(noteDetail);
           }
        }

        public ActionResult NoteDetail(string noteId, string download)
        {
            ViewBag.isRegister = false;
            NoteDetail notedetail = db.NoteDetails.SingleOrDefault(m => m.NoteID.ToString().Equals(noteId) && m.IsActive == true);
            if (string.IsNullOrEmpty(noteId))
            {
                return HttpNotFound();
            }
            if (userId != 0)
            {
                ViewBag.isRegister = true;
                var userprofile = db.UserProfileDetails.SingleOrDefault(m => m.UserID == userId);
                if (userprofile != null)
                    ViewBag.profilePicture = userprofile.ProfilePicture;

            }
            int avg = 0, count = 0;
            var abc = db.Feedbacks.Where(m => m.NoteID == notedetail.NoteID).ToList();
            if (abc != null)
            {
                count = abc.Count();
                if (count != 0)
                {
                    avg = abc.ToList().Sum(m => m.Ratings ) / count;
                }
            }


            ViewBag.Review = count;
            ViewBag.Rating = avg;
            ViewBag.Spam = db.SpamReports.Where(m => m.NoteID == notedetail.NoteID).Count();

            List<Feedback> noteReviews = db.Feedbacks.Where(m => m.NoteID.ToString().Equals(noteId) && m.IsActive == true).ToList();
           
            ViewBag.Customer = noteReviews.OrderByDescending(m => m.Ratings);



            if (notedetail.IsPaid == false)
                ViewBag.btnMsg = "Download";
            else
                ViewBag.btnMsg = "Download/$" + notedetail.SellPrice.ToString();

            var found = db.BuyerRequests.SingleOrDefault(m => m.BuyerID == userId && m.NoteID == notedetail.NoteID);
            var approved = db.BuyerRequests.SingleOrDefault(m => m.BuyerID == userId && m.NoteID == notedetail.NoteID && m.Status == true);

            if (string.IsNullOrEmpty(download))
            {
                if (found != null && approved == null)
                {
                    ViewBag.btnMsg = "Requested";
                }
                //...
            }
            else
            {
                NoteAttachement Downloadnote = db.NoteAttachements.SingleOrDefault(m => m.NoteID == notedetail.NoteID);

                if (userId == 0)
                {
                    return RedirectToAction("Login", "Home");
                }
                if (userId == notedetail.UserID)
                {
                    return RedirectToAction("DownloadFile", "Home", new { filename = Downloadnote.FileName });
                }

                if (found != null)
                {
                    if (approved != null)
                    {
                        return RedirectToAction("DownloadFile", "Home", new { filename = Downloadnote.FileName });
                    }
                    else
                    {
                        ViewBag.btnMsg = "Requested";
                    }
                }
                else
                {
                    BuyerRequest noteRequest = new BuyerRequest();
                    noteRequest.BuyerID = userId;
                    noteRequest.SellerID = notedetail.UserID;
                    noteRequest.NoteID = notedetail.NoteID;
                    noteRequest.NoteTitle = notedetail.Title;
                    noteRequest.Price = (int)notedetail.SellPrice;
                    noteRequest.BuyerEmailID = db.Users.SingleOrDefault(m=>m.UserId == userId).EmailID;
                    noteRequest.PhoneNumber = db.UserProfileDetails.SingleOrDefault(m => m.UserID == userId).PhoneNumber;
                    noteRequest.SellType = (notedetail.IsPaid?"Paid":"Free");
                    noteRequest.ApprovedDate = DateTime.Now;

                    if (notedetail.SellPrice == 0)
                    {
                        noteRequest.Status = true;
                        db.BuyerRequests.Add(noteRequest);
                        db.SaveChanges();
                        return RedirectToAction("DownloadFile", "Home", new { filename = Downloadnote.FileName }); ;
                    }
                    else if (notedetail.SellPrice != 0)
                    {
                        noteRequest.Status = false;
                        noteRequest.IsActive = true;
                        noteRequest.NoteTitle = notedetail.Title;
                        
                        db.BuyerRequests.Add(noteRequest);
                        db.SaveChanges();
                        ViewBag.popup = true;
                        ViewBag.btnMsg = "Requested";

                        User user = db.Users.SingleOrDefault(m => m.UserId == notedetail.UserID);
                        var username = db.Users.SingleOrDefault(m => m.UserId == userId);
                        var sellername = user.FirstName + " " + user.LastName;
                        ViewBag.username = username.FirstName;
                        ViewBag.sellername = sellername;

                        try
                        {
                            MailMessage mail = new MailMessage("marketplacenote@gmail.com", "jaybhanderi866@gmail.com");//,user.EmailID.ToString());               
                            mail.Subject = username.FirstName + " " + username.LastName + " wants to purchase your notes ";
                            string Body = "Hello " + sellername + ",<br><br>We would like to inform you that, " + username.FirstName + " " + username.LastName + " wants to purchase your notes. Please see <br> Buyer Requests tab and allow download access to Buyer if you have received the payment from<br>him.<br><br>Regards,<br>Notes Marketplace";

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
                            return View(notedetail);
                        }
                    }
                }
            }

            return View(notedetail);
        }

        public ActionResult BuyerRequest(int? page, string search, string sortBy , string noteId , string buyerId)
        {
            if (userId == 0)
            {
                return RedirectToAction("Login", "Home");
            }
            var userprofile = db.UserProfileDetails.SingleOrDefault(m => m.UserID == userId);
            if (userprofile != null)
                ViewBag.profilePicture = userprofile.ProfilePicture;

            if (!string.IsNullOrEmpty(noteId))
            {
                BuyerRequest noteRequest = db.BuyerRequests.SingleOrDefault(m => m.NoteID.ToString().Equals(noteId) && m.BuyerID.ToString().Equals(buyerId));
                if (noteRequest != null)
                {
                    noteRequest.Status = true;
                    db.SaveChanges();
                    try
                    {
                        User buyer = db.Users.SingleOrDefault(m => m.UserId == userId);
                        User seller = db.Users.SingleOrDefault(m => m.UserId == noteRequest.SellerID);
                        MailMessage mail = new MailMessage("marketplacenote@gmail.com", "jaybhanderi866@gmail.com");//,user.EmailID.ToString());               
                        mail.Subject = seller.FirstName + " " + seller.LastName + " Allows you to download a note";
                        string Body = "Hello " + buyer.FirstName + " " + buyer.LastName + ",<br>We would like to inform you that," + seller.FirstName + " " + seller.LastName + " Allows you to download a note.<br>Please login and see My Download tabs to download particular note.<br><br>Regards,<br>Notes Marketplace";

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
            }

            
            List<BuyerRequest> noteRequests = db.BuyerRequests.Where(m => m.SellerID == userId && m.Status == false).ToList();

            

            noteRequests = noteRequests.OrderByDescending(m => m.ApprovedDate).ToList();
            if (!string.IsNullOrEmpty(search))
                noteRequests = noteRequests.Where(m => m.NoteTitle.ToLower().StartsWith(search.ToLower()) || m.Category.ToLower().StartsWith(search.ToLower()) || m.BuyerEmailID.ToLower().StartsWith(search.ToLower()) || m.PhoneNumber.ToLower().StartsWith(search.ToLower()) || m.Price.ToString().Equals(search.ToLower())).ToList();
            if (!string.IsNullOrEmpty(sortBy))
            {
                if (sortBy.Equals("title"))
                {
                    noteRequests = noteRequests.OrderBy(m => m.NoteTitle).ToList();
                }
                else if (sortBy.Equals("category"))
                {
                    noteRequests = noteRequests.OrderBy(m => m.Category).ToList();
                }
                else if (sortBy.Equals("buyer"))
                {
                    noteRequests = noteRequests.OrderBy(m => m.BuyerEmailID).ToList();
                }
                else if (sortBy.Equals("PhoneNumber"))
                {
                    noteRequests = noteRequests.OrderBy(m => m.PhoneNumber).ToList();
                }
                else if (sortBy.Equals("price"))
                {
                    noteRequests = noteRequests.OrderBy(m => m.Price).ToList();
                }
                else if (sortBy.Equals("download"))
                {
                    noteRequests = noteRequests.OrderBy(m => m.ApprovedDate).ToList();
                }
            }
            return View(noteRequests.ToPagedList(page ?? 1, 10));
        }

        public FileResult DownloadFile(string filename)
        {
            string path = Path.Combine(Server.MapPath("~/Uploads/Notes/")) + filename;
            byte[] fileBytes = System.IO.File.ReadAllBytes(path);
            ViewBag.btnMsg = "Requested";
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, filename);
        }

        public ActionResult DownloadNote(int? page, int? noteId, string search, string sortBy, Feedback noteReview, SpamReport spamReport)
        {
            if (userId == 0)
            {
                return RedirectToAction("Login", "Home");
            }
            var userprofile = db.UserProfileDetails.SingleOrDefault(m => m.UserID == userId);
            if (userprofile != null)
                ViewBag.profilePicture = userprofile.ProfilePicture;

            List<BuyerRequest> noteRequests = new List<BuyerRequest>();
            List<BuyerRequest> noteRequests1 = db.BuyerRequests.Where(m => m.BuyerID == userId && m.Status == true).ToList();
            NoteAttachement noteAttachement = db.NoteAttachements.SingleOrDefault(m => m.NoteID == noteId);
            foreach (var notes in noteRequests1)
            {
                BuyerRequest noteRequestsObj = new BuyerRequest();
                NoteDetail noteDetails = db.NoteDetails.SingleOrDefault(m => m.NoteID == notes.NoteID);
                noteRequestsObj.NoteID = noteDetails.NoteID;
                noteRequestsObj.NoteTitle = noteDetails.Title;
                noteRequestsObj.Category = noteDetails.NoteCategory.Name;
                noteRequestsObj.Price = (int)noteDetails.SellPrice;
                noteRequestsObj.ApprovedDate = notes.ApprovedDate;
                noteRequestsObj.BuyerEmailID = db.Users.SingleOrDefault(m => m.UserId == notes.BuyerID).EmailID;

                if (noteRequestsObj.Price == 0)
                    noteRequestsObj.SellType = "Free";
                else
                    noteRequestsObj.SellType = "Paid";

                noteRequests.Add(noteRequestsObj);
            }

            noteRequests = noteRequests.OrderByDescending(m => m.ApprovedDate).ToList();
            if (!string.IsNullOrEmpty(search))
                noteRequests = noteRequests.Where(m => m.NoteTitle.ToLower().StartsWith(search.ToLower()) || m.Category.ToLower().StartsWith(search.ToLower()) || m.BuyerEmailID.ToLower().StartsWith(search.ToLower()) || m.Price.ToString().Equals(search.ToLower())).ToList();
            if (!string.IsNullOrEmpty(sortBy))
            {
                if (sortBy.Equals("title"))
                {
                    noteRequests = noteRequests.OrderBy(m => m.NoteTitle).ToList();
                }
                else if (sortBy.Equals("category"))
                {
                    noteRequests = noteRequests.OrderBy(m => m.Category).ToList();
                }
                else if (sortBy.Equals("buyer"))
                {
                    noteRequests = noteRequests.OrderBy(m => m.BuyerEmailID).ToList();
                }
                else if (sortBy.Equals("price"))
                {
                    noteRequests = noteRequests.OrderBy(m => m.Price).ToList();
                }
                else if (sortBy.Equals("download"))
                {
                    noteRequests = noteRequests.OrderBy(m => m.ApprovedDate).ToList();
                }
            }

            if (noteId != 0)
            {
                var notedetail = db.NoteDetails.FirstOrDefault(m => m.NoteID == noteId);
                if (notedetail != null)
                {
                    NoteAttachement note = db.NoteAttachements.SingleOrDefault(m => m.NoteID == noteId);
                    return RedirectToAction("DownloadFile", "Home", new { filename = note.FileName });
                }

            }
            Download download = new Download();
            download.pagedList = noteRequests.ToPagedList(page ?? 1, 10);

            try
            {
                if (spamReport.NoteID != 0)
                {
                    SpamReport spamReport1 = db.SpamReports.SingleOrDefault(m => m.UserID == userId && m.NoteID == spamReport.NoteID);
                    if (spamReport1 != null)
                    {
                        spamReport1.Remark = spamReport.Remark;
                        spamReport1.IsActive = true;
                        spamReport1.ModifiedDate = DateTime.Now;
                        spamReport1.ModifiedBy = userId;
                        
                        db.SaveChanges();
                    }
                    else
                    {
                        spamReport.UserID = userId;
                        spamReport.IsActive = true;
                        spamReport.CreatedDate = DateTime.Now;
                        spamReport.CreatedBy = userId;
                        db.SpamReports.Add(spamReport);
                        db.SaveChanges();
                    }
                }

                if (noteReview.NoteID != 0)
                {
                    Feedback noteReview1 = db.Feedbacks.SingleOrDefault(m => m.UserID == userId && m.NoteID == noteReview.NoteID);
                    if (noteReview1 != null)
                    {
                        noteReview1.Ratings = noteReview.Ratings;
                        noteReview1.Comments = noteReview.Comments;
                        noteReview1.ModifiedDate = DateTime.Now;
                        db.SaveChanges();
                    }
                    else
                    {
                        noteReview.UserID = userId;
                        noteReview.IsActive = true;
                        noteReview.CreatedDate = DateTime.Now;
                        db.Feedbacks.Add(noteReview);
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception)
            {
                return View(download);
            }

            return View(download);
        }

        public ActionResult RejectedNote(int? page, int? noteId, string search, string sortBy)
        {
            if (userId != 0)
            {
                var userprofile = db.UserProfileDetails.SingleOrDefault(m => m.UserID == userId);
                if (userprofile != null)
                    ViewBag.profilePicture = userprofile.ProfilePicture;

                if (noteId != 0)
                {
                    var notes1 = db.NoteAttachements.FirstOrDefault(m => m.NoteID == noteId);

                    if (notes1 != null)
                    {
                        return RedirectToAction("DownloadFile", "Home", new { filename = notes1.FileName });
                    }

                }

                List<NoteDetail> notedetail = db.NoteDetails.Where(m => m.UserID == userId && m.IsActive == true && m.NoteStatu.Name == "Rejected").ToList();
                
                if (!string.IsNullOrEmpty(search))
                {
                    notedetail = notedetail.Where(m => m.Title.ToLower().StartsWith(search.ToLower()) || m.NoteCategory.Name.ToLower().Equals(search.ToLower())).ToList();
                }
                if (!string.IsNullOrEmpty(sortBy))
                {
                    if (sortBy == "title")
                        notedetail = notedetail.OrderBy(m => m.Title).ToList();
                    else if (sortBy == "category")
                        notedetail = notedetail.OrderBy(m => m.Category).ToList();
                }
                return View(notedetail.ToPagedList(page ?? 1, 10));
            }
            else
            {
                return RedirectToAction("Login", "Home");
            }
        }

        public ActionResult SoldNote(int? page, string search, string sortBy, string noteId)
        {
            if (userId == 0)
            {
                return RedirectToAction("Login", "Home");
            }
            var userprofile = db.UserProfileDetails.SingleOrDefault(m => m.UserID == userId);
            if (userprofile != null)
                ViewBag.profilePicture = userprofile.ProfilePicture;

            if (!string.IsNullOrEmpty(noteId))
            {
                var notedetail = db.NoteAttachements.FirstOrDefault(m => m.NoteID.ToString().Equals(noteId));
                if (notedetail != null)
                {
                    return RedirectToAction("DownloadFile", "Home", new { filename = notedetail.FileName });
                }
            }

            List<BuyerRequest> noteRequests = new List<BuyerRequest>();
            List<BuyerRequest> noteRequests1 = db.BuyerRequests.Where(m => m.SellerID == userId && m.BuyerID != userId && m.Status == true).ToList();

            foreach (var notes in noteRequests1)
            {
                var noteRequestsObj = new BuyerRequest();
                NoteDetail noteDetails = db.NoteDetails.SingleOrDefault(m => m.NoteID == notes.NoteID);
                noteRequestsObj.NoteID = noteDetails.NoteID;
                noteRequestsObj.NoteTitle = noteDetails.Title;
                noteRequestsObj.Category = noteDetails.NoteCategory.Name;
                noteRequestsObj.Price = (int)noteDetails.SellPrice;
                noteRequestsObj.ApprovedDate = notes.ApprovedDate;
                noteRequestsObj.BuyerEmailID = db.Users.SingleOrDefault(m => m.UserId == notes.BuyerID).EmailID;

                if (noteRequestsObj.Price == 0)
                    noteRequestsObj.SellType = "Free";
                else
                    noteRequestsObj.SellType = "Paid";

                noteRequests.Add(noteRequestsObj);
            }

            noteRequests = noteRequests.OrderByDescending(m => m.ApprovedDate).ToList();
            if (!string.IsNullOrEmpty(search))
                noteRequests = noteRequests.Where(m => m.NoteTitle.ToLower().StartsWith(search.ToLower()) || m.Category.ToLower().StartsWith(search.ToLower()) || m.BuyerEmailID.ToLower().StartsWith(search.ToLower()) || m.Price.ToString().Equals(search.ToLower())).ToList();
            if (!string.IsNullOrEmpty(sortBy))
            {
                if (sortBy.Equals("title"))
                {
                    noteRequests = noteRequests.OrderBy(m => m.NoteTitle).ToList();
                }
                else if (sortBy.Equals("category"))
                {
                    noteRequests = noteRequests.OrderBy(m => m.Category).ToList();
                }
                else if (sortBy.Equals("buyer"))
                {
                    noteRequests = noteRequests.OrderBy(m => m.BuyerEmailID).ToList();
                }
                else if (sortBy.Equals("price"))
                {
                    noteRequests = noteRequests.OrderBy(m => m.Price).ToList();
                }
                else if (sortBy.Equals("download"))
                {
                    noteRequests = noteRequests.OrderBy(m => m.ApprovedDate).ToList();
                }
            }

            return View(noteRequests.ToPagedList(page ?? 1, 10));
        }

        public ActionResult ChangePassword()
        {
            if (userId == 0)
            {
                return RedirectToAction("Login", "Home");
            }
            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(ChangePassword changePassword)
        {
            if (userId == 0)
            {
                return RedirectToAction("Login", "Home");
            }
            if (!ModelState.IsValid)
            {
                return View();
            }
            User v_user = db.Users.SingleOrDefault(m => m.UserId == userId && m.Password.Equals(changePassword.Password1));
            try
            {
                if (v_user != null)
                {
                    v_user.Password = changePassword.Password2;
                    v_user.Password2 = changePassword.Password2;
                    v_user.UserId = userId;
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
    }
}