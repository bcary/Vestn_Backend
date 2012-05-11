using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entity
{
    public class Analytics
    {
        public int id { get; set; }
        public string eventType { get; set; }
        public DateTime eventTime { get; set; }
        public string eventUserName { get; set; }
        public string eventStatement { get; set; }
    }
}
