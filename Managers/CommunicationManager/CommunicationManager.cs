using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Diagnostics.CodeAnalysis;
using System.Web.Security;
using System.Net;
using System.IO;
using System.Web.Razor;
using RazorEngine;
using System.Web;
using Accessor;
using Entity;
using System.Data.Entity;
using System.Data;
using System.Data.Linq;
using SendGridMail;
using SendGridMail.Transport;

namespace Manager
{
    public class CommunicationManager
    {
        LogAccessor logAccessor = new LogAccessor();
        public string TestMe()
        {
            return "success";
        }

        [ExcludeFromCodeCoverage]
        public bool SendTestEmail()
        {

            SendMessage("vestnteam@vestn.com", "noreply@vestn.com", new String[1] { "vestnteam@gmail.com" }, null, null, "SendTestEmail", "SendTestEmail Successful!");

            return true;
        }

        [ExcludeFromCodeCoverage]
        private string generateEmailBody(string template, VerifyEmailModel model)
        {
            try
            {
                return Razor.Parse(template, model);
            }
            catch (Exception e)
            {

                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), e.ToString());
                return e.Message;
            }
        }

        [ExcludeFromCodeCoverage]
        private void SendMessage(string mailFrom, string mailFromDisplayName, string[] mailTo, string[] mailCc, string[] mailBcc, string subject, string body)
        {
            SmtpClient smtpClient = new SmtpClient("smtp.sendgrid.net", Convert.ToInt32(587));
            System.Net.NetworkCredential credentials = new System.Net.NetworkCredential("vestn", "V3stn.com!");
            smtpClient.Credentials = credentials;

            string to = mailTo != null ? string.Join(",", mailTo) : null;
            string cc = mailCc != null ? string.Join(",", mailCc) : null;

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(mailFrom, mailFromDisplayName);
            mail.To.Add(to);

            if (cc != null)
            {
                mail.CC.Add(cc);
            }

            mail.Subject = subject;
            mail.Body = body.Replace(Environment.NewLine, "<BR>");
            mail.IsBodyHtml = true;

            smtpClient.Send(mail);
        }

        [ExcludeFromCodeCoverage]
        public bool SendVerificationMail(Guid guid, string name, string email)
        {
            try
            {
                string newString = HttpContext.Current.Request.Url.Authority.ToString();
                string guidstring = guid.ToString();

                VerifyEmailModel verifyEmailModel = new VerifyEmailModel()
                {
                    Name = name,
                    GuidString = guid.ToString(),
                    ClientUrl = "http://" + HttpContext.Current.Request.Url.Authority.ToString()
                };


                String messageBody = generateEmailBody(Manager.Properties.Resources.VerifyEmailTemplate.ToString(), verifyEmailModel);

                SendMessage("vestnteam@vestn.com", "Vestn", new String[1] { email }, null, null, "Verify Your Email to Create Your Vestn Account!", messageBody);

                return true;
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());

                return false;
            }
        }

        [ExcludeFromCodeCoverage]
        public bool SendForgotPasswordEmail(string name, string email, string resetPasswordHash)
        {
            try
            {
                ForgotPasswordModel forgotPasswordModel = new ForgotPasswordModel()
                {
                    Name = name,
                    ResetPasswordHash = resetPasswordHash,
                    ClientUrl = "http://" + HttpContext.Current.Request.Url.Authority.ToString()
                };

                String messageBody = generateEmailBody(Manager.Properties.Resources.ForgotPasswordTemplate.ToString(), forgotPasswordModel);

                SendMessage("vestnteam@vestn.com", "noreply@vestn.com", new String[1] { email }, null, null, "Reset Your Vestn Account Password!", messageBody);

                return true;
            }
            catch (Exception e)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), e.ToString());
                return false;
            }
        }
        [ExcludeFromCodeCoverage]
        public bool SendErrorEmail(string username, string message)
        {
            try
            {
                String messageBody = message;

                SendMessage("vestnteam@vestn.com", "noreply@vestn.com", new String[1] { "vestnteam@gmail.com" }, null, null, "ERROR!", "Username : "+username+"  "+messageBody);

                return true;
            }
            catch (Exception e)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), e.ToString());
                return false;
            }
        }

        [ExcludeFromCodeCoverage]
        public bool SendShareEmail(string link, string email, string senderName)
        {
            try
            {
                ShareModel shareModel = new ShareModel()
                {
                    Name = senderName,
                    Link = link
                };

                
                String messageBody = generateEmailBody(Manager.Properties.Resources.ShareTemplate.ToString(), shareModel);


                SendMessage("vestnteam@vestn.com", "noreply@vestn.com", new String[1] { email }, null, null, "Someone wants to share their Vestn profile with you!", messageBody);

                return true;
            }
            catch (Exception e)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), e.ToString());
                return false;
            }
        }

        [ExcludeFromCodeCoverage]
        public bool SendHelpRequestEmail(string useremail, string message)
        {
            try
            {
                String messageBody = message;

                SendMessage("vestnteam@vestn.com", "noreply@vestn.com", new String[1] { "vestnteam@gmail.com" }, null, null, "Help request!", "User email: " + useremail + " Message: " + messageBody);

                return true;
            }
            catch (Exception e)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), e.ToString());
                return false;
            }
        }


        [ExcludeFromCodeCoverage]
        public bool SendSiteFeedbackEmail(string name, string useremail, string message)
        {
            try
            {
                String messageBody = message;

                SendMessage("vestnteam@vestn.com", "noreply@vestn.com", new String[1] { "connerdana@vestn.com" }, null, null, "Feedback from " + name, "User name: " + name + " User email: " + useremail + " Message: " + messageBody);

                return true;
            }
            catch (Exception e)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), e.ToString());
                return false;
            }
        }

        private string generateEmailBody(string template, ForgotPasswordModel model)
        {
            try
            {
                return Razor.Parse(template, model);
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return ex.Message;
            }
        }

        public bool SendInviteFriendEmail(string userFirstName, string friendName, string friendEmail)
        {
            try
            {

                InviteFriendModel inviteFriendModel = new InviteFriendModel()
                {
                    UserFirstName = userFirstName,
                    FriendName = friendName
                };

                String messageBody = generateEmailBody(Manager.Properties.Resources.InviteFriendTemplate.ToString(), inviteFriendModel);
                SendMessage("vestn@vestn.com", "Vestn", new String[1] { friendEmail }, null, null, userFirstName + " wants you to join Vestn", messageBody);
                return true;
            }
            catch (Exception e)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), e.ToString());
                return false;
            }
        }

        private string generateEmailBody(string template, InviteFriendModel model)
        {
            try
            {
                return Razor.Parse(template, model);
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return ex.Message;
            }
        }

        public bool SendFeedbackToUserEmail(string userFirstName, string friendFirstName, string friendEmail, string feedback)
        {
            try
            {

                UserFeedbackModel userFeedbackModel = new UserFeedbackModel()
                {
                    UserFirstName = userFirstName,
                    FriendFirstName = friendFirstName,
                    Feedback = feedback
                };

                String messageBody = generateEmailBody(Manager.Properties.Resources.UserFeedbackTemplate.ToString(), userFeedbackModel);
                SendMessage("vestn@vestn.com", "Vestn", new String[1] { friendEmail }, null, null, userFirstName + " left you feedback on your Vestn profile", messageBody);
                return true;
            }
            catch (Exception e)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), e.ToString());
                return false;
            }
        }

        private string generateEmailBody(string template, UserFeedbackModel model)
        {
            try
            {
                return Razor.Parse(template, model);
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return ex.Message;
            }
        }

        private string generateEmailBody(string template, ShareModel model)
        {
            try
            {
                return Razor.Parse(template, model);
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return ex.Message;
            }
        }

        public bool SendRequestBetaKeyEmail(string requesterEmail)
        {
            //check to see if friendEmail is already associated with an account.
            //TODO CURRENTLY NOT IMPLEMENTED
            //IMLEMENTED IN USERMANAGER.CheckDuplicateEmail

            try
            {

                RequestBetaKeyModel requestBetaKeyModel = new RequestBetaKeyModel()
                {
                    //currently we don't need any user specific data for this...
                };

                String messageBody = generateEmailBody(Manager.Properties.Resources.RequestBetaKeyTemplate.ToString(), requestBetaKeyModel);

                SendMessage("vestnteam@gmail.com", "noreply@vestn.com", new String[1] { requesterEmail }, null, null, "Your Vestn Beta Key has Arrived!", messageBody);
                return true;
            }
            catch (Exception e)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), e.ToString());
                return false;
            }
        }

        private string generateEmailBody(string template, RequestBetaKeyModel model)
        {
            try
            {
                return Razor.Parse(template, model);
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), ex.ToString());
                return ex.Message;
            }
        }

    }
}
