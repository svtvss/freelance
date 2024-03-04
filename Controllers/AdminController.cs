using freelance.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace freelance.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext db;
        public AdminController(ApplicationDbContext context)
        {
            db = context;
        }

        public IActionResult Index(string name, string email, int? role, int? status)
        {
            string jwtToken = Request.Cookies["prologs"];

            if (ContextManager.IsJwtTokenValid(jwtToken))
            {
                var jwt = Request.Cookies["prologs"];
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(jwt);
                var userId = token.Claims.First(c => c.Type == "userId").Value;

                User user = db.Users.FirstOrDefault(m => m.Id == int.Parse(userId));
                var model = new ContextManager { CurrentUser = user, Users = db.Users.ToList(), Roles = db.Roles.ToList() };

                var allusers = db.Users.OrderBy(q => q.Id).ToList();

                if (user.RoleId == 1)
                {
                    if (!string.IsNullOrEmpty(name))
                    {
                        allusers = allusers.Where(q => q.Name.Contains(name)).ToList();
                        ViewBag.SelectedName = name;
                    }

                    if (!string.IsNullOrEmpty(email))
                    {
                        allusers = allusers.Where(q => q.Email.Contains(email)).ToList();
                        ViewBag.SelectedEmail = email;
                    }

                    if (role != null)
                    {
                        allusers = allusers.Where(q => q.RoleId == role).ToList();
                        ViewBag.SelectedRole = role;
                    }

                    if (status != null)
                    {
                        allusers = allusers.Where(q => q.isRemoved == Convert.ToBoolean(status)).ToList();
                        ViewBag.SelectedStatus = status;
                    }

                    ViewBag.AllUsers = allusers;
                    return View(model);
                }
                else
                    return Redirect("~/Home/Index");
            }
            else
                return Redirect("~/Logon/Login");
        }

        public IActionResult ActionsUser(string type, int srchuser)
        {
            string jwtToken = Request.Cookies["prologs"];
            string action = string.Empty;

            if (ContextManager.IsJwtTokenValid(jwtToken))
            {
                var jwt = Request.Cookies["prologs"];
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(jwt);
                var userId = token.Claims.First(c => c.Type == "userId").Value;

                User user = db.Users.FirstOrDefault(m => m.Id == int.Parse(userId));
                var model = new ContextManager { CurrentUser = user, Users = db.Users.ToList(), Roles = db.Roles.ToList() };

                if (user.RoleId == 1)
                {
                    var searchuser = db.Users.FirstOrDefault(q => q.Id == srchuser);
                    if (type == "del")
                    {
                        searchuser.isRemoved = true;
                        action = "Удаление пользователя";
                    }
                    else if (type == "rec")
                    {
                        searchuser.isRemoved = false;
                        action = "Восстановление пользователя";
                    }
                    db.Users.Update(searchuser);
                    db.SaveChanges();

                    ContextManager.AddLog(db, action, $"Администратор {user.Name} (ID = {user.Id}) выполнил действие: \"{action}\" по отношению к фрилансеру {searchuser.Name} (ID = {searchuser.Id}).", user);

                    return Redirect("~/Admin/Index");
                }
                else
                    return Redirect("~/Logon/Login");
            }
            else
                return Redirect("~/Logon/Login");
        }

        public IActionResult SendProLogsNews(string message)
        {
            var users = db.Users.Where(q => !q.isRemoved && q.RoleId == 2).ToList();

            foreach (var user in users)
            {
                var notificationRule = db.NotificationRules
                    .Where(q => q.UserId == user.Id && q.NotiName == "Уведомления о новостях ProLogs").ToList();

                if (notificationRule.Count == 1)
                {
                    if (notificationRule.ElementAt(0).NotiFrom == "Email")
                    {
                        ContextManager.SendMessageToEmail(user.Email, $"Новости ProLogs от {DateTime.Now.ToLongDateString()}", message);
                    }
                    else if (notificationRule.ElementAt(0).NotiFrom == "ProLogs")
                    {
                        var notification = new Notification
                        {
                            Name = $"Новости ProLogs:\n {message}",
                            InvoiceDate = DateTime.Now,
                            isRemoved = false,
                            UserId = user.Id
                        };

                        db.Notifications.Add(notification);
                        db.SaveChanges();
                    }
                }
                else if (notificationRule.Count == 2)
                {
                    ContextManager.SendMessageToEmail(user.Email, $"Новости ProLogs от {DateTime.Now.ToLongDateString()}", message);

                    var notification = new Notification
                    {
                        Name = $"Новости ProLogs:\n {message}",
                        InvoiceDate = DateTime.Now,
                        isRemoved = false,
                        UserId = user.Id
                    };

                    db.Notifications.Add(notification);
                    db.SaveChanges();
                }
            }

            return RedirectToAction("Index", "Admin");
        }

        public IActionResult ResetPassword(int userid)
        {
            string newpassword = ContextManager.CreatePassword(8);

            try
            {
                var user = db.Users.FirstOrDefault(q => q.Id == userid);
                user.PasswordHash = ContextManager.Hashing(newpassword);
                db.Users.Update(user);
                db.SaveChanges();

                ContextManager.SendMessageToEmail(
                    user.Email, 
                    "ProLogs - Новый пароль",
                    "<p>Администрация ProLogs поменяла вам пароль после обращения на почту на тему сброса пароля.<br>" +
                    "Никто кроме вас не знает текущий пароль. Если вы получили письмо случайно, то рекомендуем сменить пароль, любым из возможных способов.<br>" +
                    $"Email: {user.Email}<br>" +
                    $"Password: {newpassword}<br><br><br>------------------------------------------<br>2024 - ProLogs</p>");

                return RedirectToAction("Index", "Admin");
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}
