using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entity;
using System.Data.Entity;
using System.Data;
using System.Data.Linq;


namespace Accessor
{
    public class LogAccessor
    {

        public Log CreateLog(DateTime eventTime, string location, string exception)
        {
            Log log = new Log();
            log.eventTime = eventTime;
            log.location = location;
            log.exception = exception;

            try
            {
                VestnDB db = new VestnDB();
                db.logs.Add(log);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                return null;//what to do here?
            }
            return log;
        }


        public List<Log> GetLogs()
        {
            List<Log> logs = new List<Log>();         
            try
            {
                VestnDB db = new VestnDB();
                logs = db.logs.ToList();
            }
            catch (Exception e)
            {
                return null;//what to do here?
            }
            return logs;
        }
        
    }
}
