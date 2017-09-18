using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DroneMainAdmin.Models;
using System.Net.Mail;
using System.Net;
using System.Web.Security;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Data.Entity.Validation;
using System.Text;

namespace DroneMainAdmin.Controllers
{
    public class UserController : Controller
    {
        // Registration Action
            bool stateflag=false;
        [HttpGet]
        public ActionResult Registration()
        {
            DroneDBEntities drop = new DroneDBEntities();
            ViewBag.SelectValue = new SelectList(drop.countries,"id","name");
            ViewBag.StateSelect = new SelectList(drop.states,"state_id","name");
            //   ViewBag.StateSelect = "";
            stateflag = true;
            return View();
        }
        string countryname;
        [HttpGet]
        public JsonResult GetStateById(int ID)
        {
            DroneDBEntities ds = new DroneDBEntities();
            ds.Configuration.ProxyCreationEnabled = false;
            stateflag = false;
            return Json(ds.states.Where(p => p.country_id == ID), JsonRequestBehavior.AllowGet);
        }
        //Registration Post Action
        string con,date;
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration([Bind(Exclude = "IsEmailVerified,ActivationCode,AdminType")]DroneMainAdmin.Models.User user)
        {
            bool Status = false;
            string message = "";
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
               // user.UserID = user.UserID;
                #endregion
                #region Generate Activation Code
                user.ActivationCode = Guid.NewGuid();
                #endregion
                #region Password Hashing
                user.Password = Crypto.Hash(user.Password);
                user.ConfirmPassword = Crypto.Hash(user.ConfirmPassword);
                DroneDBEntities drop = new DroneDBEntities();
                int county = Convert.ToInt16(user.CountryName);
                int stat = Convert.ToInt16(user.Provinance);
                //int county= Convert.ToInt16(user.CountryName);
                var nam= drop.countries.Where(p => p.id == county).FirstOrDefault();
                var sat = drop.states.Where(x => x.state_id == stat).FirstOrDefault();
                user.CountryName = nam.name;
                user.Provinance = sat.name;
                user.IsEmailVerified = false;
                if(user.SubEmail)
                {
                    user.SubEmail = true;
                }
                else
                {
                    user.SubEmail = false;
                }
                if(user.TermsAccepted)
                {
                    user.TermsAccepted = true;
                }
                else
                {
                    user.TermsAccepted = false;
                }
                user.Provinance = user.Provinance;
                user.CountryName = user.CountryName;
                #endregion
                #region Save Data to Database
                using (DroneDBEntities dc = new DroneDBEntities())
                {
                    dc.Users.Add(user);
                    try
                    {
                        dc.SaveChanges();
                        SendVerificationLinkEmail(user.EmailID, user.ActivationCode.ToString());
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
                    return RedirectToAction("Registration", "User");

                }
                #endregion

            }
            else
            {
                message = "Invalid Request";
            }

            ViewBag.Message = message;
            ViewBag.Status = Status;
            return View(user);
        }

        // Verify Email 

        // Verify Email Link
        [HttpGet]
        public ActionResult VerifyAccount(string id)
        {
            bool Status = false;
            string message = "";
            try
            {
                using (DroneDBEntities dc = new DroneDBEntities())
                {
                    dc.Configuration.ValidateOnSaveEnabled = false; // Avoid Confirmation password does not match on save changes
                    var v = dc.Users.Where(a => a.ActivationCode == new Guid(id)).FirstOrDefault();
                    if (v != null)
                    {
                        v.IsEmailVerified = true;
                        v.ActivationCode = Guid.NewGuid();
                        dc.SaveChanges();
                        Status = true;
                        message = "Succefully Verified Your Email account and your account is Activate Now";
                    }
                    else
                    {
                        message = "Invalid Request";
                        Status = false;
                    }

                }
            }
            catch (Exception)
            {
                message = "Invalid Request";
                Status = false;

            }
            
            ViewBag.Message = message;
            ViewBag.Status = Status;
            return View();
        }
        //Login
        [HttpGet]
        public ActionResult ForgetPassChange(string id)
        {
            bool Status = false;
            string message = "";
            try
            {
                using (DroneDBEntities dc = new DroneDBEntities())
                {
                    dc.Configuration.ValidateOnSaveEnabled = false; // Avoid Confirmation password does not match on save changes
                    var v = dc.Users.Where(a => a.ActivationCode == new Guid(id)).FirstOrDefault();
                    if (v != null)
                    {
                        v.IsEmailVerified = true;
                        var changepass = v.Password;
                        v.Password = rnd.Next(0003000, 99999999).ToString();
                        v.ActivationCode = Guid.NewGuid();
                        dc.SaveChanges();
                        SendChangePassword(v.EmailID, v.ActivationCode.ToString(), "");
                        Status = true;
                        message = "Succefully Change Your Password";
                    }
                    else
                    {
                        message = "Invalid Request";
                        Status = false;
                    }

                }
            }
            catch (Exception)
            {

                message = "Invalid Request";
                Status = false;
            }
            ViewBag.Message = message;
            ViewBag.Status = Status;
            return RedirectToAction("Login","User");
        }
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }
        // Login Post
        public async Task<ActionResult> Login(DroneMainAdmin.Models.UserLogin login, string ReturnUrl = "")
        {
            bool Status = false;
            string message = "";
            using (DroneDBEntities dc = new DroneDBEntities())
            {
                var v = dc.Users.Where(a => a.EmailID == login.EmailID).FirstOrDefault();
                if (v != null)
                {
                    if (string.Compare(Crypto.Hash(login.Password), v.Password) == 0)
                    {
                        int timeout = login.RememberMe ? 525600 : 20;// 525600 minute = 1 year here timeout time is 20 min
                        var ticket = new FormsAuthenticationTicket(login.EmailID, login.RememberMe, timeout);
                        string encrypted = FormsAuthentication.Encrypt(ticket);
                        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                        cookie.Expires = DateTime.Now.AddMinutes(timeout);
                        cookie.HttpOnly = true;
                        Response.Cookies.Add(cookie);
                        if (v.AdminType && v.IsEmailVerified)
                        {
                            Status = true;
                            // return 
                            message = "Successfully Login";
                           return RedirectToAction("Index", "TeamList");
                        }
                        else if (v.IsEmailVerified)
                        {
                            Status = true;
                            message = "Successfully Login";
                            return RedirectToAction("Index", "Home");

                        }
                        else if (Url.IsLocalUrl(ReturnUrl))
                        {
                            message = "Redirect To Home";
                            return Redirect(ReturnUrl);
                        }
                    }
                    else
                    {
                        Status = false;
                        message = "Invalid Credential Provided ";
                    }

                }
                else
                {
                    message = "Invalid Credential Provided ";
                }
            }
            ViewBag.Status =Status;
            ViewBag.Message = message;
            return View();
        }
        // Bulk Email Sender

