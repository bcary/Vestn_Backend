using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using Entity;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Microsoft.WindowsAzure.StorageClient;
using Manager;

namespace UserClientMembers.Models
{
    public class UserModel
    {
        public int id { get; set; }
        public string userName { get; set; }
        public string email { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string title { get; set; }
        public DateTime birthDate { get; set; }
        public string location { get; set; }
        public string school { get; set; }
        public string major { get; set; }
        [DataType(DataType.PhoneNumber)]
        public string phoneNumber { get; set; }
        //public WillingToRelocateType willingToRelocate { get; set; }
        public int emailVerified { get; set; }
        public int isPublic { get; set; }
        public int isActive { get; set; }
        public List<User> connections { get; set; }
        public string profilePicture { get; set; }
        public string profilePictureThumbnail { get; set; }
        public string aboutPicture { get; set; }
        public string aboutPictureThumbnail { get; set; }
        public string profileURL { get; set; }
        public string projectOrder { get; set; }

        public User toUser()
        {
            User newUser = new User();

            newUser.firstName = this.firstName;
            newUser.lastName = this.lastName;
            newUser.title = this.title;
            newUser.email = this.email;
            newUser.userName = this.userName;
            newUser.id = this.id;
            newUser.birthDate = this.birthDate;
            newUser.location = this.location;
            newUser.organization = this.school;
            newUser.major = this.major;
            newUser.phoneNumber = this.phoneNumber;
            //newUser.willingToRelocate = this.willingToRelocate;
            newUser.emailVerified = this.emailVerified;
            newUser.isPublic = this.isPublic;
            newUser.isActive = this.isActive;
            newUser.profilePicture = this.profilePicture;
            newUser.profilePictureThumbnail = this.profilePictureThumbnail;
            newUser.aboutPicture = this.aboutPicture;
            newUser.aboutPictureThumbnail = this.aboutPictureThumbnail;
            newUser.profileURL = this.profileURL;
            newUser.projectOrder = this.projectOrder;

            return newUser;
        }

        public UserModel() { }

        public UserModel(User user)
        {
            this.firstName = user.firstName;
            this.lastName = user.lastName;
            this.title = user.title;
            this.email = user.email;
            this.userName = user.userName;
            this.id = user.id;
            this.birthDate = user.birthDate;
            this.location = user.location;
            this.school = user.organization;
            this.major = user.major;
            this.phoneNumber = user.phoneNumber;
            //this.willingToRelocate = user.willingToRelocate;
            this.emailVerified = user.emailVerified;
            this.isPublic = user.isPublic;
            this.isActive = user.isActive;
            this.profilePicture = user.profilePicture;
            this.profilePictureThumbnail = user.profilePictureThumbnail;
            this.aboutPicture = user.aboutPicture;
            this.aboutPictureThumbnail = user.aboutPictureThumbnail;
            this.profileURL = user.profileURL;
            this.projectOrder = user.projectOrder;
            
        }
    }

