using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entity;

namespace Entity
{
    public class Authentication
    {
        public int id { get; set; }
        public int userId { get; set; }
        public DateTime timeStamp { get; set; }
        public string token { get; set; }
    }
}
