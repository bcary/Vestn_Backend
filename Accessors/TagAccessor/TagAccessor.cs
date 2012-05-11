using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Text;
using Entity;
using System.Diagnostics.CodeAnalysis;

namespace Accessor
{
    public class TagAccessor
    {
        LogAccessor logAccessor = new LogAccessor();
        public string TestMe()
        {
            return "success";
        }

        public sTag CreateSTag(int parentId, string value)//set parentId to 0 when it is at the top level
        {
            sTag tag = new sTag();
            tag.value = value;
            tag.parentId = parentId;
            try
            {
                VestnDB db = new VestnDB();
                db.sTag.Add(tag);
                db.SaveChanges();
            }
            catch (InvalidOperationException)
            {
                return null;//bad id
            }
            return (tag);
        }

        public fTag CreateFTag(int parentId, string value)//set parentId to 0 when it is at the top level
        {
            fTag tag = new fTag();
            tag.value = value;
            tag.owner = parentId;
            try
            {
                VestnDB db = new VestnDB();
                db.fTag.Add(tag);
                db.SaveChanges();
            }
            catch (InvalidOperationException)
            {
                return null;//bad id
            }
            return (tag);
        }

        public sTag GetSTag(int id)
        {
            //Get entity user using ID
            sTag tag = GetEntitySTag(id);
            if (tag == null)
            {
                return null;
            }
            return tag;
        }

        public fTag GetFTag(int id)
        {
            //Get entity user using ID
            fTag tag = GetEntityFTag(id);
            if (tag == null)
            {
                return null;
            }
            return tag;
        }

        public sTag GetSTag(string value)
        {
            //Get entity user using ID
            sTag tag = GetEntitySTag(value);
            if (tag == null)
            {
                return null;
            }
            return tag;
        }

        public fTag GetFTag(string value)
        {
            //Get entity user using ID
            fTag tag = GetEntityFTag(value);
            if (tag == null)
            {
                return null;
            }
            return tag;
        }

        public sTag UpdateSTag(sTag tag)
        {
            sTag oldTag = GetSTag(tag.id);
            if (oldTag.value != tag.value)
            {
                return tag;
            }
            try
            {
                VestnDB db = new VestnDB();
                db.Entry(tag).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (InvalidOperationException)
            {
                return null;
            }
            return tag;
        }

        public fTag UpdateFTag(fTag tag)
        {
            fTag oldTag = GetFTag(tag.id);
            if (oldTag.value != tag.value)
            {
                return tag;
            }
            try
            {
                VestnDB db = new VestnDB();
                db.Entry(tag).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (InvalidOperationException)
            {
                return null;
            }
            return tag;
        }

        private sTag GetEntitySTag(int id)
        {
            VestnDB db = new VestnDB();
            sTag tag;
            try
            {
                tag = db.sTag.Find(id);
            }
            catch (InvalidOperationException e)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), e.ToString());
                return null;
            }
            return tag;
        }

