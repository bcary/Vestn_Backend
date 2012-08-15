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
        public string networkPictureThumbnail { get; set; }
        //public string networks { get; set; }

        public virtual ICollection<Network> networks { get; set; }
        public virtual ICollection<Network> adminNetworks { get; set; }

        public User()
        {
            networks = new HashSet<Network>();
            adminNetworks = new HashSet<Network>();
        }
    }

    public class Network
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string privacy { get; set; } //if isPublic is true -> any user can access the network. if false -> only members of networkUsers can access the network (and subnetworks)
        public string coverPicture { get; set; }
        public string profileURL { get; set; }
        public string networkIdentifier { get; set; }

        public virtual ICollection<User> admins { get; set; }//determine how to flag admin status and for what network?
        public virtual ICollection<User> networkUsers { get; set; } //all users in all subnetworks

        public Network()
        {
            admins = new HashSet<User>();
            networkUsers = new HashSet<User>();
        }
    }

    public class Network_SubNetwork : Network
    {
        public virtual ICollection<Network_Group> groups { get; set; }
        public virtual Network_TopNetwork Network_TopNetwork { get; set; }

        public Network_SubNetwork()
        {
            groups = new List<Network_Group>();
        }
    }

    public class Network_Group : Network
    {
        public virtual Network_SubNetwork Network_SubNetwork { get; set; }
    }

    public class Network_TopNetwork : Network
    {
        public virtual ICollection<Network_SubNetwork> subNetworks { get; set; }

        public Network_TopNetwork()
        {
            subNetworks = new List<Network_SubNetwork>();
        }
    }

}
