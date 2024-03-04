using freelance.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace freelance.Controllers
{
    public class LogonController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IConfiguration _configuration;
        public LogonController(ApplicationDbContext context, IConfiguration configuration)
        {
            db = context;
            _configuration = configuration;
        }

        public IActionResult Login()
        {
            ViewData["Title"] = "Авторизация";
            return View();
        }

        [HttpPost]
        public IActionResult Login(string useremail, string userpassword)
        {
            var user = db.Users.FirstOrDefault(q => q.Email == useremail && q.PasswordHash == ContextManager.Hashing(userpassword));

            if (user == null)
            {
                return Redirect("~/Logon/Login");
            }

            if (user.isRemoved == true)
            {
                return Redirect("~/Logon/Login");
            }

            if (user != null)
            {
                var jwtConfig = _configuration.GetSection("JwtConfig").Get<JwtConfig>();
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.RoleId.ToString())
                };
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(jwtConfig.Secret);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim("userId", user.Id.ToString()),
                        new Claim("username", user.Name),
                        new Claim("email", user.Email),
                        new Claim("role", user.RoleId.ToString())
                    }),
                    Expires = DateTime.UtcNow.AddMinutes(jwtConfig.ExpirationInMinutes),
                    NotBefore = DateTime.UtcNow,
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                Response.Cookies.Append(jwtConfig.CookieName, tokenHandler.WriteToken(token), new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTime.UtcNow.AddMinutes(jwtConfig.ExpirationInMinutes)
                });

                if (user.RoleId == 1)
                {
                    ContextManager.AddLog(db, "Авторизация", $"Администратор {user.Name} (ID = {user.Id}) выполнил действие: \"Авторизация\".", user);
                    return Redirect("~/Admin/Index");
                }

                var completedDeadlineTasks = db.Tasks.Where(q => q.DueDate.Date < DateTime.Today && q.Status != Models.TaskStatus.Завершенный).ToList();
                foreach (var task in completedDeadlineTasks)
                {
                    ContextManager.SendNotificationsMessage(db, false, task.Id, user.Id);
                }

                var completedDeadlineProjects = db.Projects.Where(q => q.EndDate < DateTime.Today && q.Status != ProjectStatus.Завершенный).ToList();
                foreach (var project in completedDeadlineProjects)
                {
                    ContextManager.SendNotificationsMessage(db, true, project.Id, user.Id);
                }

                return Redirect("~/Home/Index");
            }
            else
                return Redirect("~/Logon/Login");
        }

        public IActionResult Register()
        {
            ViewData["Title"] = "Регистрация";
            return View();
        }

        [HttpPost]
        public IActionResult Register(string username, string useremail, string userpassword)
        {
            var isHaveAccount = db.Users.FirstOrDefault(q => q.Email == useremail);

            if (isHaveAccount != null)
            {
                return Redirect("~/Logon/Login");
            }

            User user = new User() { Name = username, Email = useremail, RoleId = 2, PasswordHash = ContextManager.Hashing(userpassword), isRemoved = false };
            db.Users.Add(user);
            db.SaveChanges();

            var notificationRulesToAdd = new List<NotificationRules>
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
            db.SaveChanges();

            return Redirect("~/Logon/Login");
        }

        public int UserLogout()
        {
            Response.Cookies.Delete("prologs");
            return 1;
        }

        public IActionResult Recovery()
        {
            ViewData["Title"] = "Восстановления пароля";
            return View();
        }

        public IActionResult SendEmailForRecovery(string email)
        {
            User user = db.Users.FirstOrDefault(m => m.Email == email);

            if (user != null)
            {
                ContextManager.SendMessageToEmail(
                    email,
                    "Вам отправлен код для восстановления пароля!",
                    $"Здравствуйте, {email}.<br>Ваш код восстановления {ContextManager.GenerateCodeForRecoveryPassword()}<br><br><br> --------------------- <br> 2024 - ProLogs");
                return new EmptyResult();
            }
            else
            {
                return Redirect("~/Logon/Login");
            }
        }

        public EmptyResult UpdatePasswordInRecovery(string email, string code, string newpassword)
        {
            User user = db.Users.FirstOrDefault(m => m.Email == email);
            if (code == ContextManager.CurentCode)
            {
                user.PasswordHash = ContextManager.Hashing(newpassword);
                db.Update(user);
                db.SaveChanges();
            }

            return new EmptyResult();
        }
    }
}
