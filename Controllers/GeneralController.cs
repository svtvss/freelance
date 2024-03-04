using freelance.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace freelance.Controllers
{
    public class GeneralController : Controller
    {
        private readonly ApplicationDbContext db;
        public GeneralController(ApplicationDbContext context)
        {
            db = context;
        }

        public IActionResult Account()
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
                ViewData["Title"] = "Профиль";
                return View(model);
            }
            else
                return Redirect("~/Logon/Login");
        }

        public int UpdateUsername(int userid, string username)
        {
            try
            {
                var user = db.Users.FirstOrDefault(q => q.Id == userid);
                string oldusername = user.Name;

                user.Name = username;
                db.Users.Update(user);
                db.SaveChanges();

                ContextManager.AddLog(db, "Редактирование имени пользователя", $"Фрилансер {user.Name} (ID = {user.Id}) выполнил действие: \"Редактирование имени пользователя\". Старое имя: {oldusername}, новое имя: {user.Name}.", user);

                return 1;
            }
            catch
            {
                return 0;
            }
        }

        public int UpdatePassword(int userid, string oldpass, string newpass)
        {
            var user = db.Users.FirstOrDefault(q => q.Id == userid);

            if (user.PasswordHash == ContextManager.Hashing(oldpass))
            {
                return 2;
            }

            if (ContextManager.Hashing(oldpass) == ContextManager.Hashing(newpass))
            {
                return 3;
            }

            user.PasswordHash = ContextManager.Hashing(newpass);
            db.Users.Update(user);
            db.SaveChanges();

            ContextManager.AddLog(db, "Изменение пароля", $"Фрилансер {user.Name} (ID = {user.Id}) выполнил действие: \"Изменение пароля\". Старый пароль: {oldpass}", user);

            var notiules = db.NotificationRules.Where(q => q.UserId == user.Id && q.NotiName == "Уведомления об изменении пароля").ToList();
            if (notiules.Count == 1)
            {
                if (notiules.ElementAt(0).NotiFrom == "Email")
                {
                    ContextManager.SendMessageToEmail(user.Email,
                        "Изменение пароля от учетной записи!",
                        $"Здравствуйте, {user.Email}.<br>Ваш пароль от учетной записи был изменен!<br><br>Если это сделали не вы, то восстановите доступ с помощью функции восстановления пароля! Также рекомендуем обратиться в поддержку: albina_prolog@inbox.ru<br><br><br> --------------------- <br> 2024 - ProLogs");
                }
                else if (notiules.ElementAt(0).NotiFrom == "ProLogs")
                {
                    db.Notifications.Add(new Notification()
                    {
                        Name = "Изменен пароль от учетной записи",
                        InvoiceDate = DateTime.Now,
                        UserId = user.Id,
                        isRemoved = false
                    });
                    db.SaveChanges();
                }
            }
            else if (notiules.Count == 2)
            {
                ContextManager.SendMessageToEmail(user.Email,
                        "Изменение пароля от учетной записи!",
                        $"Здравствуйте, {user.Email}.<br>Ваш пароль от учетной записи был изменен!<br><br>Если это сделали не вы, то восстановите доступ с помощью функции восстановления пароля! Также рекомендуем обратиться в поддержку: albina_prolog@inbox.ru<br><br><br> --------------------- <br> 2024 - ProLogs");

                db.Notifications.Add(new Notification()
                {
                    Name = "Изменен пароль от учетной записи",
                    InvoiceDate = DateTime.Now,
                    UserId = user.Id,
                    isRemoved = false
                });
                db.SaveChanges();
            }

            return 1;
        }

        public int RemoveAccount(int userid)
        {
            var user = db.Users.FirstOrDefault(q => q.Id == userid);
            user.isRemoved = true;
            db.Users.Update(user);
            db.SaveChanges();

            ContextManager.AddLog(db, "Удаление аккаунта", $"Фрилансер {user.Name} (ID = {user.Id}) выполнил действие: \"Удаление аккаунта\".", user);

            return 1;
        }

        public IActionResult Settings()
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
                ViewData["Title"] = "Настройка уведомлений";

                /*var notificationRulesToAdd = new List<NotificationRules>
                {
                    new NotificationRules
                    {
                        NotiName = "Уведомления об изменении пароля", NotiFrom = "Email", isWork = true, UserId = user.Id
                    },
                    new NotificationRules
                    {
                        NotiName = "Уведомления о новостях ProLogs", NotiFrom = "Email", isWork = true, UserId = user.Id
                    },
                    new NotificationRules
                    {
                        NotiName = "Уведомления об просроченности дедлайна проектов", NotiFrom = "ProLogs", isWork = true, UserId = user.Id
                    },
                    new NotificationRules
                    {
                        NotiName = "Уведомления об просроченности дедлайна задач", NotiFrom = "ProLogs", isWork = true, UserId = user.Id
                    },
                    new NotificationRules
                    {
                        NotiName = "Уведомления об изменении пароля", NotiFrom = "ProLogs", isWork = true,UserId = user.Id
                    },
                    new NotificationRules
                    {
                        NotiName = "Уведомления о новостях ProLogs", NotiFrom = "ProLogs", isWork = true, UserId = user.Id
                    }
                };

                var existingRules = db.NotificationRules
                    .Where(q => q.UserId == user.Id)
                    .Select(q => new { q.NotiFrom, q.NotiName })
                    .ToList();

                var rulesToAdd = notificationRulesToAdd
                    .Where(rule => !existingRules.Contains(new { rule.NotiFrom, rule.NotiName }))
                    .ToList();

                db.NotificationRules.AddRange(rulesToAdd);
                db.SaveChanges();*/

                ViewBag.MyNoti = db.NotificationRules.Where(q => q.UserId == user.Id).ToList();

                return View(model);
            }
            else
                return Redirect("~/Logon/Login");
        }

        public EmptyResult UpdateNotifications([FromBody] List<NotificationUpdateModel> notifications)
        {
            int userid = 0;
            foreach (var a in notifications)
            {
                var noti = db.NotificationRules.FirstOrDefault(q => q.Id == int.Parse(a.Id));
                noti.isWork = a.IsWork;
                db.NotificationRules.Update(noti);
                db.SaveChanges();

                userid = noti.UserId;
            }

            var user = db.Users.FirstOrDefault(q => q.Id == userid);
            ContextManager.AddLog(db, "Настройка уведомлений", $"Фрилансер {user.Name} (ID = {user.Id}) выполнил действие: \"Настройка уведомлений\".", user);

            return new EmptyResult();
        }

        public EmptyResult UserLogout()
        {
            Response.Cookies.Delete("prologs");
            return new EmptyResult();
        }
    }
}
