using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Accessor;
using Entity;

namespace Manager
{
    public class TagManager
    {
        LogAccessor logAccessor = new LogAccessor();
        TagAccessor tagAccessor = new TagAccessor();
        public string TestMe()
        {
            return "success";
        }

        public sTag CreateSTag(int parentId, string value)//set parentId to 0 when it is at the top level
        {
            return tagAccessor.CreateSTag(parentId, value);
        }

        public fTag CreateFTag(int parentId, string value)//set parentId to 0 when it is at the top level
        {
            return tagAccessor.CreateFTag(parentId, value);
        }

        public sTag GetSTag(int id)
        {
            return tagAccessor.GetSTag(id);
        }

        public sTag GetSTag(string value)
        {
            return tagAccessor.GetSTag(value);
        }

        public fTag GetFTag(int id)
        {
            return tagAccessor.GetFTag(id);
        }

        public fTag GetFTag(string value)
        {
            return tagAccessor.GetFTag(value);
        }

        public sTag UpdateSTag(sTag tag)
        {
            return tagAccessor.UpdateSTag(tag);
        }

        public fTag UpdateFTag(fTag tag)
        {
            return tagAccessor.UpdateFTag(tag);
        }

        public sTag DeleteSTag(sTag tag)
        {
            return tagAccessor.DeleteSTag(tag);
        }

        public fTag DeleteFTag(fTag tag)
        {
            return tagAccessor.DeleteFTag(tag);
        }

        public bool RemoveProjectLink(int tagId, int projectId, string type)
        {
            return tagAccessor.removeProjectLink(tagId, projectId, type);
        }

        public string AddTag(string value, string tagType, int ownerId)//tagType is s of f
        {
            int x = -99;
            try
            {
                Tag tag = new Tag();

                if (tagType == "s")
                {
                    tag = tagAccessor.GetSTag(value);
                }
                else if (tagType == "f" && tagAccessor.GetFTag(value) != null)
                {
                    //fTag is already in fTag table, so we'll pull it rather than creating a new one
                    tag = tagAccessor.GetFTag(value);
                }
                else if (tagType == "f" && tagAccessor.GetFTag(value) == null)
                {
                    //fTag doesn't already exist, so let's add it to the fTag table
                    tag = CreateFTag(0, value);
                }
                x = tagAccessor.AddProjectLink(tag.id, ownerId, tagType);
            }
            catch (Exception)
            {
                return null;
            }
            if (x > 0)
            {
                return value;
            }
            else if (x == 0)
            {
                return "Tag already added.";
            }
            else
            {
                return null;
            }
        }

        public List<string> getAllUserTags(int userId)
        {
            List<Tag> userTags = tagAccessor.GetUserTags(userId);
            List<string> userTagStrings = new List<string>();
            foreach (Tag t in userTags)
            {
                userTagStrings.Add(t.value);
            }
            return userTagStrings;
        }

        public List<string> getAllProjectTags(int projectId)
        {
            List<Tag> projectTags = tagAccessor.GetProjectTags(projectId);
            List<string> projectTagStrings = new List<string>();
            foreach (Tag t in projectTags)
            {
                projectTagStrings.Add(t.value);
            }
            return projectTagStrings;
        }

        public List<Tag> GetProjectTags(int projectId)
        {
            return tagAccessor.GetProjectTags(projectId);
        }

        public List<sTag> GetAllSTags()
        {
            return tagAccessor.GetAllSTags();
        }

        public List<string> GetAllSTagValues()
        {
            return tagAccessor.GetAllSTagValues();
        }

        public List<fTag> GetAllFTags()
        {
            return tagAccessor.GetAllFTags();
        }



