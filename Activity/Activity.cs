using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entity
{
    public class Activity
    {
        public int id { get; set; }
        public int userId { get; set; }
        public string type { get; set; }
        public string action { get; set; } //action performed by user (created, updated, deleted, gave(prop))
        public int referenceId { get; set; } //the id of the object (Prop, Project, Artifact, etc.) - identified in the type
        public DateTime timeStamp { get; set; }
    }
}
