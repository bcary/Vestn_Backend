using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entity;
using Accessor;

namespace PropAccessor
{
    public class PropAccessor
    {
        LogAccessor logAccessor = new LogAccessor();
        public Prop AddProp(Prop prop)
        {
            try
            {
                if (prop != null)
                {
                    VestnDB db = new VestnDB();
                    db.prop.Add(prop);
                    db.SaveChanges();
                    return prop;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                logAccessor.CreateLog(DateTime.Now, "userAccessor - AddReference", e.StackTrace);
                return null;
            }
        }
    }
}
