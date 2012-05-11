using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entity
{
    public class ProjectElement_Experience : ProjectElement
    {
        public string jobTitle { get; set; }
        public string jobDescription { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string company { get; set; }
    }
}
