using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entity
{
    public class Network_SubNetwork : Network
    {
        public List<Network_Group> groups { get; set; }
        public int Network_TopNetwork_Id { get; set; }
    }
}
