using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entity
{
    public class ProjectElement_Picture : ProjectElement
    {
        public string pictureLocation { get; set; }
        public string pictureThumbnailLocation { get; set; }
        public string pictureGalleriaThumbnailLocation { get; set; }
    }
}
