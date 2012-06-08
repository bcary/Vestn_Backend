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
        public class Experience
        {
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string jobTitle { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string jobDescription { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string company { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string startDate { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string endDate { get; set; }
        }

        [DataContract]
        public class Reference
        {
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string name { get; set; }
        }

        [DataContract]
        public class Links
        {
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string facebookLink { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string twitterLink { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
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
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public int id { get; set; }
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
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string type { get; set; }//document, picture, video
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string description { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string creationDate { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string location { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string thumbnailLocation { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string title { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public int id { get; set; }
            //for experiences
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string company { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string startDate { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string endDate { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string city { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string state { get; set; }
            //for references?

        }

        [DataContract]
        public class CompleteProject
        {
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string name { get; set; }//document, picture, video
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string description { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public int id { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public List<ProjectTag> projectTags { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public List<Artifact> artifacts { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string elementOrder { get; set; }
        }

        [DataContract]
        public class UploadReponse
        {
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string name { get; set; }//document, picture, video
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string URL { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public int id { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string pdfURL { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string thumbnailURL { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string galeriaURL { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public string description { get; set; }
        }
    }
}
