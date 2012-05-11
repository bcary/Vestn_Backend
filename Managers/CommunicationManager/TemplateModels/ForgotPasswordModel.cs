using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager
{
    public class ForgotPasswordModel
    {
        public string Name { get; set; }
        public string ResetPasswordHash { get; set; }
        public string ClientUrl { get; set; }
    }
}
