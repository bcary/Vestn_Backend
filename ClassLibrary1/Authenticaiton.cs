﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entity
{
    public class Authenticaiton
    {
        public int id { get; set; }
        public int userId { get; set; }
        public string token { get; set; }
        public DateTime timeStamp { get; set; }
    }
}
