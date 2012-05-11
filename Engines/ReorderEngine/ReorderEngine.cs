using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entity;
using Accessor;

namespace Engine
{
    public class ReorderEngine
    {
        public User ReOrderProjects(User model)
        {
            try
            {
                if (model == null)
                {
                    return null;
                }
                if (model.projectOrder != null)
                {
                    List<Project> reorderedProjects = new List<Project>();
                    List<int> orderList = stringOrderToList(model.projectOrder);
                    //model.projects = new List<Project>();
                    foreach (int x in orderList)
                    {
                        reorderedProjects.Add(model.projects.Where(u => u.id == x).FirstOrDefault());
                        //model.projects.Add(projectManager.GetProject(x));
                    }
                    model.projects = reorderedProjects;
                    List<ProjectElement> returnedElements;
                    foreach (Project p in model.projects)
                    {
                        returnedElements = new List<ProjectElement>();
                        if (p.projectElementOrder != null)
                        {
                            returnedElements = ReOrderProjectElements(p, p.projectElementOrder);
                            if (returnedElements != null)
                            {
                                p.projectElements = returnedElements;
                            }
                        }
                    }
                }
                return model;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public List<ProjectElement> ReOrderProjectElements(Project p, string projectElementOrder)
        {
            if (projectElementOrder != null)
            {
                List<ProjectElement> reorderedProjectElements = new List<ProjectElement>();
                List<int> orderList = stringOrderToList(projectElementOrder);
                //model.projects = new List<Project>();
                foreach (int x in orderList)
                {
                    reorderedProjectElements.Add(p.projectElements.Where(u => u.id == x).FirstOrDefault());
                    //model.projects.Add(projectManager.GetProject(x));
                }
                return reorderedProjectElements;
            }
            else
            {
                return null;
            }

        }

        public List<int> stringOrderToList(string projectOrder)
        {
            LogAccessor logAccessor = new LogAccessor();
            string[] s = null;
            try
            {
                s = projectOrder.Split(',');
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

    }

}
