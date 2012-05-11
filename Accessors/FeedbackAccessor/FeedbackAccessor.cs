using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entity;

namespace Accessor
{
    public class FeedbackAccessor
    {
        LogAccessor logAccessor = new LogAccessor();
        public string TestMe()
        {
            return "success";
        }

        public Feedback CreateFeedback(string name, string message, string subject)
        {
            Feedback f = new Feedback();
            f.name = name;
            f.message = message;
            f.subject = subject;

            try
            {
                VestnDB db = new VestnDB();
                db.feedback.Add(f);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), e.ToString());
                return null;//what to do here?

            }
            return f;
        }
    }
}
