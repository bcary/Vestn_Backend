using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entity
{
    public class ProjectTags
    {
        public int id { get; set; }
        public int projectId { get; set; }
        public int tagId { get; set; }
        public string tagType { get; set; }
    }
}
