using freelance.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace freelance.Controllers
{
    public class PartialController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public PartialController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            db = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public ActionResult CreateProject(int id)
        {
            string jwtToken = Request.Cookies["prologs"];

            if (ContextManager.IsJwtTokenValid(jwtToken))
            {
                ViewBag.IdUser = id;
                return PartialView("CreateProject");
            }
            else
                return Redirect("~/Logon/Login");
        }

        public EmptyResult CreateNewProject(int userid, string name, string desc, DateTime start, DateTime end)
        {
            var newproj = new Project()
            {
                Name = name,
                Description = desc,
                StartDate = start,
                EndDate = end,
                Status = ProjectStatus.Запланированный,
                UserId = userid
            };

            db.Projects.Add(newproj);
            db.SaveChanges();

            var newusersprojects = new UserProject()
            {
                UserId = userid,
                ProjectId = newproj.Id
            };

            db.UsersProjects.Add(newusersprojects);
            db.SaveChanges();

            var user = db.Users.FirstOrDefault(q => q.Id == userid);
            ContextManager.AddLog(db, "Создание проекта", $"Фрилансер {user.Name} (ID = {user.Id}) выполнил действие: \"Создание проекта\" - {newproj.Name} (ID = {newproj.Id}).", user);

            return new EmptyResult();
        }

        [HttpGet]
        public ActionResult EditProject(int projectid)
        {
            string jwtToken = Request.Cookies["prologs"];

            if (ContextManager.IsJwtTokenValid(jwtToken))
            {
                var jwt = Request.Cookies["prologs"];
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(jwt);
                var userId = token.Claims.First(c => c.Type == "userId").Value;

                User user = db.Users.FirstOrDefault(m => m.Id == int.Parse(userId));

                var project = db.Projects.FirstOrDefault(q => q.Id == projectid);

                var user_projs = db.UsersProjects.FirstOrDefault(q => q.ProjectId == project.Id && q.UserId == user.Id);
                if (user_projs != null)
                {
                    ViewBag.Project = project;
                    return PartialView("EditProject");
                }
                else
                    return new EmptyResult();
            }
            else
                return Redirect("~/Logon/Login");
        }

        public EmptyResult EditingProject(Project project)
        {
            var proj = db.Projects.FirstOrDefault(q => q.Id == project.Id);
            proj.Name = project.Name;
            proj.Description = project.Description;
            proj.StartDate = project.StartDate;
            proj.EndDate = project.EndDate;
            proj.Status = ContextManager.ConvertProjectStatus2(project.Status.ToString());

            if (project.Status.ToString() == "Завершенный")
            {
                proj.EndDate = DateTime.Today;
            }

            db.Projects.Update(proj);
            db.SaveChanges();

            var user = db.Users.FirstOrDefault(q => q.Id == proj.UserId);
            ContextManager.AddLog(db, "Редактирование проекта", $"Фрилансер {user.Name} (ID = {user.Id}) выполнил действие: \"Редактирование проекта\" - {proj.Name} (ID = {proj.Id}).", user);

            return new EmptyResult();
        }

        [HttpGet]
        public ActionResult CreateTask(int projectid)
        {
            string jwtToken = Request.Cookies["prologs"];

            if (ContextManager.IsJwtTokenValid(jwtToken))
            {
                var jwt = Request.Cookies["prologs"];
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(jwt);
                var userId = token.Claims.First(c => c.Type == "userId").Value;

                User user = db.Users.FirstOrDefault(m => m.Id == int.Parse(userId));
                var model = new ContextManager { CurrentUser = user, Users = db.Users.ToList(), Projects = db.Projects.ToList() };
                ViewBag.Project = db.Projects.FirstOrDefault(q => q.Id == projectid);
                ViewData["Title"] = "Создание задачи";
                return PartialView("CreateTask", model);
            }
            else
                return Redirect("~/Logon/Login");
        }

        public EmptyResult CreateNewTask(Models.Task task, List<IFormFile> files)
        {
            var newtask = new Models.Task()
            {
                Name = task.Name,
                Description = task.Description,
                Priority = ContextManager.ConvertTaskPriority(task.Priority.ToString()),
                Status = Models.TaskStatus.Новый,
                CreationDate = task.CreationDate,
                DueDate = task.DueDate,
                UserId = task.UserId,
                ProjectId = task.ProjectId
            };

            db.Tasks.Add(newtask);
            db.SaveChanges();

            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;

                    Models.File newFile = new Models.File
                    {
                        FileName = uniqueFileName,
                        FileType = file.ContentType,
                        FilePath = uniqueFileName,
                        TaskId = newtask.Id
                    };

                    string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    db.Files.Add(newFile);
                    db.SaveChanges();
                }
            }

            var user = db.Users.FirstOrDefault(q => q.Id == newtask.UserId);
            ContextManager.AddLog(db, "Создание задачи", $"Фрилансер {user.Name} (ID = {user.Id}) выполнил действие: \"Создание задачи\" - {newtask.Name} (ID = {newtask.Id}).", user);

            return new EmptyResult();
        }

        public EmptyResult EditingTask(Models.Task task)
        {
            if (task.Status.ToString() == "Завершенный")
            {
                task.DueDate = DateTime.Today;
            }

            db.Tasks.Update(task);
            db.SaveChanges();

            var user = db.Users.FirstOrDefault(q => q.Id == task.UserId);
            ContextManager.AddLog(db, "Редактирование задачи", $"Фрилансер {user.Name} (ID = {user.Id}) выполнил действие: \"Редактирование задачи\" - {task.Name} (ID = {task.Id}).", user);

            return new EmptyResult();
        }

        [HttpGet]
        public ActionResult QrCode(string link)
        {
            ViewBag.QrLink = link;
            return PartialView("QrCode");
        }

        [HttpGet]
        public ActionResult ReadTask(int taskId)
        {
            string jwtToken = Request.Cookies["prologs"];

            if (ContextManager.IsJwtTokenValid(jwtToken))
            {
                var jwt = Request.Cookies["prologs"];
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(jwt);
                var userId = token.Claims.First(c => c.Type == "userId").Value;

                User user = db.Users.FirstOrDefault(m => m.Id == int.Parse(userId));
                var model = new ContextManager { CurrentUser = user, Users = db.Users.ToList(), Projects = db.Projects.ToList(), Files = db.Files.ToList() };

                var task = db.Tasks.FirstOrDefault(q => q.Id == taskId);
                ViewBag.Task = task;

                if (task.UserId != user.Id)
                    ViewBag.MyTask = false;
                else
                    ViewBag.MyTask = true;

                var proj = db.Projects.FirstOrDefault(q => q.Id == task.ProjectId);
                var user_projs = db.UsersProjects.FirstOrDefault(q => q.ProjectId == proj.Id && q.UserId == user.Id);
                if (user_projs != null)
                    return PartialView("ReadTask", model);
                else
                    return new EmptyResult();
            }
            else
                return Redirect("~/Home/Login");
        }

        [HttpGet]
        public ActionResult EditTask(int taskId)
        {
            string jwtToken = Request.Cookies["prologs"];

            if (Request.Cookies["prologs"] != null)
            {
                var jwt = Request.Cookies["prologs"];
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(jwt);
                var userId = token.Claims.First(c => c.Type == "userId").Value;

                User user = db.Users.FirstOrDefault(m => m.Id == int.Parse(userId));
                var model = new ContextManager { CurrentUser = user, Users = db.Users.ToList(), Projects = db.Projects.ToList(), Files = db.Files.ToList() };

                ViewBag.Task = db.Tasks.FirstOrDefault(q => q.Id == taskId);

                if (ViewBag.Task.UserId != user.Id)
                    ViewBag.MyTask = false;
                else
                    ViewBag.MyTask = true;

                var proj = db.Projects.FirstOrDefault(q => q.Id == ViewBag.Task.ProjectId);
                var user_projs = db.UsersProjects.FirstOrDefault(q => q.ProjectId == proj.Id && q.UserId == user.Id);
                if (user_projs != null)
                    return PartialView("EditTask", model);
                else
                    return new EmptyResult();
            }
            else
                return Redirect("~/Home/Login");
        }

        public IActionResult DownloadFile(string fileName)
        {
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", fileName);

            if (System.IO.File.Exists(filePath))
            {
                string[] parts = fileName.Split('_');
                string filedownloadname = parts[1];

                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, "application/octet-stream", filedownloadname);
            }

            return NotFound();
        }

        public EmptyResult DeleteFileInTask(int file)
        {
            var myfile = db.Files.FirstOrDefault(q => q.Id == file);
            db.Files.Remove(myfile);
            db.SaveChanges();

            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
            var filePath = Path.Combine(uploadsFolder, myfile.FileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            var task = db.Tasks.FirstOrDefault(q => q.Id == myfile.TaskId);
            var user = db.Users.FirstOrDefault(q => q.Id == task.UserId);
            ContextManager.AddLog(db, "Удаление файла задачи", $"Фрилансер {user.Name} (ID = {user.Id}) выполнил действие: \"Удаление файла задачи\" - {myfile.FileName} (ID = {myfile.Id}).", user);

            return new EmptyResult();
        }

        public EmptyResult DeleteTask(int taskid)
        {
            var task = db.Tasks.FirstOrDefault(q => q.Id == taskid);
            db.Tasks.Remove(task);
            db.SaveChanges();

            var user = db.Users.FirstOrDefault(q => q.Id == task.UserId);
            ContextManager.AddLog(db, "Удаление задачи", $"Фрилансер {user.Name} (ID = {user.Id}) выполнил действие: \"Удаление задачи\" - {task.Name} (ID = {task.Id}).", user);

            return new EmptyResult();
        }

        [HttpGet]
        public ActionResult OpenLogs(int id)
        {
            ViewBag.User = db.Users.FirstOrDefault(q => q.Id == id);
            ViewBag.UserRole = db.Roles.FirstOrDefault(w => w.Id == db.Users.FirstOrDefault(q => q.Id == id).RoleId).Name;
            ViewBag.Logs = db.Logs.Where(q => q.UserId == id);
            return PartialView("OpenLogs");
        }
    }
}