    public class ProfileModel
    {
        public int id { get; set; }
        public string userName { get; set; }
        public string email { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string title { get; set; }
        public DateTime birthDate { get; set; }
        public string location { get; set; }
        public string school { get; set; }
        public string major { get; set; }
        [DataType(DataType.PhoneNumber)]
        public string phoneNumber { get; set; }
       // public WillingToRelocateType willingToRelocate { get; set; }
        public List<string> userTags { get; set; }
        public string profilePicture { get; set; }
        public string profilePictureThumbnail { get; set; }
        public string aboutPicture { get; set; }
        public string aboutPictureThumbnail { get; set; }
        public string resume { get; set; }
        public string description { get; set; }
       // public EmploymentStatus status { get; set; }
        public DateTime graduationDate { get; set; }
        public int emailVerified { get; set; }
        public int isPublic { get; set; }
        public int isActive { get; set; }
        public List<ProjectModel> projects { get; set; }
        public string visibleProject { get; set; }
        public List<User> connections { get; set; }
        public bool agreement { get; set; }
        public string profileURL { get; set; }
        public string projectOrder { get; set; }
        public ProfileModel() { }

        public ProfileModel(User user)
        {
            this.id = user.id;
            this.birthDate = user.birthDate;
            this.description = user.description;
            //this.status = user.status;
            this.graduationDate = user.graduationDate;
            this.email = user.email;
            this.firstName = user.firstName;
            this.lastName = user.lastName;
            this.title = user.title;
            this.location = user.location;
            this.major = user.major;
            this.phoneNumber = user.phoneNumber;
            this.school = user.organization;
            this.userName = user.userName;
            //this.willingToRelocate = user.willingToRelocate;
            this.profilePicture = user.profilePicture;
            this.profilePictureThumbnail = user.profilePictureThumbnail;
            this.aboutPicture = user.aboutPicture;
            this.aboutPictureThumbnail = user.aboutPictureThumbnail;
            this.projects = new List<ProjectModel>();
            this.profileURL = user.profileURL;
            this.projectOrder = user.projectOrder;
            TagManager tm = new TagManager();
            this.userTags = tm.getAllUserTags(this.id);
            if(user.projects != null)
            {
                foreach (Project project in user.projects)
                {
                    if (project.isActive)
                    {
                        this.projects.Add(new ProjectModel(project));
                    }
                }
            }

            this.resume = user.resume;
            this.emailVerified = user.emailVerified;
            this.isPublic = user.isPublic;
            this.isActive = user.isActive;
            //this.visibleProject = "project0";
        }

        public User toUser()
        {
            User newUser = new User();

            newUser.id = this.id;
            newUser.birthDate = this.birthDate;
            newUser.email = this.email;
            newUser.firstName = this.firstName;
            newUser.lastName = this.lastName;
            newUser.title = this.title;
            newUser.location = this.location;
            newUser.major = this.major;
            newUser.phoneNumber = this.phoneNumber;
            newUser.organization = this.school;
            newUser.userName = this.userName;
            newUser.description = this.description;
            //newUser.status = this.status;
            newUser.graduationDate = this.graduationDate;
            newUser.emailVerified = this.emailVerified;
            newUser.isPublic = this.isPublic;
            newUser.isActive = this.isActive;
            newUser.aboutPicture = this.aboutPicture;
            newUser.aboutPictureThumbnail = this.aboutPicture;
            newUser.profilePicture = this.profilePicture;
            newUser.profilePictureThumbnail = this.profilePictureThumbnail;
            newUser.profileURL = this.profileURL;
            newUser.projectOrder = this.projectOrder;
            return newUser;
        }
    }

    public class ChangePasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ProjectModel
    {
        public int id { get; set; }
        public List<ProjectElement_Document> documents { get; set; }
        public List<ProjectElement_Experience> experience { get; set; }
        public List<ProjectElement_Information> information { get; set; }
        public List<ProjectElement_Picture> pictures { get; set; }
        public List<ProjectElement_Video> videos { get; set; }
        public List<ProjectElement_Audio> audio { get; set; }
        public List<bool> profileAccessKeys { get; set; }
        public string name { get; set; }
        public string coverPicture { get; set; }
        public string coverPictureThumbnail { get; set; }
        public string description { get; set; }
        public bool isActive { get; set; }
        public string projectElementOrder { get; set; }
        public List<string> projectTags { get; set; }
        public ProjectModel(){ }

        public ProjectModel(Project project)
        {
            this.id = project.id;
            this.information = new List<ProjectElement_Information>();
            this.documents = new List<ProjectElement_Document>();
            this.experience = new List<ProjectElement_Experience>();
            this.pictures = new List<ProjectElement_Picture>();
            this.videos = new List<ProjectElement_Video>();
            this.audio = new List<ProjectElement_Audio>();
            this.name = project.name;
            this.coverPicture = project.coverPicture;
            this.coverPictureThumbnail = project.coverPictureThumbnail;
            this.description = project.description;
            this.isActive = project.isActive;
            this.projectElementOrder = project.projectElementOrder;
            TagManager tm = new TagManager();
            this.projectTags = tm.getAllProjectTags(project.id);
            foreach(ProjectElement element in project.projectElements){
                if (element.GetType() == typeof(ProjectElement_Document))
                {
                    this.documents.Add((ProjectElement_Document)element);
                }
                else if (element.GetType() == typeof(ProjectElement_Experience))
                {
                    this.experience.Add((ProjectElement_Experience)element);
                }
                else if (element.GetType() == typeof(ProjectElement_Information))
                {
                    this.information.Add((ProjectElement_Information)element);
                }
                else if (element.GetType() == typeof(ProjectElement_Picture))
                {
                    this.pictures.Add((ProjectElement_Picture)element);
                }
                else if (element.GetType() == typeof(ProjectElement_Video))
                {
                    this.videos.Add((ProjectElement_Video)element);
                }
                else if (element.GetType() == typeof(ProjectElement_Audio))
                {
                    this.audio.Add((ProjectElement_Audio)element);
                }
            }
        }

