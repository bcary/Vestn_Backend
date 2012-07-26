using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using Entity;
using Accessor;
using System.Diagnostics.CodeAnalysis;

namespace Accessor
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

        public Prop GetProp(int propId)
        {
            try
            {
                VestnDB db = new VestnDB();
                Prop prop = db.prop.Where(p => p.id == propId).FirstOrDefault();
                return prop;

            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "PropAccessor - GetProp", ex.StackTrace);
                return null;
            }
        }

        public Prop UpdateProp(Prop prop)
        {
            try
            {
                if (prop != null)
                {
                    VestnDB db = new VestnDB();
                    db.Entry(prop).State = EntityState.Modified;
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
                logAccessor.CreateLog(DateTime.Now, "userAccessor - UpdateExperience", e.StackTrace);
                return null;
            }
        }
        public List<Prop> GetProjectProps(int projectId)
        {
            try
            {
                VestnDB db = new VestnDB();
                List<Prop> projectProps = (List<Prop>)db.prop.Where(p => p.projectId == projectId);
                return projectProps;
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "PropAccessor - GetProjectProps", ex.StackTrace);
                return null;
            }
        }
        public bool DeleteProp(Prop prop)
        {
            try
            {
                bool wasDeleted = false;
                if (prop != null)
                {
                    VestnDB db = new VestnDB();
                    db.prop.Attach(prop);
                    db.prop.Remove(prop);
                    db.SaveChanges();
                    wasDeleted = true;
                    return wasDeleted;
                }
                else
                {
                    return wasDeleted;
                }
            }
            catch (Exception e)
            {
                logAccessor.CreateLog(DateTime.Now, "userAccessor - deleteExperience", e.StackTrace);
                return false;
            }
        }
    }
}
