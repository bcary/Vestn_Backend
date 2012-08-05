using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entity
{
    public class Network
    {
        public int id { get; set; }
        public string name { get; set; }
        public List<User> admins { get; set; }//determine how to flag admin status and for what network?
        public List<User> networkUsers { get; set; } //all users in all subnetworks
        public string description { get; set; }
        public string privacy { get; set; } //if isPublic is true -> any user can access the network. if false -> only members of networkUsers can access the network (and subnetworks)
        public string coverPicture { get; set; }
        public string profileURL { get; set; }
        public string networkIdentifier { get; set; }
    }
}
