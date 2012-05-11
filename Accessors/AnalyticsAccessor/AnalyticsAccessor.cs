using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entity;

namespace Accessor
{
    public class AnalyticsAccessor
    {

        public Analytics CreateAnalytic(string eventType, DateTime eventTime, string eventUserName)
        {
            return CreateAnalytic(eventType, eventTime, eventUserName, null);
        }
        
        public Analytics CreateAnalytic(string eventType, DateTime eventTime, string eventUserName, string eventStatement)
        {
            Analytics analytic = new Analytics();
            analytic.eventType = eventType;
            analytic.eventTime = eventTime;
            analytic.eventUserName = eventUserName;
            analytic.eventStatement = eventStatement;

            try
            {
                VestnDB db = new VestnDB();
                db.analytics.Add(analytic);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                return null;//what to do here?
            }
            return analytic;
        }    
    }
}
