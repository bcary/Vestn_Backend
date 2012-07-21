using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entity
{
    public class SubNetwork : Network
    {
        public List<Group> groups { get; set; }
        public int networkId { get; set; }
    }
}
