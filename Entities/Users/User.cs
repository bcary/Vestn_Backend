using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using Entity;

namespace Entity
{
    public class User
    {
        public int id { get; set; }
        public string userName { get; set; }
        public string email { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string title { get; set; }
        public DateTime birthDate { get; set; }
        public string tagIds { get; set; }
        public string location { get; set; }
        public string organization { get; set; }
        public string major { get; set; }
        [DataType(DataType.PhoneNumber)]
        public string phoneNumber { get; set; }
        public List<Project> projects { get; set; }
        public string profilePicture { get; set; }
        public string profilePictureThumbnail { get; set; }
        public string resume { get; set; }
        public string description { get; set; }
        public DateTime graduationDate { get; set; }
        public int emailVerified { get; set; }
        public int isPublic { get; set; }
        public int isActive { get; set; }
        public string connections { get; set; }
        public string aboutPicture { get; set; }
        public string aboutPictureThumbnail { get; set; }
        public string profileURL { get; set; }
        public string projectOrder { get; set; }
        public string tagLine { get; set; }
        public string facebookLink { get; set; }
        public string twitterLink { get; set; }
        public string linkedinLink { get; set; }
        public int profileViews { get; set; }
    }

    public enum WillingToRelocateType
    {
        undecided = -1,
        yes = 1,
        no = 0
    }

    public enum EmploymentStatus
    {
        Employed = -1,
        LookingForInternship = 0,
        LookingForJob = 1,
        NotLooking = 2
    }
}
