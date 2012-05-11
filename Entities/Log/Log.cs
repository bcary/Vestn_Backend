using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entity
{
    public class Log
    {
        public int id { get; set; }
        public DateTime eventTime { get; set; }
        public string location { get; set; }
        public string exception { get; set; }
    }
}
