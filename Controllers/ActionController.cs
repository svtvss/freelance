using freelance.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace freelance.Controllers
{
    public class ActionController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ActionController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            db = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Projects(string? name, DateTime? date, string? status)
        {
            string jwtToken = Request.Cookies["prologs"];

            if (ContextManager.IsJwtTokenValid(jwtToken))
            {
                var jwt = Request.Cookies["prologs"];
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(jwt);
                var userId = token.Claims.First(c => c.Type == "userId").Value;

                User user = db.Users.FirstOrDefault(m => m.Id == int.Parse(userId));
                var model = new ContextManager { CurrentUser = user, Users = db.Users.ToList(), Tasks = db.Tasks.ToList(), UsersProjects = db.UsersProjects.ToList() };

                var myusersprojects = db.UsersProjects.Where(q => q.UserId == user.Id).ToList();
                var myprojects = new List<Project>();

                foreach (var p in myusersprojects)
                {
                    myprojects.Add(db.Projects.FirstOrDefault(q => q.Id == p.ProjectId));
                }

                ViewData["Title"] = "Проекты";

                List<Project> allprojects = myprojects;

                if (name != null)
                {
                    if (name != "all")
                    {
                        if (name == "asc")
                        {
                            allprojects = allprojects.OrderBy(q => q.Name).ToList();
                            ViewBag.Name = "asc";
                        }
                        else if (name == "desc")
                        {
                            allprojects = allprojects.OrderByDescending(q => q.Name).ToList();
                            ViewBag.Name = "desc";
                        }
                    }
                    else
                    {
                        ViewBag.Name = "all";
                    }
                }

                if (date != null)
                {
                    allprojects = allprojects.Where(q => q.StartDate == date).ToList();
                }

                if (status != null)
                {
                    if (status != "all")
                    {
                        if (status == "Запланированный")
                            ViewBag.Status = "Запланированный";
                        else if (status == "Впроцессе")
                            ViewBag.Status = "Впроцессе";
                        else if (status == "Завершенный")
                            ViewBag.Status = "Завершенный";

                        allprojects = allprojects.Where(q => q.Status == ContextManager.ConvertProjectStatus2(status)).ToList();
                    }
                    else
                    {
                        ViewBag.Status = "all";
                    }
                }

                ViewBag.SortedProjects = allprojects.OrderBy(p => p.Status).ToList();

                return View(model);
            }
            else
                return Redirect("~/Home/Login");
        }

        public IActionResult Project(int projid)
        {
            string jwtToken = Request.Cookies["prologs"];

            if (ContextManager.IsJwtTokenValid(jwtToken))
            {
                var jwt = Request.Cookies["prologs"];
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(jwt);
                var userId = token.Claims.First(c => c.Type == "userId").Value;

                User user = db.Users.FirstOrDefault(m => m.Id == int.Parse(userId));
                var model = new ContextManager { CurrentUser = user, Users = db.Users.ToList(), Tasks = db.Tasks.ToList(), Files = db.Files.ToList() };

                var proj = db.Projects.FirstOrDefault(q => q.Id == projid);

                if (proj.UserId != user.Id)
                    ViewBag.MyProject = false;
                else
                    ViewBag.MyProject = true;

                var user_projs = db.UsersProjects.FirstOrDefault(q => q.ProjectId == projid && q.UserId == user.Id);
                if (user_projs == null)
                    return Redirect("~/Action/Projects");

                ViewBag.ThisProject = proj;
                var tasksinproj = db.Tasks.Where(q => q.ProjectId == proj.Id);

                List<freelance.Models.Task> tasksInProgress = tasksinproj.Where(q => q.Status == Models.TaskStatus.Впроцессе).ToList();
                List<freelance.Models.Task> tasksNew = tasksinproj.Where(q => q.Status == Models.TaskStatus.Новый).ToList();
                List<freelance.Models.Task> tasksComplete = tasksinproj.Where(q => q.Status == Models.TaskStatus.Завершенный).ToList();

                ViewBag.TasksInProgress = tasksInProgress; ViewBag.TasksNew = tasksNew; ViewBag.TasksComplete = tasksComplete;
                var authors = db.UsersProjects.Where(q => q.ProjectId == projid).ToList();
                ViewBag.Authors = authors;

                ViewData["Title"] = "Проект " + proj.Name;
                return View(model);
            }
            else
                return Redirect("~/Logon/Login");
        }

        public EmptyResult ChangeTaskStatus(int taskid, string status)
        {
            var task = db.Tasks.FirstOrDefault(q => q.Id == taskid);
            task.Status = ContextManager.ConvertTaskStatus(status);

            if (status == "Завершенный")
            {
                task.DueDate = DateTime.Today;
            }

            db.Tasks.Update(task);
            db.SaveChanges();

            var user = db.Users.FirstOrDefault(q => q.Id == task.UserId);
            ContextManager.AddLog(db, "Редактирование статуса задачи", $"Фрилансер {user.Name} (ID = {user.Id}) выполнил действие: \"Редактирование статуса задачи\" - {task.Name} (ID = {task.Id}).", user);

            return new EmptyResult();
        }

        public IActionResult AddUserProject(int projectId)
        {
            string jwtToken = Request.Cookies["prologs"];

            if (ContextManager.IsJwtTokenValid(jwtToken))
            {
                var jwt = Request.Cookies["prologs"];
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(jwt);
                var userId = token.Claims.First(c => c.Type == "userId").Value;

                User user = db.Users.FirstOrDefault(m => m.Id == int.Parse(userId));
                var model = new ContextManager { CurrentUser = user, Users = db.Users.ToList() };

                var iamHaveThisProject = db.UsersProjects.FirstOrDefault(q => q.UserId == user.Id && q.ProjectId == projectId);

                if (iamHaveThisProject == null)
                {
                    var newusersprojects = new UserProject()
                    {
                        UserId = user.Id,
                        ProjectId = projectId
                    };
                    db.UsersProjects.Add(newusersprojects);
                    db.SaveChanges();
                }

                return Redirect($"~/Action/Project?projid={projectId}");
            }
            else
                return Redirect("~/Logon/Login");
        }

        public IActionResult Tasks(string? namesort, DateTime? creationdatesort, string? statussort, string? prioritysort)
        {
            string jwtToken = Request.Cookies["prologs"];

            if (ContextManager.IsJwtTokenValid(jwtToken))
            {
                var jwt = Request.Cookies["prologs"];
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(jwt);
                var userId = token.Claims.First(c => c.Type == "userId").Value;

                User user = db.Users.FirstOrDefault(m => m.Id == int.Parse(userId));
                var model = new ContextManager { CurrentUser = user, Users = db.Users.ToList(), Files = db.Files.ToList(), Projects = db.Projects.ToList() };
                ViewData["Title"] = "Задачи";

                var alltasks = db.Tasks.Where(q => q.UserId == user.Id).OrderBy(q => q.Status).ToList();

                if (namesort != null)
                {
                    if (namesort != "all")
                    {
                        if (namesort == "asc")
                        {
                            alltasks = alltasks.OrderBy(q => q.Name).ToList();
                            ViewBag.Name = "asc";
                        }
                        else if (namesort == "desc")
                        {
                            alltasks = alltasks.OrderByDescending(q => q.Name).ToList();
                            ViewBag.Name = "desc";
                        }
                    }
                    else
                    {
                        ViewBag.Name = "all";
                    }
                }

                if (creationdatesort != null)
                {
                    alltasks = alltasks.Where(q => q.CreationDate == creationdatesort).ToList();
                }

                if (statussort != null)
                {
                    if (statussort != "all")
                    {
                        if (statussort == "Новый")
                            ViewBag.Status = "Новый";
                        else if (statussort == "Впроцессе")
                            ViewBag.Status = "Впроцессе";
                        else if (statussort == "Завершенный")
                            ViewBag.Status = "Завершенный";

                        alltasks = alltasks.Where(q => q.Status == ContextManager.ConvertTaskStatus(statussort)).ToList();
                    }
                    else
                    {
                        ViewBag.Status = "all";
                    }
                }

                if (prioritysort != null)
                {
                    if (prioritysort != "all")
                    {
                        if (prioritysort == "Низкий")
                            ViewBag.Priority = "Низкий";
                        else if (prioritysort == "Средний")
                            ViewBag.Priority = "Средний";
                        else if (prioritysort == "Высокий")
                            ViewBag.Priority = "Высокий";

                        alltasks = alltasks.Where(q => q.Priority == ContextManager.ConvertTaskPriority(prioritysort)).ToList();
                    }
                    else
                    {
                        ViewBag.Priority = "all";
                    }
                }

                ViewBag.SortedTasks = alltasks;

                return View(model);
            }
            else
                return Redirect("~/Logon/Login");
        }
    }
}
