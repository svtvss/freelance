using freelance.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;

namespace freelance.Models
{
    public class ContextManager
    {
        public User CurrentUser { get; set; }
        public List<User> Users { get; set; }
        public List<Task> Tasks { get; set; }
        public List<File> Files { get; set; }
        public List<Project> Projects { get; set; }
        public List<Role> Roles { get; set; }
        public List<UserProject> UsersProjects { get; set; }

        public static string Hashing(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    builder.Append(hashBytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }

        /// <summary>
        /// Отправление письма на почту
        /// </summary>
        /// <param name="email"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        public async static void SendMessageToEmail(string email, string subject, string body)
        {
            try
            {
                MailAddress from = new MailAddress("albina_prolog@inbox.ru");
                MailAddress to = new MailAddress(email);

                MailMessage message = new MailMessage(from, to)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                SmtpClient smtp = new SmtpClient("smtp.inbox.ru", 2525);
                smtp.Credentials = new NetworkCredential("albina_prolog@inbox.ru", "h2vhbkTy0GE0tewNpeV7");
                smtp.EnableSsl = true;

                await smtp.SendMailAsync(message);
                Console.WriteLine($"Успешно");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отправке сообщения: {ex.Message}");
            }
        }

        public static string CurentCode = string.Empty;
        public static string GenerateCodeForRecoveryPassword()
        {
            Random random = new Random();
            string code = random.Next(1000, 9999).ToString();
            CurentCode = code;
            return code;
        }

        public static void SendNotificationsMessage(ApplicationDbContext db, bool type, int taskOrProjectId, int userid)
        {
            if (type)
            {
                var project = db.Projects.FirstOrDefault(q => q.Id == taskOrProjectId);
                if (project != null)
                {
                    var notificationName = $"Закончился дедлайн у проекта {project.Name}";

                    var existingNotification = db.Notifications.FirstOrDefault(q => q.Name == notificationName && q.UserId == userid);

                    if (existingNotification == null)
                    {
                        var newNoti = new Notification()
                        {
                            Name = notificationName,
                            InvoiceDate = project.EndDate.AddDays(1),
                            isRemoved = false,
                            UserId = userid
                        };

                        db.Notifications.Add(newNoti);
                        db.SaveChanges();
                    }
                }
            }
            else
            {
                var task = db.Tasks.FirstOrDefault(q => q.Id == taskOrProjectId);
                var project = db.Projects.FirstOrDefault(q => q.Id == task.ProjectId);

                if (project != null && task != null)
                {
                    var notificationName = $"Закончился дедлайн у задачи {task.Name} в проекте {project.Name}";

                    var existingNotification = db.Notifications.FirstOrDefault(q => q.Name == notificationName && q.UserId == userid);

                    if (existingNotification == null)
                    {
                        var newNoti = new Notification()
                        {
                            Name = notificationName,
                            InvoiceDate = task.DueDate.AddDays(1),
                            isRemoved = false,
                            UserId = userid
                        };

                        db.Notifications.Add(newNoti);
                        db.SaveChanges();
                    }
                }
            }
        }

        public static string ConvertProjectStatus(string status)
        {
            List<string> allstatus = new List<string>() { "Запланированный", "В процессе", "Завершенный" };
            var result = allstatus.FindIndex(a => a == status);

            if (result == 0)
                return "Запланированный";

            if (result == 1)
                return "Впроцессе";

            if (result == 2)
                return "Завершенный";

            return string.Empty;
        }

        public static TaskPriority ConvertTaskPriority(string input)
        {
            if (input == "Низкий")
                return TaskPriority.Низкий;
            if (input == "Средний")
                return TaskPriority.Средний;
            if (input == "Высокий")
                return TaskPriority.Высокий;

            return 0;
        }

        public static ProjectStatus ConvertProjectStatus2(string input)
        {
            if (input == "Запланированный")
                return ProjectStatus.Запланированный;
            if (input == "Впроцессе")
                return ProjectStatus.Впроцессе;
            if (input == "Завершенный")
                return ProjectStatus.Завершенный;

            return 0;
        }

        public static TaskStatus ConvertTaskStatus(string input)
        {
            if (input == "Новый")
                return TaskStatus.Новый;
            if (input == "Впроцессе")
                return TaskStatus.Впроцессе;
            if (input == "Завершенный")
                return TaskStatus.Завершенный;

            return 0;
        }

        public static string ConvertTaskStatusRevert(TaskStatus input)
        {
            if (input == TaskStatus.Новый)
                return "Новый";
            if (input == TaskStatus.Впроцессе)
                return "В процессе";
            if (input == TaskStatus.Завершенный)
                return "Завершенный";

            return string.Empty;
        }

        public static string ConvertTaskPriorityRevert(TaskPriority input)
        {
            if (input == TaskPriority.Низкий)
                return "Низкий";
            if (input == TaskPriority.Средний)
                return "Средний";
            if (input == TaskPriority.Высокий)
                return "Высокий";

            return string.Empty;
        }

        public static string ConvertProjectStatusRevert(ProjectStatus input)
        {
            if (input == ProjectStatus.Запланированный)
                return "Запланированный";
            if (input == ProjectStatus.Впроцессе)
                return "В процессе";
            if (input == ProjectStatus.Завершенный)
                return "Завершенный";

            return string.Empty;
        }

        public static List<string> SearchSkill(string taskDescription)
        {
            taskDescription = taskDescription.ToLower();
            string[] keySkills = {
                "javascript", "html", "css", "python", "react", "node.js", "design", "database", "mobile", "ui/ux", "php", "с#", "с++", "java", "angular", "vue", "рефакторинг", "git", "gitlab", "azure", "bootstrap", "docker", ".net", "golang", "linux",
                "swift", "java", "android", "ios", "react native", "flutter", "xcode", "ui/ux", "api", "mssql", "sql", "mysql", "postgresql", "postgre", "postman", "sqlite", "typescript", "vuejs", "redis",
                "photoshop", "illustrator", "indesign", "typography", "branding", "layout", "vector", "color theory", "adobe creative suite", "figma",
                "data analysis", "statistics", "machine learning", "data visualization", "excel", "tableau", "data mining", "1c",
                "writing", "blogging", "copywriting", "editing", "seo", "content marketing", "social media", "proofreading", "research"
            };

            List<string> skillsFound = new List<string>();

            foreach (var skill in keySkills)
            {
                if (taskDescription.Contains(skill, StringComparison.OrdinalIgnoreCase))
                {
                    skillsFound.Add(skill);
                }
            }

            return skillsFound;
        }

        public static void AddLog(ApplicationDbContext db, string action, string description, User user)
        {
            var newLog = new Log()
            {
                Action = action,
                Timestamp = DateTime.Now,
                Description = description,
                UserId = user.Id
            };

            db.Logs.Add(newLog);
            db.SaveChanges();
        }

        public static bool IsJwtTokenValid(string jwtToken)
        {
            return !string.IsNullOrEmpty(jwtToken);
        }

        public static string CreatePassword(int length)
        {
            const string valid = "123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%&()";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }
    }
}