        /*
        public List<Tag> GetAllLowestLevel()
        {
            List<Tag> allTags = new List<Tag>();
            List<Tag> allLowest = new List<Tag>();
            allTags = GetAllTags();
            foreach (Tag t in allTags)
            {
                if (t.children == null)
                {
                    allLowest.Add(t);
                }
            }
            return allLowest;
        }

        public List<Tag> GetAllHighestLevel()
        {
            List<Tag> allTags = new List<Tag>();
            List<Tag> allHighest = new List<Tag>();
            allTags = GetAllTags();
            foreach (Tag t in allTags)
            {
                if (t.parents == null)
                {
                    allHighest.Add(t);
                }
            }
            return allHighest;
        }
        //
        public Tag AddParents(Tag t, List<int> parents)
        {
            if (!CheckIdExists(parents))
            {
                return null;
            }
            foreach (int i in parents)
            {
                List<int> currentParents = new List<int>();
                currentParents = GetIds(t.parents);
                if (currentParents == null)
                {
                    currentParents = new List<int>();
                }
                if (!currentParents.Contains(i))
                {
                    if (t.parents == null)
                    {
                        t.parents += i.ToString();
                    }
                    else
                    {
                        t.parents += "," + i.ToString();
                    }
                }
                Tag parentToUpdate = GetTag(i);
                parentToUpdate = AddChild(parentToUpdate, t.id);
                UpdateTag(t);
            }
            return t;
        }
        //
        public Tag AddParents(Tag t, string newParents)
        {
            List<int> i = GetIds(newParents);
            return AddParents(t,i);
        }
        //
        public Tag AddParent(Tag t, int newParent)
        {
            List<int> currentParents = GetIds(t.parents);
            if (currentParents == null)
            {
                currentParents = new List<int>();
            }
            if (!currentParents.Contains(newParent))
            {
                if (!CheckIdExists(newParent.ToString()))
                {
                    return null;
                }
                if (t.parents == null)
                {
                    t.parents += newParent.ToString();
                }
                else
                {
                    t.parents += "," + newParent.ToString();
                }
                UpdateTag(t);
                Tag parentToUpdate = GetTag(newParent);
                parentToUpdate = AddChild(parentToUpdate, t.id);
            }
            return t;
        }
        //
        public Tag DeleteParent(Tag t, int parentTagId)
        {
            List<int> i = GetIds(t.parents);
            int previousLength = i.Count();
            i.Remove(parentTagId);
            if (i.Count == previousLength)
            {
                return t;
            }
            t.parents = ListToString(i);
            UpdateTag(t);
            DeleteChild(GetTag(parentTagId), t.id);
            return t;
        }
        //
        public Tag AddChildren(Tag t, List<int> children)
        {
            if (!CheckIdExists(children))
            {
                return null;
            }
            foreach (int i in children)
            {
                List<int> currentChildren = new List<int>();
                currentChildren = GetIds(t.children);
                if (currentChildren == null)
                {
                    currentChildren = new List<int>();
                }
                if (!currentChildren.Contains(i))
                {
                    if (t.children == null)
                    {
                        t.children += i.ToString();
                    }
                    else
                    {
                        t.children += "," + i.ToString();
                    }
                }
                Tag childToUpdate = GetTag(i);
                childToUpdate = AddParent(childToUpdate, t.id);
                UpdateTag(t);
            }
            return t;
        }
        //
        public Tag AddChildren(Tag t, string newChildren)
        {
            List<int> i = GetIds(newChildren);
            return AddChildren(t, i);
        }
        //
        public Tag AddChild(Tag t, int newChild)
        {
            List<int> currentChildren = GetIds(t.children);
            if (currentChildren == null)
            {
                currentChildren = new List<int>();
            }
            if (!currentChildren.Contains(newChild))
            {
                if (!CheckIdExists(newChild.ToString()))
                {
                    return null;
                }
                if (t.children == null)
                {
                    t.children += newChild.ToString();
                }
                else
                {
                    t.children += "," + newChild.ToString();
                }
                UpdateTag(t);
                Tag childToUpdate = GetTag(newChild);
                childToUpdate = AddParent(childToUpdate, t.id);
            }
            return t;
        }
        //
        public Tag DeleteChild(Tag t, int childTagId)
        {
            List<int> i = GetIds(t.children);
            int previousLength = i.Count();
            i.Remove(childTagId);
            if (i.Count == previousLength)
            {
                return t;
            }
            t.children = ListToString(i);
            UpdateTag(t);
            DeleteParent(GetTag(childTagId), t.id);
            return t;
        }
        //
        public List<Tag> GetTags(string tagIds)
        {
            List<Tag> tags = new List<Tag>();
            List<int> list = GetIds(tagIds);
            return GetTags(list);
        }
        //
        public List<Tag> GetTags(List<int> tagIds)
        {
            List<Tag> tags = new List<Tag>();
            foreach (int i in tagIds)
            {
                try
                {
                    tags.Add(GetTag(i));
                }
                catch (Exception e) {
                    logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), e.ToString());
                };
            }

            return tags;
        }
        //
        public String ListToString(List<int> list)
        {
            string s = null;
            foreach (int i in list)
            {
                s += i.ToString() + ",";
            }
            s = s.Remove(s.Length - 1);
            return s;
        }

        public List<Tag> GetParents(List<Tag> tags, List<Tag> current)
        {
            List<Tag> currentParents = new List<Tag>();
            foreach (Tag t in current)//adds list of parentTags to currentParents
            {
                List<int> CurrentParentIds = new List<int>();
                string sParents = GetTag(t.id).parents;
                //CurrentParentIds = ;
                CurrentParentIds = GetIds(sParents);
                //IEnumerable<int> distinctIDs = CurrentParentIds.Distinct();
                if (CurrentParentIds == null)
                {
                    return tags;
                }
                foreach (int i in CurrentParentIds)
                {
                    currentParents.Add(GetTag(i));
                    tags.Add(GetTag(i));
                }
            }

            List<Tag> distinctTags = new List<Tag>();
            List<Tag> distinctParents = new List<Tag>();
            distinctTags = Distinct(tags);
            distinctParents = Distinct(currentParents);
            return GetParents(distinctTags, distinctParents);
        }

        public List<int> GetIds(string sParents)
        {
            string[] s =null;
            try
            {
                s = sParents.Split(',');
            }
            catch (Exception e)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), e.ToString());
                return null;
            }
            List<int> ids = new List<int>();
            foreach (string c in s)
            {
                int value;
                int.TryParse(c, out value);
                ids.Add(value);
            }
            return ids;
        }

        public List<Tag> Distinct(List<Tag> allTags)
        {
            List<Tag> distinctTags = new List<Tag>();
            List<int> distinctIds = new List<int>();
            foreach (Tag t in allTags)
            {  
                if (!distinctTags.Contains(t) && !distinctIds.Contains(t.id))
                {
                    distinctTags.Add(t);
                    distinctIds.Add(t.id);
                }
            }
            return distinctTags;
        }

        public bool CheckIdExists(string ids)
        {
            if (ids == null)
            {
                return true;
            }
            List<int> listIds = GetIds(ids);
            return CheckIdExists(listIds);

        }

        public bool CheckIdExists(List<int> ids)
        {
            if (ids == null)
            {
                return true;
            }
            foreach (int i in ids)
            {
                try
                {
                    GetTag(i);
                }
                catch (Exception e)
                {
                    logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), e.ToString());
                    return false;
                }
            }
            return true;
        }

        public Tag CreateTag(string parents, string children, string value)
        {

            if (CheckIdExists(children) && CheckIdExists(parents))
            {
                return tagAccessor.CreateTag(parents, children, value);
            }
            else
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), "Create tag failed because either the parent or child did not exist.");
                return null;
            }
        }

        public List<Tag> GetAllTags()
        {
            return tagAccessor.GetAllTags();
        }

        public Tag GetTag(int id)
        {
            return tagAccessor.GetTag(id);
        }

        public Tag GetTagByValue(string value)//shortened version for easy method calls
        {
            return tagAccessor.GetTagByValue(value);
        }

        public Tag DeleteTag(Tag tag)
        {
            return tagAccessor.DeleteTag(tag);
        }

        public Tag UpdateTag(Tag tag)
        {
            return tagAccessor.UpdateTag(tag);
        }
         * */
    }
}