        private fTag GetEntityFTag(int id)
        {
            VestnDB db = new VestnDB();
            fTag tag;
            try
            {
                tag = db.fTag.Find(id);
            }
            catch (InvalidOperationException e)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), e.ToString());
                return null;
            }
            return tag;
        }

        public sTag DeleteSTag(sTag tag)
        {
            VestnDB db = new VestnDB();
            db.sTag.Attach(tag);
            tag = db.sTag.Remove(tag);
            db.SaveChanges();

            return tag;
        }

        public fTag DeleteFTag(fTag tag)
        {
            VestnDB db = new VestnDB();
            db.fTag.Attach(tag);
            tag = db.fTag.Remove(tag);
            db.SaveChanges();

            return tag;
        }

        private sTag GetEntitySTag(string value)
        {
            VestnDB db = new VestnDB();
            sTag tag = new sTag();
            try
            {
                tag = db.sTag.Where(u => u.value == value).First();
            }
            catch (Exception e)
            {
                return null;
            }
            return tag;
        }

        private fTag GetEntityFTag(string value)
        {
            VestnDB db = new VestnDB();
            fTag tag = new fTag();
            try
            {
                tag = db.fTag.Where(u => u.value == value).First();
            }
            catch (Exception e)
            {
                return null;
            }
            return tag;
        }

        public int AddUserLink(int tagId, int userId, string tagType)
        {
            UserTags ut = new UserTags();
            ut.tagId = tagId;
            ut.userId = userId;
            ut.tagType = tagType;
            VestnDB db = new VestnDB();
            UserTags fromDatabase = db.userTags.Where(x => x.tagId == tagId).Where(y => y.userId == userId).FirstOrDefault();
            if (fromDatabase == null)
            {
                try
                {

                    db.userTags.Add(ut);
                    db.SaveChanges();
                }
                catch (InvalidOperationException)
                {
                    return -1;//bad id
                }
                return ut.id;
            }
            else
            {
                return 0;
            }            
        }

        public int AddProjectLink(int tagId, int projectId, string tagType)
        {
            ProjectTags pt = new ProjectTags();
            pt.tagId = tagId;
            pt.projectId = projectId;
            pt.tagType = tagType;
            VestnDB db = new VestnDB();
            ProjectTags fromDatabase = db.projectTags.Where(x => x.tagId == tagId).Where(y => y.projectId == projectId).FirstOrDefault();
            if (fromDatabase == null)
            {
                try
                {
                    db.projectTags.Add(pt);
                    db.SaveChanges();
                }
                catch (InvalidOperationException)
                {
                    return -1;//bad id
                }
                return pt.id;
            }
            else
            {
                return 0;
            } 
            
        }

        public bool removeProjectLink(int tagId, int projectId)
        {
            VestnDB db = new VestnDB();
            ProjectTags pt = db.projectTags.Where(p => p.projectId == projectId).Where(t => t.tagId == tagId).FirstOrDefault();
            try
            {

                db.projectTags.Attach(pt);
                pt = db.projectTags.Remove(pt);
                db.SaveChanges();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool removeUserLink(int tagId, int userId)
        {
            VestnDB db = new VestnDB();
            UserTags ut = db.userTags.Where(u => u.userId == userId).Where(t => t.tagId == tagId).FirstOrDefault();
            try
            {

                db.userTags.Attach(ut);
                ut = db.userTags.Remove(ut);
                db.SaveChanges();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public List<Tag> GetUserTags(int userId)
        {
            VestnDB db = new VestnDB();
            List<UserTags> utList = db.userTags.Where(u => u.userId == userId).ToList();
            List<Tag> tags = new List<Tag>();
            foreach(UserTags entry in utList)
            {
                if (entry.tagType == "s")
                {
                    tags.Add(GetSTag(entry.tagId));
                }
                else if (entry.tagType == "f")
                {
                    tags.Add(GetFTag(entry.tagId));
                }
            }
            return tags;
        }

        public List<Tag> GetProjectTags(int projectId)
        {
            VestnDB db = new VestnDB();
            List<ProjectTags> ptList = db.projectTags.Where(u => u.projectId == projectId).ToList();
            List<Tag> tags = new List<Tag>();
            foreach (ProjectTags entry in ptList)
            {
                if (entry.tagType == "s")
                {
                    tags.Add(GetSTag(entry.tagId));
                }
                else if (entry.tagType == "f")
                {
                    tags.Add(GetFTag(entry.tagId));
                }
            }
            return tags;
        }

        public List<sTag> GetAllSTags()
        {
            VestnDB db = new VestnDB();
            return db.sTag.ToList();
        }

        public List<string> GetAllSTagValues()
        {
            VestnDB db = new VestnDB();
            List<sTag> sTags = db.sTag.ToList();
            List<string> tags = new List<string>();
            foreach (sTag s in sTags)
            {
                tags.Add(s.value);
            }
            return tags;
        }

        public List<sTag> GetAllParents(int id)
        {
            sTag sTag = GetSTag(id);
            List<sTag> parents = new List<sTag>();
            while (sTag.parentId != 0)
            {
                parents.Add(GetSTag(sTag.parentId));
                sTag = GetSTag(sTag.parentId);
            }
            return parents;
        }

        public List<fTag> GetAllFTags()
        {
            VestnDB db = new VestnDB();
            return db.fTag.ToList();
        }
    }

}
