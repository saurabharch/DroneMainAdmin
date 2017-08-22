using DroneMainAdmin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DroneMainAdmin.Models;
using System.Net.Mail;
using System.Net;

namespace DroneMainAdmin.Controllers
{
    public class PassResetController : Controller
    {
        // GET: PassReset
        [Authorize]
        public ActionResult ForgetPass()
        {
            return View();
        }



        [NonAction]
        public bool IsEmailExist(string emailID)
        {
            using (Entities de = new Entities())
            {
                var EC = de.Users.Where(a => a.EmailID == emailID).FirstOrDefault();
                return EC != null;// if not equal to null means True
            }
        }
      

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ForgetPassChange(string id)
        {
            bool Status = false;
            string message = "";
            using (Entities dc = new Entities())
            {
                dc.Configuration.ValidateOnSaveEnabled = false; // Avoid Confirmation password does not match on save changes
                var v = dc.Users.Where(a => a.ActivationCode == new Guid(id)).FirstOrDefault();
                if (v != null)
                {
                    v.IsEmailVerified = true;
                    var changepass = v.Password;
                    v.Password = id;
                    v.ActivationCode = Guid.NewGuid();
                    dc.SaveChanges();
                    SendChangePassword(v.EmailID, v.ActivationCode.ToString(), changepass.ToString());
                    Status = true;
                    message = "Succefully Change Your Password";
                }
                else
                {
                    message = "Invalid Request";
                    Status = false;
                }

            }
            ViewBag.Message = message;
            ViewBag.Status = Status;
            return View();
        }
        [NonAction]
        public void SendChangePassword(string emailID, string activationcode, string userpass)
        {
            var verifyUrl = "/PassReset/ForgetPass/" + activationcode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);
            var fromEmail = new MailAddress("2advpost@gmail.com", "🔴 DroneFest");
            var toEmail = new MailAddress(emailID);
            var fromEmailPassword = "~~~Kash007";
            string subject = "🔴 DroneFest Account Password 🔴";
            string body = "<br/><br/>Your Drone Fest Account Password is : " + link;
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