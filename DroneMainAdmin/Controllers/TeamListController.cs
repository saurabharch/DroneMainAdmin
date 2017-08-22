using DroneMainAdmin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Newtonsoft.Json;
using DroneMainAdmin.Models;
using System.Text;
using System.Data.Entity.Validation;

namespace DroneMainAdmin.Controllers
{
    public class TeamListController : Controller
    {
        private DroneDBEntities dbe = new DroneDBEntities();
        // GET: /User/
        [Authorize]
        [HttpGet]
        public ActionResult Index()
        {
            if(User.Identity.IsAuthenticated)
            {
               // ViewBag.Message = "Welcome to administation panel";
            }
           
            return View();

        }

        // GET: /User/Details/5
        public ActionResult Details(int id = 0)
        {
            User user = dbe.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        [HttpGet]
        public JsonResult GetTeams()
        {
            using (DroneDBEntities dc = new DroneDBEntities())
            {
                List<User> userList = dc.Users.ToList<User>();
                ViewBag.Message = "Data Loading..";
                //var team = dc.Users.OrderBy(a => a.FirstName).ToList();
                return Json(userList, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /User/Create
        public ActionResult Create()
        {
            return PartialView();
        }

        // POST: /User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(User user, string Command)
        {
            if (ModelState.IsValid)
            {
                dbe.Users.Add(user);
                dbe.SaveChanges();
                TempData["Msg"] = "Data has been saved succeessfully";
                return RedirectToAction("Index");
            }

            return View(user);
        }

        // GET: /User/Edit/5
        [HttpGet]
        public ActionResult Edit([Bind(Exclude = "ConfirmPassword")]DroneMainAdmin.Models.User user, int id)
        {
            DroneDBEntities drop = new DroneDBEntities();
            if (User.Identity.IsAuthenticated)
            {
                string username = User.Identity.Name;
                var v = drop.Users.Where(a => a.UserID == id).FirstOrDefault();
                try
                {
                    var adminchk = drop.Users.Where(a => a.EmailID == username).FirstOrDefault();
                   if (v != null && adminchk.AdminType == true)
                    {
                        string selected = (from sub in drop.Users
                                           where sub.UserID == id
                                           select sub.CountryName).FirstOrDefault();
                        ViewBag.SelectValue = new SelectList(drop.countries, "name", "name", selected);
                        string stateselect = (from sub in drop.Users
                                              where sub.UserID == id
                                              select sub.Provinance).FirstOrDefault();
                        ViewBag.StateSelect = new SelectList(drop.states, "name", "name", stateselect);
                        ViewBag.DateSet = (from sub in drop.Users
                                           where sub.UserID == id
                                           select sub.DateOfBirth).FirstOrDefault();
                        user = dbe.Users.Find(id);
                    }
                   else
                    {
                        ViewBag.Message = "You are Not Authorize for View This Page";
                        return RedirectToAction("Index");
                    }
                }
                catch(Exception ex)
                {
                   
                    
                }
            }
            return View(user);
        }

        // POST: /User/Edit/5
       
        [HttpPost] 
        [ActionName("Edit")]
        //[Bind(Exclude = "")]
        public ActionResult Edit(int id,[Bind(Exclude = "ConfirmPassword")]DroneMainAdmin.Models.User user)
        {
            bool status = false;
            string message = "";
            if (User.Identity.IsAuthenticated)
            {
                if (!ModelState.IsValid)
                {
                    using (DroneDBEntities db = new DroneDBEntities())
                    {
                        string username = User.Identity.Name;
                        var v = dbe.Users.Where(a => a.UserID == id).FirstOrDefault();
                        try
                        {
                            var adminchk = dbe.Users.Where(a => a.EmailID == username).FirstOrDefault();
                            if (v != null && adminchk.AdminType == true)

                                db.Configuration.ValidateOnSaveEnabled = false; // Avoid Confirmation password does not match on save changes

                            if (v==null && adminchk.AdminType == true)
                            {
                                v.FirstName = user.FirstName;
                                v.MiddleName = user.MiddleName;
                                v.LastName = user.LastName;
                                v.EmailID = user.EmailID;
                                v.ContactNo = user.ContactNo;
                                v.TeamName = user.TeamName;
                                if (user.Password.Length <= 42)
                                {
                                    v.Password = Crypto.Hash(user.Password);
                                }
                                // v.Password = user.Password;
                                //  v.ConfirmPassword = user.ConfirmPassword;
                                // v.DateOfBirth = user.DateOfBirth;
                                if (user.IsEmailVerified)
                                {
                                    v.IsEmailVerified = true;
                                }
                                else
                                {
                                    v.IsEmailVerified = false;
                                }
                                v.CountryName = user.CountryName;
                                if (user.SubEmail)
                                {
                                    v.SubEmail = true;
                                }
                                else
                                {
                                    v.SubEmail = false;
                                }
                                v.Decscript = user.Decscript;
                                if (user.AdminType == true)
                                {
                                    v.AdminType = true;
                                }
                                else
                                {
                                    v.AdminType = false;
                                }
                                try
                                {
                                    db.Entry(User).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                }
                                catch (DbEntityValidationException ex)
                                {
                                    StringBuilder sb = new StringBuilder();
                                    foreach (var eve in ex.EntityValidationErrors)
                                    {
                                        sb.AppendLine(string.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                                                        eve.Entry.Entity.GetType().Name,
                                                                        eve.Entry.State));
                                        foreach (var ve in eve.ValidationErrors)
                                        {
                                            sb.AppendLine(string.Format("- Property: \"{0}\", Error: \"{1}\"",
                                                                        ve.PropertyName,
                                                                        ve.ErrorMessage));
                                        }
                                    }
                                    message = "Error Message  :" + ex;

                                }
                                status = true;
                                ViewBag.Message = "User Profile is Succesfully Updated";
                            }
                            else
                            {
                                ViewBag.Message = "You are not Authorize for doing changes in User Record,\n for more contact administrator";
                            }
                        }
                        catch { }
                        }
                }
            }
            return RedirectToAction("Index", "TeamList");

        }
               
        // GET: /User/Delete/5
        //public ActionResult Delete(int id)
        //{
        //    bool status = false;
        //    using (DroneDBEntities dbe = new DroneDBEntities())
        //    {
        //        var v = dbe.Users.Where(a => a.UserID == id).FirstOrDefault();
        //        if (v != null)
        //        {
        //            dbe.Users.Remove(v);
        //            dbe.SaveChanges();
        //            status = true;
        //            ViewBag.Message = " ";
        //        }
        //    }
        //    return Json(new { success = true, messgae = "Delete Successfully" }, JsonRequestBehavior.AllowGet);
        //}

        protected override void Dispose(bool disposing)
        {
            dbe.Dispose();
            base.Dispose(disposing);
        }

        [Authorize]
        [HttpGet]
        public ActionResult Save(int id)
        {
            
                using (DroneDBEntities dbe = new DroneDBEntities())
                {
                    var v = dbe.Users.Where(a => a.UserID == id).FirstOrDefault();
                    return View(v);
                }
        }
        Random rnd = new Random();
        [Authorize]
        [HttpPost]
        public ActionResult Save([Bind(Exclude = "ConfirmPassword")]DroneMainAdmin.Models.User user)
        {
            bool status = false;
            string message = "";
            if(ModelState.IsValid)
            {
                using (DroneDBEntities db = new DroneDBEntities())
                {
                    
                        var v = db.Users.Where(a => a.UserID == user.UserID).FirstOrDefault();
                        if(v!=null)
                        {
                            v.FirstName = user.FirstName;
                            v.MiddleName = user.MiddleName;
                            v.LastName = user.LastName;
                            v.EmailID = user.EmailID;
                            v.ContactNo = user.ContactNo;
                            v.TeamName = user.TeamName;
                        // v.DateOfBirth = user.DateOfBirth;
                            if(user.IsEmailVerified)
                            {
                                v.IsEmailVerified = true;
                            }
                            else
                            {
                                v.IsEmailVerified = false;
                            }
                            v.CountryName = user.CountryName;
                            if (user.SubEmail)
                            {
                                v.SubEmail = true;
                            }
                            else
                            {
                                v.SubEmail = false;
                            }
                            v.Decscript = user.Decscript;
                            if(user.AdminType == true)
                           {
                              v.AdminType = true;
                           }
                           else
                           {
                            v.AdminType = false;
                           }
                        try
                        {
                            db.Entry(User).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        catch (Exception error)
                        {

                           
                        }
                            status = true;
                            message = "User Profile is Succesfully Updated";
                        }

                   else if(v==null)
                   {

                        
                        //Model Validation
                        if (ModelState.IsValid)
                        {

                            #region //Email is already Exist Check
                            var isExist = IsEmailExist(user.EmailID);
                            if (isExist)
                            {
                                ModelState.AddModelError("EmailExist", "Email is already Exist");
                                return View(user);
                            }
                            #endregion
                            #region Generate Activation Code
                            user.ActivationCode = Guid.NewGuid();
                            #endregion
                            #region Password Hashing
                            string passw = rnd.Next(000092,999999).ToString();
                            user.Password = passw;
                            user.Password = Crypto.Hash(passw);
                            user.ConfirmPassword = Crypto.Hash(passw);
                            user.IsEmailVerified = true;
                            #endregion
                            #region Save Data to Database
                            using (Entities dc = new Entities())
                            {
                                dc.Users.Add(user);
                                try
                                {
                                    dc.SaveChanges();
                                    SendVerificationLinkEmail(user.EmailID, user.ActivationCode.ToString(),passw);
                                    message = "Registration is successfully done. Account activation link " +
                                        " has been sent to your email id : " + user.EmailID+
                                        "and your Password is :"+passw;
                                    status = true;

                                }
                                catch (Exception ex)
                                {

                                }
                                //Send Email to Users
                                return RedirectToAction("Index", "TeamList");

                            }
                            #endregion

                        }
                        else
                        {
                            message = "Invalid Request";
                        }
                    }
                    else
                    {
                        status = false;
                        message = "Invalid Request";
                    }
                    
                }
            }
            return new JsonResult { Data = new { status = status } };
        }
        public bool IsEmailExist(string emailID)
        {
            using (DroneDBEntities de = new DroneDBEntities())
            {
                var EC = de.Users.Where(a => a.EmailID == emailID).FirstOrDefault();
                return EC != null;// if not equal to null means True
            }
        }

        [NonAction]
        public void SendVerificationLinkEmail(string emailID, string activationcode, string pass)
        {
            var verifyUrl = "/User/ChangePass/" + activationcode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);
            var fromEmail = new MailAddress("2advpost@gmail.com", "Promax Scientific Developer's Team");
            var toEmail = new MailAddress(emailID);
            var fromEmailPassword = "~~~Kash007";
            string subject = "🔴 DroneFest Account is Successfully Created 🔴";
            //string body = "<br/><br/>We are excited to tell you that your Drone Fest account is" +
            //    " successfully created. Please click on the below link to verify your account" +
            //    "<br/><br/><a href='" + link + "'>" + link + "</a>";
            string body = "<meta http-equiv='Content-Type' content='text/html; charset=UTF-8' />" +
                "<meta name='viewport' content='width=device-width; initial-scale=1.0; maximum-scale=1.0;'>" +
                "<title>Notify</title>" +
                "<style type='text/css'>" +
                "div, p, a, li, td { -webkit-text-size-adjust:none; }" +
                ".ReadMsgBody{width: 100%; background-color: #f3f3f3;}" +
                ".ExternalClass{width: 100%; background-color: #f3f3f3;}" +
                "body{width: 100%; height: 100%; background-color: #f3f3f3; margin:0; padding:0; -webkit-font-smoothing: antialiased;}" +
                "html{width: 100%;}@font-face {font-family: 'proxima_nova_softmedium';src: url('http://www.rocketway.net/themebuilder/demo_template/folio/font/mark_simonson_-_proxima_nova_soft_medium-webfont.eot');src: url('http://www.rocketway.net/themebuilder/demo_template/folio/font/mark_simonson_-_proxima_nova_soft_medium-webfont.eot?#iefix') format('embedded-opentype'),url('http://www.rocketway.net/themebuilder/demo_template/folio/font/mark_simonson_-_proxima_nova_soft_medium-webfont.woff') format('woff'),url('http://www.rocketway.net/themebuilder/demo_template/folio/font/mark_simonson_-_proxima_nova_soft_medium-webfont.ttf') format('truetype');font-weight: normal;font-style: normal;" +
                "}" +
                "@font-face {font-family: 'proxima_nova_softregular';src: url('http://www.rocketway.net/themebuilder/demo_template/folio/font/mark_simonson_-_proxima_nova_soft_regular-webfont.eot'); src: url('http://www.rocketway.net/themebuilder/demo_template/folio/font/mark_simonson_-_proxima_nova_soft_regular-webfont.eot?#iefix') format('embedded-opentype'),url('http://www.rocketway.net/themebuilder/demo_template/folio/font/mark_simonson_-_proxima_nova_soft_regular-webfont.woff') format('woff'),url('http://www.rocketway.net/themebuilder/demo_template/folio/font/mark_simonson_-_proxima_nova_soft_regular-webfont.ttf') format('truetype');font-weight: normal;font-style: normal;" +
                "}.hover:hover {opacity:0.90;filter:alpha(opacity=90);}" +
                "</style>" +
                "<table width='100%' border='0' cellpadding='0' cellspacing='0' align='center'>" +
                    "<tr>" +
                        "<td>" +

                            "<table width='960' border='0' cellpadding='0' cellspacing='0' align='center' style='margin-top: 50px; margin-bottom: 100px;'>" +
                                "<tr>" +
                                    "<td width='960'>" +
                                        "<table width='960' border='0' cellpadding='0' cellspacing='0' align='center'>" +
                                            "<tr>" +
                                                "<td width='960' bgcolor='#ffffff' style='border: 1px solid #e7eeee; border-radius: 5px;'>" +
                                                    "<table width='960' border='0' cellpadding='0' cellspacing='0' align='center' style='margin-top: 40px;'>" +
                                                        "<tr>" +
                                                            "<td width='960' style='padding-bottom: 40px; border-bottom: 1px solid #e7eeee;'>" +
                                                                "<center><img src='http://myscienpromaker.com/Content/assets1/images/promaxLogo.png' alt='DroneFest' border='0'></center>" +
                                                            "</td>" +
                                                        "</tr>" +
                                                        "<tr>" +
                                                            "<td width='960' style='font-size: 39px; color: #65707a; text-align: center; font-family:" + "'proxima_nova_softmedium', Helvetica, Arial, sans-serif; line-height: 48px; padding-top: 40px;'>" +
                                                            "Thank you for Registering with Drone Fest Events" +
                                                            "</td>" +
                                                        "</tr>" +
                                                    "</table>" +
                                                    "<table width='960' border='0' cellpadding='0' cellspacing='0' align='center'" +
                                                   "style='margin-top:60px;margin-bottom:60px;'>" +
                                                        "<tr>" +
                                                            "<td width='40'></td>" +
                                                            "<td width='370' style='text-align: center;' valign='top'>" +
                                                                "<p style='font-size: 16px; color: #686868; text-align: center; font-family:" + "'proxima_nova_softmedium', Helvetica, Arial, sans-serif; line-height: 24px;'>" +
                                                                "We are excited to tell you that your Drone Fest account is" +
                                 "successfully created. Please click on the below Button or for change your password and Current Pass is "+pass+" </p>" +
                                 "<p style='margin-bottom:28px;'></p><center>" +
                                 "<img src='http://www.rocketway.net/themebuilder/demo_template/panorama/images/arrow.jpg' alt='' border='0'></center>" +
                                 "<p style='margin-bottom: 5px;'></p><br/><br/><a href='" + link + "' target='_blank' style='background-color: #65707a;" +
                                 "font-family:'proxima_nova_softmedium', Helvetica, Arial, sans-serif; text-decoration: none; color: #ffffff; padding: 10px" +
                                 "20px 10px 20px; border-radius: 4px; font-size: 18px;' class='hover'>Please Verify Email ID</a><p style='margin-bottom:" +
                                 "5px;'><br/></p><a href='" + link + "' target='_blank' style='background-color: #51c4d4; font-family: " + "'proxima_nova_softmedium'" +
                                 ",Helvetica, Arial, sans-serif; text-decoration: none; color: #ffffff; padding: 10px 20px 10px 20px; border-radius: 4px;" +
                                 "font-size: 18px;' class='hover'>Verify Email ID</a>" +
                                                            "</td>" +
                                                            "<td width='546'><center><img" +
                                                            "src='http://myscienpromaker.com/Content/assets1/images/Nexus-6-MockUp-group.png' alt=''" +
                                                            "border='0'></center><center>" +
                                                            "<img src='http://www.rocketway.net/themebuilder/demo_template/panorama/images/shadow.jpg'" +
                                                            "alt='' border='0'></center></td>" +
                                                            "<td width='4'></td>" +
                                                        "</tr>" +
                                                    "</table>" +
                                                    "<table width='960' border='0' cellpadding='0' cellspacing='0' align='center' bgcolor='#65707a'>" +
                                                        "<tr>" +
                                                            "<td width='550' height='100' style='font-size: 16px; color: #ffffff; text-align: right;" + "font-family: 'proxima_nova_softregular', Helvetica, Arial, sans-serif; line-height: 24px;" + "padding-right: 80px;'>" +
                                                            "Thank you for your Connection with Team DroneFest !" +
                                                            "</td>" +
                                                            "<td width='408' height='100' style='font-size: 16px; color: #ffffff; text-align: left;" +
                                                            "font-family:'proxima_nova_softregular', Helvetica, Arial, sans-serif; line-height: 24px;'>" +
                                                            "<a href='mailto:done@drone@dronefest.events' style='color: " +
                                                            "#ffffff;'>drone@dronefest.events</a>" +
                                                            "</td>" +
                                                        "</tr>" +
                                                    "</table>" +
                                                "</td>" +
                                            "</tr></table>";
            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)

            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
                smtp.Send(message);
        }

        [Authorize]
        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeleteUser(int id)
        {
            bool status = false;

            if (User.Identity.IsAuthenticated)
            {
                string username = User.Identity.Name;

                using (DroneDBEntities dbe = new DroneDBEntities())
                {
                    var v = dbe.Users.Where(a => a.UserID == id).FirstOrDefault();
                    try
                    {
                        var adminchk = dbe.Users.Where(a => a.EmailID == username).FirstOrDefault();


                        if (v != null && adminchk.AdminType == true)
                        {
                            dbe.Users.Remove(v);
                            dbe.SaveChanges();
                            status = true;
                            ViewBag.Message = v.EmailID + " User Record Delete Successfully";
                        }
                        else
                        {
                            status = false;
                            ViewBag.Message = "You are Not Authorize for this request";
                        }
                    }
                    catch (Exception)
                    {
                        status = false;
                        ViewBag.Message = "This is Invalid User Request";
                        return RedirectToAction("TeamList");
                    }

                }
            }
            return Json(new { success = status, messgae = ViewBag.Message }, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        [HttpPost]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "User");
        }

        [HttpGet]
        [ActionName("AddOrEdit")]
        public ActionResult AddOrEdit(int id = 0)
        {
            bool status = false;
            DroneDBEntities drop = new DroneDBEntities();
            if (User.Identity.IsAuthenticated)
            {
                string username = User.Identity.Name;
                var v = drop.Users.Where(a => a.UserID == id).FirstOrDefault();
                try
                {
                    var adminchk = drop.Users.Where(a => a.EmailID == username).FirstOrDefault();


                    if (v == null && adminchk.AdminType == true)
                    {

                        if (id == 0)
                        {

                            ViewBag.SelectValue = new SelectList(drop.countries, "id", "name");
                            ViewBag.StateSelect = new SelectList(drop.states, "state_id", "name");
                            ViewBag.Status = true;
                            ViewBag.Message = "Edit " + username + " detailes here";
                            return View(new User());
                        }
                        
                    }
                    else
                    {
                        ViewBag.Status = false;
                        ViewBag.Message = username + " : you are not Authorize for upadate or edit of user Record";
                        return Json(new { success = false, messgae = ViewBag.Message }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch(Exception ex)
                {
                    ViewBag.Status = false;
                    ViewBag.Message = username + " : you are not Authorize for upadate or edit of user Record";
                }
            }
            return View(User);
        }
        [Authorize]
        [HttpPost]
        [ActionName("AddOrEdit")]
        public ActionResult AddOrEdit([Bind(Exclude = "ConfirmPassword")]DroneMainAdmin.Models.User user)
        {
            if (!ModelState.IsValid)
            {
                bool Status = false;
                string message = "";
                //Model Validation
                if (!ModelState.IsValid)
                {

                    #region //Email is already Exist Check
                    var isExist = IsEmailExist(user.EmailID);
                    if (isExist)
                    {
                        ModelState.AddModelError("EmailExist", "Email is already Exist");
                        return View(user);
                    }
                    // user.UserID = user.UserID;
                    #endregion
                    #region Generate Activation Code
                    user.ActivationCode = Guid.NewGuid();
                    #endregion
                    #region Password Hashing
                    string passw = rnd.Next(000092, 999999).ToString();
                    user.Password = passw;
                    user.Password = Crypto.Hash(passw);
                    user.ConfirmPassword = Crypto.Hash(passw);
                    DroneDBEntities drop = new DroneDBEntities();
                    int county = Convert.ToInt16(user.CountryName);
                    int stat = Convert.ToInt16(user.Provinance);
                    //int county= Convert.ToInt16(user.CountryName);
                    var nam = drop.countries.Where(p => p.id == county).FirstOrDefault();
                    var sat = drop.states.Where(x => x.state_id == stat).FirstOrDefault();
                    user.CountryName = nam.name;
                    user.Provinance = sat.name;
                    user.IsEmailVerified = true;
                    if (user.SubEmail)
                    {
                        user.SubEmail = true;
                    }
                    else
                    {
                        user.SubEmail = false;
                    }
                    if (user.TermsAccepted)
                    {
                        user.TermsAccepted = true;
                    }
                    else
                    {
                        user.TermsAccepted = false;
                    }
                    #endregion
                    #region Save Data to Database
                    using (DroneDBEntities dc = new DroneDBEntities())
                    {

                        try
                        {
                            dc.Users.Add(user);
                            dc.SaveChanges();
                            SendVerificationLinkEmail(user.EmailID, user.ActivationCode.ToString(), passw);
                            message = "Registration is successfully done. Account activation link " +
                                " has been sent to your email id : " + user.EmailID;
                            Status = true;

                        }
                        catch (DbEntityValidationException ex)
                        {
                            StringBuilder sb = new StringBuilder();
                            foreach (var eve in ex.EntityValidationErrors)
                            {
                                sb.AppendLine(string.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                                                eve.Entry.Entity.GetType().Name,
                                                                eve.Entry.State));
                                foreach (var ve in eve.ValidationErrors)
                                {
                                    sb.AppendLine(string.Format("- Property: \"{0}\", Error: \"{1}\"",
                                                                ve.PropertyName,
                                                                ve.ErrorMessage));
                                }
                            }
                            message = "Error Message  :" + ex;
                        }
                        //Send Email to Users
                    }
                    #endregion

                }
                else
                {
                    message = "Invalid Request";
                }
                ViewBag.Message = message;
                ViewBag.Status = Status;
                return RedirectToAction("Index", "TeamList");
            }

            return Json(new { success = true, messgae = "Added Successfully" }, JsonRequestBehavior.AllowGet);
        }
    }
}