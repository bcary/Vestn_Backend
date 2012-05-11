using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entity
{
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
        //public string projectElementOrderDocument { get; set; }
        //public string projectElementOrderPicture { get; set; }
        //public string projectElementOrderVideo { get; set; }
        //public string projectElementOrderAudio { get; set; }
        //public string projectElementOrderExperience { get; set; }
        public string projectElementOrder { get; set; }
    }
}
