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
    public class ActivityAccessor
    {
        LogAccessor logAccessor = new LogAccessor();
        public Activity AddActivity(Activity activity)
        {
            try
            {
                if (activity != null)
                {
                    VestnDB db = new VestnDB();
                    db.activity.Add(activity);
                    db.SaveChanges();
                    return activity;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                logAccessor.CreateLog(DateTime.Now, "activityAccessor - AddActivity", e.StackTrace);
                return null;
            }
        }

        public Activity GetActivity(int activityId)
        {
            try
            {
                VestnDB db = new VestnDB();
                Activity activity = db.activity.Where(p => p.id == activityId).FirstOrDefault();
                return activity;

            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "ActivityAccessor - GetActivity", ex.StackTrace);
                return null;
            }
        }

        public Activity UpdateActivity(Activity activity)
        {
            try
            {
                if (activity != null)
                {
                    VestnDB db = new VestnDB();
                    db.Entry(activity).State = EntityState.Modified;
                    db.SaveChanges();
                    return activity;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                logAccessor.CreateLog(DateTime.Now, "ActivityAccessor - UpdateActivity", e.StackTrace);
                return null;
            }
        }

        public List<Activity> GetUserActivity(int userId)
        {
            try
            {
                VestnDB db = new VestnDB();
                List<Activity> userActivity = (List<Activity>)db.activity.Where(p => p.userId == userId);
                return userActivity;
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "ActivityAccessor - GetUserActivity", ex.StackTrace);
                return null;
            }
        }

        public bool DeleteActivity(Activity activity)
        {
            try
            {
                bool wasDeleted = false;
                if (activity != null)
                {
                    VestnDB db = new VestnDB();
                    db.activity.Attach(activity);
                    db.activity.Remove(activity);
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
                logAccessor.CreateLog(DateTime.Now, "ActivityAccessor - deleteActivity", e.StackTrace);
                return false;
            }
        }

    }
}