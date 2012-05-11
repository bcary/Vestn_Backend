using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entity
{
    public class UserTags
    {
        public int id { get; set; }
        public int tagId { get; set; }
        public int userId { get; set; }
        public string tagType { get; set; }
    }
}
