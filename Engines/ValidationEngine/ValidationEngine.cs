using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;
using Entity;
using Manager;
using System.IO;

namespace Engine
{
    public class ValidationEngine
    {
        public static string Success = "success";

        private static string betaKey = "GBR2012";

        public static string ValidateBetaKey(string testBetaKey)
        {
            try
            {
                if (testBetaKey == null)
                {
                    return "Beta key not entered";
                }

                if (testBetaKey != betaKey)
                {
                    return "Invalid beta key";
                }

                // completed with no errors
            }
            catch (Exception e)
            {
                return "Could not validate beta key";
            }
            return Success;
        }

        public static List<string> getUserEmails()
        {
            List<string> emails = new List<string>();
            UserManager UM = new UserManager();

            foreach (User u in UM.GetAllUsers())
            {
                emails.Add(u.email.ToLower());
            }
            return emails;
        }

        public static List<string> getUserURLs()
        {
            List<string> urls = new List<string>();
            UserManager UM = new UserManager();

            foreach (User u in UM.GetAllUsers())
            {
                urls.Add(u.profileURL.ToLower());
            }
            return urls;
        }

        public static string ValidateFileHasChanged(Stream newFileStream, Stream oldFileStream)
        {
            if (oldFileStream == null)
            {
                return Success;
            }

            byte[] newPictureHash;
            byte[] existingPictureHash;

            using (var newMD5 = new System.Security.Cryptography.MD5CryptoServiceProvider())
            {
                newMD5.ComputeHash(newFileStream);
                newPictureHash = newMD5.Hash;
            }

            using (var existingMD5 = new System.Security.Cryptography.MD5CryptoServiceProvider())
            {
                existingMD5.ComputeHash(oldFileStream);
                existingPictureHash = existingMD5.Hash;
            }

            int j = 0;
            for (j = 0; j < newPictureHash.Length; j++)
            {
                if (newPictureHash[j] != existingPictureHash[j])
                {
                    break;
                }
            }

            if (j == newPictureHash.Length)
            {
                return "The new file was identical to the existing file";
            }

            return Success;
        }

        public static string ValidateFileIsNotInStreamList(Stream newFileStream, List<Stream> existingFileStreams)
        {
            foreach (Stream stream in existingFileStreams)
            {
                if (ValidateFileHasChanged(newFileStream, stream) != Success)
                {
                    return "The new file was identical to another file of the same type in this project";
                }
            }
            return Success;
        }

        public static bool IsDuplicateEmail(string email)
        {
            List<string> emails = getUserEmails();
            if (emails.Contains(email.ToLower()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string ValidateEmail(string email)//breaks if setting to user's current email 
        {

            try
            {
                if (email == null)
                {
                    return "Email not entered";
                }
                string patternLenient = @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";

                string patternStrict = @"^(([^<>()[\]\\.,;:\s@\""]+"
                    + @"(\.[^<>()[\]\\.,;:\s@\""]+)*)|(\"".+\""))@"
                    + @"((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}"
                    + @"\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+"
                    + @"[a-zA-Z]{2,}))$";

