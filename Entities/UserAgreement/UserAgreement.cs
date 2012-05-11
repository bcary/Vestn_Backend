using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entity
{
    public class UserAgreement
    {
        public int id { get; set; }
        public DateTime agreementTime { get; set; }
        public string userName { get; set; }
        public string value { get; set; }
        public string IPAddress { get; set; }
    }
}
