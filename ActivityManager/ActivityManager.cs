using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Accessor;
using Entity;
using System.IO;
using System.Net;

namespace Manager
{
    public class ActivityManager
    {
        ActivityAccessor activityAccessor = new ActivityAccessor();

        public void AddActivity(int userId, string type, string action, int referenceId)
        {
            Activity a = new Activity();
            a.action = action;
            a.referenceId = referenceId;
            a.timeStamp = DateTime.Now;
            a.type = type;
            a.userId = userId;
            activityAccessor.AddActivity(a);
        }
        public bool DeleteActivity(Activity a)
        {
            return activityAccessor.DeleteActivity(a);
        }
        public Activity GetActivity(int activityId)
        {
            return activityAccessor.GetActivity(activityId);
        }
        public List<JsonModels.Activity> GetUserActivity(int userId)
        {
            try
            {
                List<Activity> aList = activityAccessor.GetUserActivity(userId);
                List<JsonModels.Activity> jsonActivityList = new List<JsonModels.Activity>();
                foreach (Activity a in aList)
                {
                    if (a != null)
                    {
                        JsonModels.Activity jsonActivity = new JsonModels.Activity();
                        jsonActivity.action = a.action;
                        jsonActivity.id = a.id;
                        jsonActivity.referenceId = a.referenceId;
                        jsonActivity.timeStamp = a.timeStamp.ToString();
                        jsonActivity.type = a.type;
                        jsonActivity.userId = a.userId;
                        jsonActivityList.Add(jsonActivity);
                    }
                }
                return jsonActivityList;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
