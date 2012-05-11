using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entity
{
    public class ProjectElement_Document : ProjectElement
    {
        public string documentLocation { get; set; }
        public string documentThumbnailLocation { get; set; }
        public string documentText { get; set; }
    }
}

