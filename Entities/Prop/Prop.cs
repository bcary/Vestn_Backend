  using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entity
{
    public class Prop
    {
        public int id { get; set; }
        public string message { get; set; }
        public int projectId { get; set; }
        public int userId { get; set; }
        public DateTime timeStamp { get; set; }
    }
}