        //Log Out
        [Authorize]
        [HttpPost]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "User");
        }

        [HttpGet]
        public ActionResult ForgetPassword()
        {
            return View();
        }
        Random rnd = new Random();
        [HttpPost]
        public ActionResult ForgetPassword(User user,string emailID, string activationcode,string oldpass)
            {
            bool Status = false;
            string message = "";
            if (!ModelState.IsValid)
            {



                Random rnd = new Random();
                #region //Email is already Exist Check
                var isExist = IsEmailExist(user.EmailID);
                if (isExist)
                {
                    using (DroneDBEntities dc = new DroneDBEntities())
                    {
                        dc.Configuration.ValidateOnSaveEnabled = false;
                        var EC = dc.Users.Where(a => a.EmailID == emailID).FirstOrDefault();
                        
                        if(EC!=null)
                        {
                            try
                            {
                                    EC.GlobalID = Guid.NewGuid();
                                    dc.SaveChanges();
                                    ForgetPassChange(EC.GlobalID.ToString());
                                    message = "Forget Password Link has been successfully Sent To your Email Account Please Check Your Email Account:  " + user.EmailID;
                                    Status = true;
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                      else
                        {
                            message = "Invalid Request";
                            Status = false;
                        }
                        //Send Email to Users
                        return RedirectToAction("Login", "User");

                    }
                }
                
            }
            return View();
        }
        [HttpGet]
        public ActionResult ChangePass()
        {
            ViewBag.Status = false;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePass(DroneMainAdmin.Models.User user, string emailID, string activationcode, string oldpass,string id)
        {
            bool status = false;
            string message = "";
            using (DroneDBEntities dc = new DroneDBEntities())
            {
                dc.Configuration.ValidateOnSaveEnabled = false;// Avoid Confirmation password does not match on save changes
                try
                {
                     var v = dc.Users.Where(a => a.GlobalID == new Guid(id)).FirstOrDefault();
                    if (v != null)
                    {
                        var changePass = user.Password;
                        changePass = Crypto.Hash(user.Password);
                        v.GlobalID = Guid.NewGuid();
                        v.Password = changePass;
                        dc.SaveChanges();
                        status = true;
                        message = "Your Account Password is changed and your password has been sent to your registered Email address.Please Check your email id for updated new password .";
                        SendChangePassword(v.EmailID, "", user.Password.ToString());
                    }
                    else
                    {
                        message = "Invalid Request";
                        status = false;
                    }

                }
                catch (Exception)
                {

                    message = "Invalid Request";
                    status = false;

                }
                            }
            ViewBag.Message = message;
            ViewBag.Status = status;
            return View();
           
        }
        string sendText;
        [NonAction]
        public void SendChangePassword(string emailID, string activationcode,string userpass)
        {
            var verifyUrl = "/User/ChangePass/" + activationcode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);
            var fromEmail = new MailAddress("2advpost@gmail.com", "🔴 DroneFest");
            var toEmail = new MailAddress(emailID);
            var fromEmailPassword = "~~~Kash007";
            string subject = "🔴 DroneFest Account Password 🔴";
            
            if((userpass!="")&& (userpass.Length>=8))
            {
                sendText = "<br/><br/>Your Drone Fest Account Password is : " + userpass;
            }
            else
            {
                sendText = "<br/><br/>Your Click Here For Generate New Password : " + link;
            }
            string body = sendText.ToString();
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
        [NonAction]
        public bool IsEmailExist(string emailID)
        {
            using (DroneDBEntities de = new DroneDBEntities())
            {
                var EC = de.Users.Where(a => a.EmailID == emailID).FirstOrDefault();
                return EC != null;// if not equal to null means True
            }
        }

        [NonAction]
        public void SendVerificationLinkEmail(string emailID, string activationcode)
        {
            var verifyUrl = "/User/VerifyAccount/" + activationcode;
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
                                 "successfully created. Please click on the below Button or Link for verify your Eamil account</p>" +
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
    }
}
#endregion