                if (!Regex.IsMatch(email, patternLenient))
                {
                    return "Invalid email address.";
                }
                // completed with no errors
            }
            catch (Exception e)
            {
                return "Could not validate email";
            }
            return Success;
        }

        public static string ValidatePassword(string password)
        {
            try
            {
                if (password == null)
                {
                    return "Password not entered";
                }

                string pattern = Membership.PasswordStrengthRegularExpression;

                if (!Regex.IsMatch(password, pattern))
                {
                    return "Invalid password (must be 6 characters long)";
                }

                // completed with no errors
            }
            catch (Exception e)
            {
                return "Could not validate password";
            }
            return Success;
        }

        public static string ValidateUsername(string username)
        {

            try
            {
                if (username == null)
                {
                    return "Username not entered";
                }

                string pattern = "^[a-zA-Z0-9_]*$";

                if (!Regex.IsMatch(username, pattern))
                {
                    return "Invalid username (must only use alphanumeric characters [A-Z, a-z, 0-9] and underscores [_])";
                }

                // completed with no errors
            }
            catch (Exception e)
            {
                return "Could not validate username";
            }
            return Success;
        }

        public static string ValidateBirthdate(DateTime birthdate)
        {

            try
            {
                if (birthdate == null)
                {
                    return "Birthdate not entered";
                }

                if (birthdate.Year < 1800 || 
                    DateTime.Now.Subtract(new TimeSpan(3650, 0, 0, 0)).CompareTo(birthdate) > 0)
                {
                    return "Invalid birthdate (either too young or too old)";
                }

                // completed with no errors
            }
            catch (Exception e)
            {
                return "Could not validate birthdate (must be in MM/DD/YYYY format)";
            }
            return Success;
        }

        public static string ValidateProfileURL(string profileURL)//breaks if setting to user's current URL
        {

            try
            {
                if (profileURL == null | profileURL.Length == 0)
                {
                    return "Profile URL not entered";
                }

                string pattern = "^[a-zA-Z0-9_]*$";
                List<String> urls = getUserURLs();

                if (!Regex.IsMatch(profileURL, pattern))
                {
                    return "Invalid Profile URL (must only use alphanumeric characters [A-Z, a-z, 0-9] and underscores [_])";
                }
                else if (urls.Contains(profileURL.ToLower()))
                {
                    return "URL is currently in use.";
                }

                // completed with no errors
            }
            catch (Exception e)
            {
                return "Could not validate profile URL";
            }
            return Success;
        }

        public static string ValidateDate(DateTime date)
        {

            try
            {
                if (date == null)
                {
                    return "Date not entered";
                }

                if (date.Year < 1800)
                {
                    return "Invalid date (either too long ago)";
                }

                // completed with no errors
            }
            catch (Exception e)
            {
                return "Could not validate date (must be in MM/DD/YYYY format)";
            }
            return Success;
        }

        public static string ValidatePhoneNumber(string phoneNumber)
        {

            try
            {
                if (phoneNumber == null)
                {
                    return "Phone number not entered";
                }

                string pattern = "[0-9]";

                if (!Regex.IsMatch(phoneNumber, pattern))
                {
                    return "Invalid phone number (please remove any punctuation)";
                }

                // completed with no errors
            }
            catch (Exception e)
            {
                return "Could not validate phone number";
            }
            return Success;
        }

        public static int MaximumDocumentSizeInBytes = 100 * (1024 * 1024);
        public static string ValidateDocument(HttpPostedFileBase file)
        {

            try
            {
                if (file == null)
                {
                    return "Document file is empty";
                }

                if (file.ContentType != "application/pdf" &&                                                            //PDF
                    file.ContentType != "application/msword" &&                                                         //DOC
                    file.ContentType != "application/vnd.oasis.opendocument.text" &&                                    //DOCX
                    file.ContentType != "application/vnd.openxmlformats-officedocument.wordprocessingml.document" &&    //DOCX
                    file.ContentType != "text/plain" &&                                                                 //TXT
                    file.ContentType != "application/vnd.ms-excel" &&                                                   //XLS
                    file.ContentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" &&          //XLSX
                    file.ContentType != "application/vnd.ms-powerpoint" &&                                              //PPT, PPS
                    file.ContentType != "application/vnd.openxmlformats-officedocument.presentationml.presentation" &&  //PPTX
                    file.ContentType != "application/vnd.oasis.opendocument.text" &&                                    //ODT
                    file.ContentType != "application/vnd.oasis.opendocument.spreadsheet" &&                             //ODS
                    file.ContentType != "application/vnd.oasis.opendocument.presentation" &&                            //ODP
                    file.ContentType != "application/vnd.sun.xml.writer" &&                                             //SXW
                    file.ContentType != "application/vnd.sun.xml.calc" &&                                               //SXC
                    file.ContentType != "application/vnd.sun.xml.impress.template" &&                                   //SXI
                    file.ContentType != "application/wordperfect" &&                                                    //WPD
                    file.ContentType != "text/richtext" &&                                                              //RTF
                    file.ContentType != "text/html" &&                                                                  //HTML
                    file.ContentType != "text/csv" &&                                                                   //CSV
                    file.ContentType != "text/tab-separated-values")                                                    //TSV
                {
                    return "Invalid document file type (we accept CSV, DOC, DOCX, HTML, ODP, ODS, ODT, PDF, PPS, PPT, PPTX, RTF, SXC, SXI, SXW, TSV, TXT, WPD, XLS and XLSX)";
                }

                if (file.ContentLength > MaximumDocumentSizeInBytes)
                {
                    return "Document file is too big, we allow up to " + MaximumDocumentSizeInBytes / (1024 * 1024) + "MB";
                }

                // completed with no errors
            }
            catch (Exception e)
            {
                return "Could not validate document";
            }
            return Success;
        }

        public static int MaximumPictureSizeInBytes = 100 * (1024 * 1024);
        public static string ValidatePicture(HttpPostedFileBase file)
        {

            try
            {
                if (file == null)
                {
                    return "Picture file is empty";
                }

                if (file.ContentType != "image/jpeg" &&     //JPEG, JPG, JPE
                    file.ContentType != "image/png" &&      //PNG
                    file.ContentType != "image/bmp")        //BMP
                {
                    return "Invalid picture file type (we accept JPG, PNG and BMP)";
                }

                if (file.ContentLength > MaximumPictureSizeInBytes)
                {
                    return "Picture file is too big, we allow up to " + MaximumPictureSizeInBytes / (1024 * 1024) + "MB";
                }

                // completed with no errors
            }
            catch (Exception e)
            {
                return "Could not validate picture";
            }
            return Success;
        }

        public static int MaximumAudioSizeInBytes = 100 * (1024 * 1024);
        public static string ValidateAudio(HttpPostedFileBase file)
        {

            try
            {
                if (file == null)
                {
                    return "Audio file is empty";
                }
                string[] split = file.FileName.Split('.');

                if (file.ContentType != "audio/mp3" && file.ContentType != "audio/mpeg" && file.ContentType != "audio/ogg" && file.ContentType != "audio/wav")
                {
                    if (split[split.Length - 1] != "mp3" && split[split.Length-1] != "m4a" && split[split.Length-1] != "oga" && split[split.Length-1] != "ogg" && split[split.Length-1] != "wav")
                    {
                        return "Invalid audio file type - we accept MP3, WAV, M4A, and OGG file types";
                    }
                }

                if (file.ContentLength > MaximumAudioSizeInBytes)
                {
                    return "Audio file is too large, we allow up to " + MaximumAudioSizeInBytes/(1024*1024) + "MB";
                }

                // completed with no errors
            }
            catch (Exception e)
            {
                return "Could not validate audio";
            }
            return Success;
        }

        public static string ValidateFirstName(string firstName)
        {
            try
            {
                if (firstName == null)
                {
                    return "First name not entered";
                }

                if (firstName.Length < 1 || firstName.Length > 100)
                {
                    return "Invalid first name (must be at least 1 character and less than 100 characters)";
                }

                // completed with no errors
            }
            catch (Exception e)
            {
                return "Could not validate first name";
            }
            return Success;
        }

        public static string ValidateLastName(string lastName)
        {
            try
            {
                if (lastName == null)
                {
                    return "Last name not entered";
                }

                if (lastName.Length < 1 || lastName.Length > 100)
                {
                    return "Invalid last name (must be at least 1 character and less than 100 characters)";
                }

                // completed with no errors
            }
            catch (Exception e)
            {
                return "Could not validate last name";
            }
            return Success;
        }

        public static string ValidateSchool(string school)
        {
            try
            {
                if (school == null)
                {
                    return "School not entered";
                }

                if (school.Length < 1 || school.Length > 100)
                {
                    return "Invalid school (must be at least 1 character and less than 100 characters)";
                }

                // completed with no errors
            }
            catch (Exception e)
            {
                return "Could not validate school";
            }
            return Success;
        }

        public static string ValidateMajor(string major)
        {
            try
            {
                if (major == null)
                {
                    return "Major not entered";
                }

                if (major.Length < 1 || major.Length > 100)
                {
                    return "Invalid major (must be at least 1 character and less than 100 characters)";
                }

                // completed with no errors
            }
            catch (Exception e)
            {
                return "Could not validate major";
            }
            return Success;
        }

        public static string ValidateLocation(string location)
        {
            try
            {
                if (location == null)
                {
                    return "Location not entered";
                }

                if (location.Length < 1 || location.Length > 100)
                {
                    return "Invalid location (must be at least 1 character and less than 100 characters)";
                }

                // completed with no errors
            }
            catch (Exception e)
            {
                return "Could not validate location";
            }
            return Success;
        }

        public static string ValidateTitle(string title)
        {
            try
            {
                if (title == null)
                {
                    return "Title not entered";
                }

                if (title.Length < 1 || title.Length > 100)
                {
                    return "Invalid title (must be at least 1 character and less than 100 characters)";
                }

                // completed with no errors
            }
            catch (Exception e)
            {
                return "Could not validate title";
            }
            return Success;
        }

        public static string ValidateCity(string city)
        {
            try
            {
                if (city == null)
                {
                    return "City not entered";
                }

                if (city.Length < 1 || city.Length > 100)
                {
                    return "Invalid city (must be at least 1 character and less than 100 characters)";
                }

                // completed with no errors
            }
            catch (Exception e)
            {
                return "Could not validate city";
            }
            return Success;
        }

        public static string ValidateState(string state)
        {
            try
            {
                if (state == null)
                {
                    return "State not entered";
                }

                if (state.Length < 1 || state.Length > 100)
                {
                    return "Invalid state (must be at least 1 character and less than 100 characters)";
                }

                // completed with no errors
            }
            catch (Exception e)
            {
                return "Could not validate state";
            }
            return Success;
        }

        public static string ValidateCompany(string company)
        {
            try
            {
                if (company == null)
                {
                    return "Company not entered";
                }

                if (company.Length < 1 || company.Length > 100)
                {
                    return "Invalid company (must be at least 1 character and less than 100 characters)";
                }

                // completed with no errors
            }
            catch (Exception e)
            {
                return "Could not validate company";
            }
            return Success;
        }
        public static string ValidateDescription(string description)
        {
            try
            {
                if (description == null)
                {
                    return "Description not entered";
                }

                if (description.Length < 1 || description.Length > 5000)
                {
                    return "Invalid description (must be at least 1 character and less than 5000 characters)";
                }

                // completed with no errors
            }
            catch (Exception e)
            {
                return "Could not validate description";
            }
            return Success;
        }
    }
}
