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
    public class ProjectAccessor
    {
        LogAccessor logAccessor = new LogAccessor();
        public string TestMe()
        {
            return "success";
        }

        public Project CreateProject(User u, List<ProjectElement> projectElements)
        {

            Project project = new Project(){ isActive = true };
            project.projectElements = projectElements;

            try
            {
                if (u.projects == null)
                {
                    u.projects = new List<Project>();
                }

                if (u.projects.Count() == 0)
                {
                    project.name = "About";
                }
                else
                {
                    project.name = "New Project";
                }

                u.projects.Add(project);

                VestnDB db = new VestnDB();
                db.projects.Add(project);
                db.Entry(u).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception e)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), e.ToString());
                return null;
            }

            try
            {
                //save ordering with new project
                if (u.projects.Count() == 1)
                {
                    u.projectOrder += project.id;
                }
                else
                {
                    u.projectOrder += "," + project.id;
                }
                VestnDB db = new VestnDB();
                db.Entry(u).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception e)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), e.ToString());
                return null;
            }

            return project;
        }

        public int AddProjectElement(Project project, ProjectElement pe)
        {
            if (project == null || pe == null)
            {
                return -1;
            }

            try
            {
                project.projectElements.Add(pe);
                project.dateModified = DateTime.Now;

                VestnDB db = new VestnDB();
                db.projectElements.Add(pe);
                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception e)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), e.ToString());
                return -1;
            }

            try
            {
                if (project.projectElementOrder == null)
                {
                    if (project.projectElements.Count > 1)
                    {
                        project = resetProjectElementOrder(project);
                        //project.projectElementOrder += "," + pe.id;
                    }
                    else
                    {
                        project.projectElementOrder += pe.id;
                    }
                }
                else
                {
                    //project.projectElementOrder += "," + pe.id;
                    project.projectElementOrder = pe.id + "," + project.projectElementOrder; // add new element to the begininning of the order
                }
                VestnDB db = new VestnDB();
                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception e)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), e.ToString());
                return -1;
            }

            return pe.id;
        }

        public Project resetProjectElementOrder(Project p)
        {
            string newProjectOrder = null;
            foreach (ProjectElement pe in p.projectElements)
            {
                newProjectOrder += pe.id + " ";
            }
            if (newProjectOrder != null)
            {
                p.projectElementOrder = newProjectOrder.TrimEnd().Replace(' ', ',');
            }
            else
            {
                p.projectElementOrder = null;
            }
            return p;
        }

        public Project UpdateProject(Project project)
        {
            

            if (GetProject(project.id) == null)
            {
                return null;
            }
            try
            {
                VestnDB db = new VestnDB();
                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception e)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), e.ToString());
                return null;
            }
            return project;

        }

        public ProjectElement UpdateProjectElement(ProjectElement pe)
        {
            if (GetProjectElement(pe.id) == null)
            {
                return null;
            }
            try
            {
                VestnDB db = new VestnDB();
                db.Entry(pe).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception e)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), e.ToString());
                return null;
            }
            return pe;
        }

        public Project GetProject(int id)
        {
            VestnDB db = new VestnDB();
            Project projectFound;
            try
            {
                projectFound = db.projects.Where(p => p.id == id).Include(e => e.projectElements).FirstOrDefault();
            }
            catch (Exception e)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), e.ToString());
                projectFound = null;
            }

            return projectFound;

        }

        public ProjectElement GetProjectElement(int id)
        {
            VestnDB db = new VestnDB();
            ProjectElement elementFound;
            try
            {
                elementFound = db.projectElements.Where(p => p.id == id).FirstOrDefault();
            }
            catch (Exception e)
            {
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), e.ToString());
                elementFound = null;
            }

            return elementFound;

        }

        public ProjectElement DeleteProjectElement(ProjectElement pe)
        {
            VestnDB db = new VestnDB();
            db.projectElements.Attach(pe);
            db.projectElements.Remove(pe);
            db.SaveChanges();
            return pe;
        }

        public Project DeleteProject(Project p)
        {
            VestnDB db = new VestnDB();
            db.projects.Attach(p);
            List<ProjectElement> peList = new List<ProjectElement>();
            foreach (ProjectElement pe in p.projectElements)
            {
                peList.Add(pe);
            }
            foreach (ProjectElement pe in peList)
            {
                db.projectElements.Remove(pe);
            }
            db.projects.Remove(p);
            db.SaveChanges();
            return p;
        }

        public List<ProjectElement_Experience> GetExperiences(User u)
        {
            List<ProjectElement_Experience> experiences = new List<ProjectElement_Experience>();
            foreach (Project p in u.projects)
            {
                try
                {
                    foreach (ProjectElement_Experience pe in p.projectElements)
                    {
                        experiences.Add(pe);
                    }
                }
                catch (Exception) { };
                    
            }
            return experiences;
        }
    }
}
