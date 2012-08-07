using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Entity
{
    public class JsonModels
    {
        [DataContract]
        public class UserInformation
        {
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string firstName { get; set; }

            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string lastName { get; set; }

            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string title { get; set; }

            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string school { get; set; }

            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string connections { get; set; }

            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string description { get; set; }

            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string tagLine { get; set; }

            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string resume { get; set; }

            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string profilePicture { get; set; }

            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string profilePictureThumbnail { get; set; }

            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string aboutPicture { get; set; }

            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string aboutPictureThumbnail { get; set; }

            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public UserStats stats { get; set; }

            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public Links links { get; set; }

            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public List<Experience> experiences { get; set; }

            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public List<Reference> references { get; set; }

            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public List<UserTag> tags { get; set; }

            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public List<ProjectShell> projects { get; set; }

            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public List<Todo> todo { get; set; }

            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public List<RecentActivity> recentActivity { get; set; }
        }

        [DataContract]
        public class ProfileInformation
        {
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string id { get; set; }

            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string firstName { get; set; }

            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string lastName { get; set; }

            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string email { get; set; }

            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string phoneNumber { get; set; }

            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string title { get; set; }

            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string organization { get; set; }

            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string connections { get; set; }

            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string location { get; set; }

            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string major { get; set; }

            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string description { get; set; }

            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string tagLine { get; set; }

            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string resume { get; set; }

            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string projectOrder { get; set; }

            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string profilePicture { get; set; }

            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string profilePictureThumbnail { get; set; }

            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public UserStats stats { get; set; }

            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string facebookLink { get; set; }

            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string twitterLink { get; set; }

            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string linkedinLink { get; set; }

            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public List<Experience> experiences { get; set; }

            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public List<Reference> references { get; set; }

            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public List<UserTag> tags { get; set; }

            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public List<CompleteProject> projects { get; set; }

            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public List<Todo> todo { get; set; }

           [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public List<Activity> activity { get; set; }

           [DataMember(IsRequired = true, EmitDefaultValue = true)]
           public int profileViews { get; set; }

           [DataMember(IsRequired = true, EmitDefaultValue = true)]
           public ProfileScore profileScore { get; set; }

        }


        [DataContract]
        public class UserStats
        {
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public int views { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public int props { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public int numberOfArtifacts { get; set; }
        }

        [DataContract]
        public class Network
        {
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public int id { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string name { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string description { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string coverPicture { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string privacy { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string profileURL { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public List<NetworkUserShell> admins { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public List<NetworkShell> subNetworks { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public NetworkShell parentNetwork { get; set; }
        }

        [DataContract]
        public class NetworkUserShell
        {
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public int userId { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string firstName { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string lastName { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string profileURL { get; set; }
        }

        [DataContract]
        public class NetworkShell
        {
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public int networkId { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string name { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string coverPicture { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string profileURL { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string privacy { get; set; }
        }

        [DataContract]
        public class NetworkUsers
        {
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public List<NetworkUserShell> users { get; set; }
        }



        [DataContract]
        public class UserInformationModel
        {

            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public List<UserNetworkShell> networks { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public UserSettings settings { get; set; }
        }

        [DataContract]
        public class UserNetworkShell
        {
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public int networkId { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string name { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string networkURL { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string userAuthorization { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public List<UserNetworkShell> subnetworks { get; set; }
        }

        [DataContract]
        public class UserSettings
        {
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string visibility { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string profileURL { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string email { get; set; }
            
        }

        [DataContract]
        public class Links
        {
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string facebookLink { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string twitterLink { get; set; }
           [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string linkedinLink { get; set; }
        }

        [DataContract]
        public class UserTag
        {
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string userTagValue { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public int userTagId { get; set; }
        }

        [DataContract]
        public class ProjectTag
        {
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string projectTagValue { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public int projectTagId { get; set; }
        }

        [DataContract]
        public class ProjectShell
        {
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public int id { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string name { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public List<ProjectTag> projectTags { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public List<ArtifactShell> artifacts { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string description { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string coverPicture { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string coverPictureThumbnail { get; set; }
        }

        [DataContract]
        public class ArtifactShell
        {
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public int id { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string title { get; set; }
        }

        [DataContract]
        public class LogOnModel
        {
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public int userId { get; set; }

            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string token { get; set; }

            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string firstName { get; set; }

            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string lastName { get; set; }

            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string profileURL { get; set; }
        }

        [DataContract]
        public class RecentActivity
        {
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public int id { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string comment { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string propGiverName { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string propGiverURL { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public int propGiverID { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string pictureURL { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string type { get; set; }//contactUpdate, newProject (projectName), newArtifact (artifactCount, projectName), prop (userName, pictureURL, comments), 
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public int artifactCount { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string projectName { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public int projectID { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string date { get; set; }
        }

        [DataContract]
        public class Todo
        {
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public int id { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string item { get; set; }
        }

        [DataContract]
        public class Artifact
        {
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string type { get; set; }//document, picture, video
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string description { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string creationDate { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string fileLocation { get; set; }
            //[DataMember(IsRequired = true, EmitDefaultValue = true)]
            //public string thumbnailLocation { get; set; }
           [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string artifactLocation { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string title { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public int id { get; set; }

        }

        [DataContract]
        public class CompleteProject
        {
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string title { get; set; }//document, picture, video
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string description { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public int id { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public List<ProjectTag> projectTags { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public List<Artifact> artifacts { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string artifactOrder { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string coverPicture { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string coverPictureThumbnail { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string dateModified { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string privacy { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public List<Prop> props { get; set; }
        }

        [DataContract]
        public class UploadReponse
        {
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string name { get; set; }//document, picture, video
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string fileURL { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public int id { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string thumbnailURL { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string artifactURL { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string galeriaURL { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string description { get; set; }
        }

        [DataContract]
        public class RegisterResponse
        {
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public int id { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string token { get; set; }
        }

        [DataContract]
        public class Experience
        {
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public int id { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string title { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string description { get; set; }
           [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string startDate { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string endDate { get; set; }
           [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string city { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string state { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string company { get; set; }
        }

        [DataContract]
        public class Reference
        {
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public int id { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string firstName { get; set; }
           [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string lastName { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string email { get; set; }
           [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string company { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string title { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string message { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string videoLink { get; set; }
           [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string videoType { get; set; }
        }

        [DataContract]
        public class Prop
        {
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public int id { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public int userId { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public int projectId { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string message { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string timeStamp { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string firstName { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string lastName { get; set; }
        }

        [DataContract]
        public class Activity
        {
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public int id { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public int userId { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string type { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string action { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public int referenceId { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public string timeStamp { get; set; }
        }

        [DataContract]
        public class ProfileScore
        {
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public int artifacts { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public int reflections { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public int props { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public int featuredProjects { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public bool profilePic { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public bool resume { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public bool location { get; set; }
            [DataMember(IsRequired = true, EmitDefaultValue = true)]
            public bool school { get; set; }
        }
    }
}