        public Project toProject()
        {
            Project project = new Project();
            project.id = this.id;
            project.projectElements = new List<ProjectElement>();
            project.name = this.name;
            project.coverPicture = this.coverPicture;
            project.coverPictureThumbnail = this.coverPictureThumbnail;
            project.description = this.description;
            project.isActive = this.isActive;
            project.projectElementOrder= this.projectElementOrder;
            if (this.documents != null)
            {
                foreach (ProjectElement element in this.documents)
                {
                    project.projectElements.Add(element);
                }
            }

            if (this.experience != null)
            {
                foreach (ProjectElement element in this.experience)
                {
                    project.projectElements.Add(element);
                }
            }
            if (this.information != null)
            {
                foreach (ProjectElement element in this.information)
                {
                    project.projectElements.Add(element);
                }
            }
            if (this.pictures != null)
            {
                foreach (ProjectElement element in this.pictures)
                {
                    project.projectElements.Add(element);
                }
            }
            if (this.videos != null)
            {
                foreach (ProjectElement element in this.videos)
                {
                    project.projectElements.Add(element);
                }
            }
            if (this.audio != null)
            {
                foreach (ProjectElement element in this.audio)
                {
                    project.projectElements.Add(element);
                }
            }
            return project;
        }
    }

    public class LogOnModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        //[Required]
        //[Display(Name = "Beta Key")]
        //public string betaKey { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email address")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public RegisterModel() { }

        public RegisterModel(User user)
        {
            this.UserName = user.userName;
            this.Email = user.email;
        }

        public User toUser()
        {
            return new User()
            {
                userName = this.UserName,
                email = this.Email
            };
        }
    }

    public class SplashPageModel
    {
        public int key { get; set; }
    }

    public class FileUploadModel
    {
        /// <summary>
        /// Gets or sets the block count.
        /// </summary>
        /// <value>The block count.</value>
        public long BlockCount { get; set; }

        /// <summary>
        /// Gets or sets the size of the file.
        /// </summary>
        /// <value>The size of the file.</value>
        public long FileSize { get; set; }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>The name of the file.</value>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the CloudBlockBlob object associated with this blob.
        /// </summary>
        /// <value>The CloudBlockBlob object associated with this blob.</value>
        public CloudBlockBlob BlockBlob { get; set; }

        /// <summary>
        /// Gets or sets the operation start time.
        /// </summary>
        /// <value>The start time.</value>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the upload status message.
        /// </summary>
        /// <value>The upload status message.</value>
        public string UploadStatusMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether upload of this instance is complete.
        /// </summary>
        /// <value>
        /// True if upload of this instance is complete; otherwise, false.
        /// </value>
        public bool IsUploadCompleted { get; set; }
        public int entityId { get; set; }
        public string entity { get; set; }
        public string contentType { get; set; }

        public string thumbnail { get; set; }
    }

    public class ChangeUserNameModel
    {
        [Display(Name = "New username")]
        public string NewUserName { get; set; }

        [Display(Name = "Confirm new username")]
        [Compare("NewUserName", ErrorMessage = "The new username and confirmation username do not match.")]
        public string ConfirmUserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string Password { get; set; }
    }

    public class HelpTips
    {
        public string HelpTipsText { get; set; }
    }

    public class ErrorModel
    {
        public int id { get; set; }
        public DateTime eventTime { get; set; }
        public string location { get; set; }
        public string exception { get; set; }

        public ErrorModel(Log log)
        {
            this.id = log.id;
            this.eventTime = log.eventTime;
            this.location = log.location;
            this.exception = log.exception;
        }
    }

    
}
