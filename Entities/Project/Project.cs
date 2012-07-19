using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entity
{
    public enum Privacy
    {
        Public,
        Network,
        Private
    }
    public class Project
    {
        public int id {get; set;}
        public List<ProjectElement> projectElements { get; set; }
        public string name { get; set; }
        public string coverPicture { get; set; }
        public string coverPictureThumbnail { get; set; }
        public string description { get; set; }
        public string tagIds { get; set; }
        public bool isActive { get; set; }
        public string projectElementOrder { get; set; }
        public DateTime dateModified { get; set; }
        //public Privacy privacy { get; set; }
    }
}
