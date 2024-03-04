using freelance.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.Globalization;
using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace freelance.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public HomeController(ApplicationDbContext context, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            db = context;
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
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

                if (user.RoleId == 1)
                {
                    return Redirect("~/Admin/Index");
                }

                return View(model);
            }
            else
                return Redirect("~/Logon/Login");
        }

        

        

        

        

        [HttpGet]
        public ActionResult GetCompletedTasksData(int userid)
        {
            DateTime currentDate = DateTime.Now;

            DateTime sixMonthsAgo = currentDate.AddMonths(-5);

            var completedTasks = db.Tasks.Where(task => task.Status == Models.TaskStatus.Завершенный && task.DueDate >= sixMonthsAgo && task.UserId == userid).ToList();

            var chartData = new List<ChartData>();

            for (DateTime date = sixMonthsAgo; date <= currentDate; date = date.AddMonths(1))
            {
                var tasksInMonth = completedTasks
                    .Count(task => task.DueDate.Month == date.Month && task.DueDate.Year == date.Year);

                chartData.Add(new ChartData
                {
                    Month = date.ToString("MMMM"),
                    CompletedTasks = tasksInMonth
                });
            }

            return Json(chartData);
        }

        [HttpGet]
        public ActionResult GetCompletedProjectsData(int userid)
        {
            DateTime currentDate = DateTime.Now;

            DateTime sixMonthsAgo = currentDate.AddMonths(-5);

            var completedProjects = db.Projects.Where(project => project.Status == ProjectStatus.Завершенный && project.EndDate >= sixMonthsAgo && project.UserId == userid).ToList();

            var chartData = new List<ChartData>();

            for (DateTime date = sixMonthsAgo; date <= currentDate; date = date.AddMonths(1))
            {
                var projectsInMonth = completedProjects.Count(project => project.EndDate.Month == date.Month && project.EndDate.Year == date.Year);

                chartData.Add(new ChartData
                {
                    Month = date.ToString("MMMM"),
                    CompletedTasks = projectsInMonth
                });
            }

            return Json(chartData);
        }

        

        

        

		public IActionResult Notifications()
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
				ViewData["Title"] = "Уведомления";

                ViewBag.MyNoti = db.Notifications.Where(q => q.UserId == user.Id && q.isRemoved == false).OrderByDescending(a => a.InvoiceDate.Date).ToList();

				return View(model);
			}
			else
				return Redirect("~/Home/Login");
		}

        public IActionResult DeleteNotification(int notificationid)
        {
            var noti = db.Notifications.FirstOrDefault(q => q.Id == notificationid);
            noti.isRemoved = true;
            db.Notifications.Update(noti);
            db.SaveChanges();

            var user = db.Users.FirstOrDefault(q => q.Id == noti.UserId);
            ContextManager.AddLog(db, "Удаление уведомления", $"Фрилансер {user.Name} (ID = {user.Id}) выполнил действие: \"Удаление уведомления\" - {noti.Name} (ID = {noti.Id}).", user);

            return Redirect("~/Home/Notifications");
        }

        public IActionResult Reports()
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

                ViewBag.CompleteProjects = db.Projects.Where(q => q.UserId == user.Id && q.Status == ProjectStatus.Завершенный).ToList();
                ViewData["Title"] = "Отчеты";
                return View(model);
            }
            else
                return Redirect("~/Home/Login");
        }

        

        

        

        

        

        public ActionResult ReportN1(int userid, string format)
        {
            var user = db.Users.FirstOrDefault(q => q.Id == userid);
            var tasks = db.Tasks.Where(r => r.UserId == userid && r.Status == Models.TaskStatus.Завершенный).ToList();
            var averageTimePerTask = tasks.Select(task => (task.DueDate - task.CreationDate).TotalDays).Average();

            if (tasks.Count > 0)
            {
                if (format == "Excel")
                {
                    int lastrow = 0;

                    using (XLWorkbook workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Выполненные задачи");

                        worksheet.Column(1).Width = 50;
                        worksheet.Column(2).Width = 50;
                        worksheet.Column(3).Width = 30;
                        worksheet.Column(4).Width = 30;
                        worksheet.Column(5).Width = 30;
                        worksheet.Column(6).Width = 25;
                        worksheet.Column(7).Width = 30;
                        worksheet.Column(8).Width = 30;

                        worksheet.Cell(1, 1).Value = "Наименование";
                        worksheet.Cell(1, 2).Value = "Наименование проекта";
                        worksheet.Cell(1, 3).Value = "Описание";
                        worksheet.Cell(1, 4).Value = "Дата начала";
                        worksheet.Cell(1, 5).Value = "Дата завершения";
                        worksheet.Cell(1, 6).Value = "Количество файлов";
                        worksheet.Cell(1, 7).Value = "Статус";
                        worksheet.Cell(1, 8).Value = "Приоритет";
                        worksheet.Row(1).Style.Font.Bold = true;

                        for (int i = 0; i < tasks.Count; i++)
                        {
                            worksheet.Cell(i + 2, 1).Value = tasks[i].Name;
                            worksheet.Cell(i + 2, 2).Value = db.Projects.FirstOrDefault(q => q.Id == tasks[i].ProjectId).Name;
                            worksheet.Cell(i + 2, 3).Value = tasks[i].Description;
                            worksheet.Cell(i + 2, 4).Value = tasks[i].CreationDate.ToShortDateString();
                            worksheet.Cell(i + 2, 5).Value = tasks[i].DueDate.ToShortDateString();
                            worksheet.Cell(i + 2, 6).Value = db.Files.Where(q => q.TaskId == tasks[i].Id).Count();
                            worksheet.Cell(i + 2, 7).Value = ContextManager.ConvertTaskStatusRevert(tasks[i].Status);
                            worksheet.Cell(i + 2, 8).Value = ContextManager.ConvertTaskPriorityRevert(tasks[i].Priority);
                            lastrow = i + 1;
                        }

                        worksheet.Cell(lastrow + 2, 6).Value = "Среднее количество дней затраченное на задачу:";
                        worksheet.Cell(lastrow + 2, 7).Value = averageTimePerTask;

                        using (var stream = new MemoryStream())
                        {
                            workbook.SaveAs(stream);
                            stream.Flush();

                            return new FileContentResult(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                            {
                                FileDownloadName = $"Отчет по выполненным задачам {user.Name} от {DateTime.Now.Date.ToString("D")}.xlsx"
                            };
                        }
                    }
                }
                else if (format == "Pdf")
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        Document document = new Document(PageSize.A4, 10, 10, 10, 10);
                        PdfWriter writer = PdfWriter.GetInstance(document, stream);

                        document.Open();

                        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                        Encoding.GetEncoding("windows-1252");

                        BaseFont baseFont = BaseFont.CreateFont(@"C:\Windows\Fonts\ARIAL.ttf", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                        iTextSharp.text.Font headerfont = new iTextSharp.text.Font(baseFont, iTextSharp.text.Font.DEFAULTSIZE, iTextSharp.text.Font.NORMAL);

                        Paragraph paragraph = new Paragraph($"Отчет по выполненным задачам от {DateTime.Now.Date.ToString("D")}", headerfont);
                        paragraph.Alignment = Element.ALIGN_CENTER;
                        paragraph.Font.Size = 16;
                        paragraph.Font.IsBold();
                        paragraph.SpacingAfter = 10f;
                        document.Add(paragraph);

                        var headerStyle = new PdfPCell();
                        headerStyle.BackgroundColor = BaseColor.WHITE;
                        headerStyle.Border = Rectangle.BOX;
                        headerStyle.HorizontalAlignment = Element.ALIGN_CENTER;

                        float[] columnWidths = new float[] { 30f, 30f, 42f, 20f, 20f, 18f, 20f, 20f };
                        PdfPTable table = new PdfPTable(columnWidths);

                        Phrase col1 = new Phrase("Наименование", headerfont);
                        col1.Font.Size = 12;
                        PdfPCell cell = new PdfPCell(col1);
                        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        table.AddCell(cell);

                        Phrase col2 = new Phrase("Наименование проекта", headerfont);
                        col2.Font.Size = 12;
                        cell = new PdfPCell(col2);
                        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        table.AddCell(cell);

                        Phrase col3 = new Phrase("Описание", headerfont);
                        col3.Font.Size = 12;
                        cell = new PdfPCell(col3);
                        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        table.AddCell(cell);

                        Phrase col4 = new Phrase("Дата начала", headerfont);
                        col4.Font.Size = 12;
                        cell = new PdfPCell(col4);
                        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        table.AddCell(cell);

                        Phrase col5 = new Phrase("Дата завершения", headerfont);
                        col5.Font.Size = 12;
                        cell = new PdfPCell(col5);
                        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        table.AddCell(cell);

                        Phrase col6 = new Phrase("Количество файлов", headerfont);
                        col6.Font.Size = 12;
                        cell = new PdfPCell(col6);
                        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        table.AddCell(cell);

                        Phrase col7 = new Phrase("Статус", headerfont);
                        col7.Font.Size = 12;
                        cell = new PdfPCell(col7);
                        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        table.AddCell(cell);

                        Phrase col8 = new Phrase("Приоритет", headerfont);
                        col8.Font.Size = 12;
                        cell = new PdfPCell(col8);
                        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        table.AddCell(cell);

                        iTextSharp.text.Font cellFont = new iTextSharp.text.Font(baseFont, 12, iTextSharp.text.Font.NORMAL);

                        foreach (var task in tasks)
                        {
                            PdfPCell cellName = new PdfPCell(new Phrase(task.Name, cellFont));
                            PdfPCell cellProjectName = new PdfPCell(new Phrase(db.Projects.FirstOrDefault(q => q.Id == task.ProjectId).Name, cellFont));
                            PdfPCell cellDescription = new PdfPCell(new Phrase(task.Description, cellFont));
                            PdfPCell cellCreationDate = new PdfPCell(new Phrase(task.CreationDate.ToShortDateString(), cellFont));
                            PdfPCell cellDueDate = new PdfPCell(new Phrase(task.DueDate.ToShortDateString(), cellFont));
                            PdfPCell cellCountFiles = new PdfPCell(new Phrase(db.Files.Where(q => q.TaskId == task.Id).Count().ToString(), cellFont));
                            PdfPCell cellPriority = new PdfPCell(new Phrase(ContextManager.ConvertTaskPriorityRevert(task.Priority), cellFont));

                            table.AddCell(cellName);
                            table.AddCell(cellProjectName);
                            table.AddCell(cellDescription);
                            table.AddCell(cellCreationDate);
                            table.AddCell(cellDueDate);
                            table.AddCell(cellCountFiles);
                            table.AddCell(cellPriority);
                        }

                        document.Add(table);

                        Paragraph paragraph2 = new Paragraph($"Среднее количество дней затраченное на задачу: {Math.Round(averageTimePerTask, 2)}", headerfont);
                        paragraph2.Font.Size = 14;
                        paragraph2.SpacingBefore = 10f;
                        document.Add(paragraph2);

                        document.Close();

                        return new FileContentResult(stream.GetBuffer(), "application/pdf")
                        {
                            FileDownloadName = $"Отчет по выполненным задачам {user.Name} от {DateTime.Now.Date.ToString("D")}.pdf"
                        };
                    }
                }
            }
            return Redirect("~/Home/Reports");
        }

        public ActionResult ReportN2(int userid, string format)
        {
            var user = db.Users.FirstOrDefault(q => q.Id == userid);
            var tasks = db.Tasks.Where(r => r.UserId == userid).ToList();

            if (tasks.Count > 0)
            {
                if (format == "Excel")
                {
                    int lastrow = 0;

                    using (XLWorkbook workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Все задачи");

                        worksheet.Column(1).Width = 50;
                        worksheet.Column(2).Width = 50;
                        worksheet.Column(3).Width = 30;
                        worksheet.Column(4).Width = 30;
                        worksheet.Column(5).Width = 30;
                        worksheet.Column(6).Width = 25;
                        worksheet.Column(7).Width = 30;
                        worksheet.Column(8).Width = 30;

                        worksheet.Cell(1, 1).Value = "Наименование";
                        worksheet.Cell(1, 2).Value = "Наименование проекта";
                        worksheet.Cell(1, 3).Value = "Описание";
                        worksheet.Cell(1, 4).Value = "Дата начала";
                        worksheet.Cell(1, 5).Value = "Дата завершения";
                        worksheet.Cell(1, 6).Value = "Количество файлов";
                        worksheet.Cell(1, 7).Value = "Статус";
                        worksheet.Cell(1, 8).Value = "Приоритет";

                        worksheet.Row(1).Style.Font.Bold = true;

                        for (int i = 0; i < tasks.Count; i++)
                        {
                            worksheet.Cell(i + 2, 1).Value = tasks[i].Name;
                            worksheet.Cell(i + 2, 2).Value = db.Projects.FirstOrDefault(q => q.Id == tasks[i].ProjectId).Name;
                            worksheet.Cell(i + 2, 3).Value = tasks[i].Description;
                            worksheet.Cell(i + 2, 4).Value = tasks[i].CreationDate.ToShortDateString();
                            worksheet.Cell(i + 2, 5).Value = tasks[i].DueDate.ToShortDateString();
                            worksheet.Cell(i + 2, 6).Value = db.Files.Where(q => q.TaskId == tasks[i].Id).Count();
                            worksheet.Cell(i + 2, 7).Value = ContextManager.ConvertTaskStatusRevert(tasks[i].Status);
                            worksheet.Cell(i + 2, 8).Value = ContextManager.ConvertTaskPriorityRevert(tasks[i].Priority);
                            lastrow = i + 1;
                        }

                        using (var stream = new MemoryStream())
                        {
                            workbook.SaveAs(stream);
                            stream.Flush();

                            return new FileContentResult(stream.ToArray(),
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                            {
                                FileDownloadName = $"Отчет по всем задачам {user.Name} от {DateTime.Now.Date.ToString("D")}.xlsx"
                            };
                        }
                    }
                }
                else if (format == "Pdf")
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        Document document = new Document();
                        PdfWriter writer = PdfWriter.GetInstance(document, stream);

                        document.Open();

                        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                        Encoding.GetEncoding("windows-1252");

                        BaseFont baseFont = BaseFont.CreateFont(@"C:\Windows\Fonts\ARIAL.ttf", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                        iTextSharp.text.Font headerfont = new iTextSharp.text.Font(baseFont, iTextSharp.text.Font.DEFAULTSIZE, iTextSharp.text.Font.NORMAL);

                        Paragraph paragraph = new Paragraph($"Отчет по всем задачам от {DateTime.Now.Date.ToString("D")}", headerfont);
                        paragraph.Alignment = Element.ALIGN_CENTER;
                        paragraph.Font.Size = 16;
                        paragraph.Font.IsBold();
                        paragraph.SpacingAfter = 10f;
                        document.Add(paragraph);

                        var headerStyle = new PdfPCell();
                        headerStyle.BackgroundColor = BaseColor.WHITE;
                        headerStyle.Border = Rectangle.BOX;
                        headerStyle.HorizontalAlignment = Element.ALIGN_CENTER;

                        float[] columnWidths = new float[] { 30f, 30f, 42f, 20f, 20f, 18f, 20f, 20f };
                        PdfPTable table = new PdfPTable(columnWidths);

                        Phrase col1 = new Phrase("Наименование", headerfont);
                        col1.Font.Size = 12;
                        PdfPCell cell = new PdfPCell(col1);
                        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        table.AddCell(cell);

                        Phrase col2 = new Phrase("Наименование проекта", headerfont);
                        col2.Font.Size = 12;
                        cell = new PdfPCell(col2);
                        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        table.AddCell(cell);

                        Phrase col3 = new Phrase("Описание", headerfont);
                        col3.Font.Size = 12;
                        cell = new PdfPCell(col3);
                        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        table.AddCell(cell);

                        Phrase col4 = new Phrase("Дата начала", headerfont);
                        col4.Font.Size = 12;
                        cell = new PdfPCell(col4);
                        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        table.AddCell(cell);

                        Phrase col5 = new Phrase("Дата завершения", headerfont);
                        col5.Font.Size = 12;
                        cell = new PdfPCell(col5);
                        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        table.AddCell(cell);

                        Phrase col6 = new Phrase("Количество файлов", headerfont);
                        col6.Font.Size = 12;
                        cell = new PdfPCell(col6);
                        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        table.AddCell(cell);

                        Phrase col7 = new Phrase("Статус", headerfont);
                        col7.Font.Size = 12;
                        cell = new PdfPCell(col7);
                        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        table.AddCell(cell);

                        Phrase col8 = new Phrase("Приоритет", headerfont);
                        col8.Font.Size = 12;
                        cell = new PdfPCell(col8);
                        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        table.AddCell(cell);

                        iTextSharp.text.Font cellFont = new iTextSharp.text.Font(baseFont, 12, iTextSharp.text.Font.NORMAL);

                        foreach (var task in tasks)
                        {
                            PdfPCell cellName = new PdfPCell(new Phrase(task.Name, cellFont));
                            PdfPCell cellProjectName = new PdfPCell(new Phrase(db.Projects.FirstOrDefault(q => q.Id == task.ProjectId).Name, cellFont));
                            PdfPCell cellDescription = new PdfPCell(new Phrase(task.Description, cellFont));
                            PdfPCell cellCreationDate = new PdfPCell(new Phrase(task.CreationDate.ToShortDateString(), cellFont));
                            PdfPCell cellDueDate = new PdfPCell(new Phrase(task.DueDate.ToShortDateString(), cellFont));
                            PdfPCell cellCountFiles = new PdfPCell(new Phrase(db.Files.Where(q => q.TaskId == task.Id).Count().ToString(), cellFont));
                            PdfPCell cellStatus = new PdfPCell(new Phrase(ContextManager.ConvertTaskStatusRevert(task.Status), cellFont));
                            PdfPCell cellPriority = new PdfPCell(new Phrase(ContextManager.ConvertTaskPriorityRevert(task.Priority), cellFont));

                            table.AddCell(cellName);
                            table.AddCell(cellProjectName);
                            table.AddCell(cellDescription);
                            table.AddCell(cellCreationDate);
                            table.AddCell(cellDueDate);
                            table.AddCell(cellCountFiles);
                            table.AddCell(cellStatus);
                            table.AddCell(cellPriority);
                        }

                        document.Add(table);
                        document.Close();

                        return new FileContentResult(stream.GetBuffer(), "application/pdf")
                        {
                            FileDownloadName = $"Отчет по всем задачам {user.Name} от {DateTime.Now.Date.ToString("D")}.pdf"
                        };
                    }
                }
            }
            return Redirect("~/Home/Reports");
        }

        public ActionResult ReportN3(int userid, string format)
        {
            var user = db.Users.FirstOrDefault(q => q.Id == userid);
            var projects = db.Projects.Where(r => r.UserId == userid && r.Status == ProjectStatus.Завершенный).ToList();
            var averageTimePerProject = projects.Select(project => (project.EndDate - project.StartDate).TotalDays).Average();

            if (projects.Count > 0)
            {
                if (format == "Excel")
                {
                    int lastrow = 0;

                    using (XLWorkbook workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Выполненные проекты");

                        worksheet.Column(1).Width = 50;
                        worksheet.Column(2).Width = 50;
                        worksheet.Column(3).Width = 30;
                        worksheet.Column(4).Width = 30;
                        worksheet.Column(5).Width = 30;

                        worksheet.Cell(1, 1).Value = "Наименование";
                        worksheet.Cell(1, 2).Value = "Описание";
                        worksheet.Cell(1, 3).Value = "Дата начала";
                        worksheet.Cell(1, 4).Value = "Дата завершения";
                        worksheet.Cell(1, 5).Value = "Количество задач";
                        worksheet.Row(1).Style.Font.Bold = true;

                        for (int i = 0; i < projects.Count; i++)
                        {
                            worksheet.Cell(i + 2, 1).Value = projects[i].Name;
                            worksheet.Cell(i + 2, 2).Value = projects[i].Description;
                            worksheet.Cell(i + 2, 3).Value = projects[i].StartDate.ToShortDateString();
                            worksheet.Cell(i + 2, 4).Value = projects[i].EndDate.ToShortDateString();
                            worksheet.Cell(i + 2, 5).Value = db.Tasks.Where(q => q.ProjectId == projects[i].Id).Count();
                            lastrow = i + 1;
                        }

                        worksheet.Cell(lastrow + 2, 4).Value = "Среднее количество дней затраченное на проект:";
                        worksheet.Cell(lastrow + 2, 5).Value = Math.Round(averageTimePerProject, 2);

                        using (var stream = new MemoryStream())
                        {
                            workbook.SaveAs(stream);
                            stream.Flush();

                            return new FileContentResult(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                            {
                                FileDownloadName = $"Отчет по выполненным проектам {user.Name} от {DateTime.Now.Date.ToString("D")}.xlsx"
                            };
                        }
                    }
                }
                else if (format == "Pdf")
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        Document document = new Document();
                        PdfWriter writer = PdfWriter.GetInstance(document, stream);

                        document.Open();

                        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                        Encoding.GetEncoding("windows-1252");

                        BaseFont baseFont = BaseFont.CreateFont(@"C:\Windows\Fonts\ARIAL.ttf", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                        iTextSharp.text.Font headerfont = new iTextSharp.text.Font(baseFont, iTextSharp.text.Font.DEFAULTSIZE, iTextSharp.text.Font.NORMAL);

                        Paragraph paragraph = new Paragraph($"Отчет по выполненным проектам от {DateTime.Now.Date.ToString("D")}", headerfont);
                        paragraph.Alignment = Element.ALIGN_CENTER;
                        paragraph.Font.Size = 16;
                        paragraph.Font.IsBold();
                        paragraph.SpacingAfter = 10f;
                        document.Add(paragraph);

                        var headerStyle = new PdfPCell();
                        headerStyle.BackgroundColor = BaseColor.WHITE;
                        headerStyle.Border = Rectangle.BOX;
                        headerStyle.HorizontalAlignment = Element.ALIGN_CENTER;

                        float[] columnWidths = new float[] { 30f, 42f, 20f, 20f, 18f };
                        PdfPTable table = new PdfPTable(columnWidths);

                        Phrase col1 = new Phrase("Наименование", headerfont);
                        col1.Font.Size = 12;
                        PdfPCell cell = new PdfPCell(col1);
                        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        table.AddCell(cell);

                        Phrase col2 = new Phrase("Описание", headerfont);
                        col2.Font.Size = 12;
                        cell = new PdfPCell(col2);
                        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        table.AddCell(cell);

                        Phrase col3 = new Phrase("Дата начала", headerfont);
                        col3.Font.Size = 12;
                        cell = new PdfPCell(col3);
                        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        table.AddCell(cell);

                        Phrase col4 = new Phrase("Дата завершения", headerfont);
                        col4.Font.Size = 12;
                        cell = new PdfPCell(col4);
                        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        table.AddCell(cell);

                        Phrase col5 = new Phrase("Количество задач", headerfont);
                        col5.Font.Size = 12;
                        cell = new PdfPCell(col5);
                        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        table.AddCell(cell);

                        iTextSharp.text.Font cellFont = new iTextSharp.text.Font(baseFont, 12, iTextSharp.text.Font.NORMAL);

                        foreach (var project in projects)
                        {
                            PdfPCell cellName = new PdfPCell(new Phrase(project.Name, cellFont));
                            PdfPCell cellDescription = new PdfPCell(new Phrase(project.Description, cellFont));
                            PdfPCell cellStartDate = new PdfPCell(new Phrase(project.StartDate.ToShortDateString(), cellFont));
                            PdfPCell cellDueDate = new PdfPCell(new Phrase(project.EndDate.ToShortDateString(), cellFont));
                            PdfPCell cellCountTasks = new PdfPCell(new Phrase(db.Tasks.Where(q => q.ProjectId == project.Id).Count().ToString(), cellFont));

                            table.AddCell(cellName);
                            table.AddCell(cellDescription);
                            table.AddCell(cellStartDate);
                            table.AddCell(cellDueDate);
                            table.AddCell(cellCountTasks);
                        }

                        document.Add(table);

                        Paragraph paragraph2 = new Paragraph($"Среднее количество дней затраченное на проект: {Math.Round(averageTimePerProject, 2)}", headerfont);
                        paragraph2.Font.Size = 14;
                        paragraph2.SpacingBefore = 10f;
                        document.Add(paragraph2);

                        document.Close();

                        return new FileContentResult(stream.GetBuffer(), "application/pdf")
                        {
                            FileDownloadName = $"Отчет по выполненным проектам {user.Name} от {DateTime.Now.Date.ToString("D")}.pdf"
                        };
                    }
                }
            }
            return Redirect("~/Home/Reports");
        }

        public ActionResult ReportN4(int userid, string format)
        {
            var user = db.Users.FirstOrDefault(q => q.Id == userid);
            var projects = db.Projects.Where(r => r.UserId == userid).ToList();

            if (projects.Count > 0)
            {
                if (format == "Excel")
                {
                    int lastrow = 0;

                    using (XLWorkbook workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Все проекты");

                        worksheet.Column(1).Width = 50;
                        worksheet.Column(2).Width = 50;
                        worksheet.Column(3).Width = 30;
                        worksheet.Column(4).Width = 30;
                        worksheet.Column(5).Width = 30;
                        worksheet.Column(6).Width = 30;

                        worksheet.Cell(1, 1).Value = "Наименование";
                        worksheet.Cell(1, 2).Value = "Описание";
                        worksheet.Cell(1, 3).Value = "Дата начала";
                        worksheet.Cell(1, 4).Value = "Дата завершения";
                        worksheet.Cell(1, 5).Value = "Количество задач";
                        worksheet.Cell(1, 6).Value = "Статус";
                        worksheet.Row(1).Style.Font.Bold = true;

                        for (int i = 0; i < projects.Count; i++)
                        {
                            worksheet.Cell(i + 2, 1).Value = projects[i].Name;
                            worksheet.Cell(i + 2, 2).Value = projects[i].Description;
                            worksheet.Cell(i + 2, 3).Value = projects[i].StartDate.ToShortDateString();
                            worksheet.Cell(i + 2, 4).Value = projects[i].EndDate.ToShortDateString();
                            worksheet.Cell(i + 2, 5).Value = db.Tasks.Where(q => q.ProjectId == projects[i].Id).Count();
                            worksheet.Cell(i + 2, 6).Value = ContextManager.ConvertProjectStatusRevert(projects[i].Status);
                            lastrow = i + 1;
                        }

                        using (var stream = new MemoryStream())
                        {
                            workbook.SaveAs(stream);
                            stream.Flush();

                            return new FileContentResult(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                            {
                                FileDownloadName = $"Отчет по всем проектам {user.Name} от {DateTime.Now.Date.ToString("D")}.xlsx"
                            };
                        }
                    }
                }
                else if (format == "Pdf")
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        Document document = new Document();
                        PdfWriter writer = PdfWriter.GetInstance(document, stream);

                        document.Open();

                        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                        Encoding.GetEncoding("windows-1252");

                        BaseFont baseFont = BaseFont.CreateFont(@"C:\Windows\Fonts\ARIAL.ttf", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                        iTextSharp.text.Font headerfont = new iTextSharp.text.Font(baseFont, iTextSharp.text.Font.DEFAULTSIZE, iTextSharp.text.Font.NORMAL);

                        Paragraph paragraph = new Paragraph($"Отчет по всем проектам от {DateTime.Now.Date.ToString("D")}", headerfont);
                        paragraph.Alignment = Element.ALIGN_CENTER;
                        paragraph.Font.Size = 16;
                        paragraph.Font.IsBold();
                        paragraph.SpacingAfter = 10f;
                        document.Add(paragraph);

                        var headerStyle = new PdfPCell();
                        headerStyle.BackgroundColor = BaseColor.WHITE;
                        headerStyle.Border = Rectangle.BOX;
                        headerStyle.HorizontalAlignment = Element.ALIGN_CENTER;

                        float[] columnWidths = new float[] { 30f, 42f, 20f, 20f, 18f, 20f };
                        PdfPTable table = new PdfPTable(columnWidths);

                        Phrase col1 = new Phrase("Наименование", headerfont);
                        col1.Font.Size = 12;
                        PdfPCell cell = new PdfPCell(col1);
                        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        table.AddCell(cell);

                        Phrase col2 = new Phrase("Описание", headerfont);
                        col2.Font.Size = 12;
                        cell = new PdfPCell(col2);
                        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        table.AddCell(cell);

                        Phrase col3 = new Phrase("Дата начала", headerfont);
                        col3.Font.Size = 12;
                        cell = new PdfPCell(col3);
                        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        table.AddCell(cell);

                        Phrase col4 = new Phrase("Дата завершения", headerfont);
                        col4.Font.Size = 12;
                        cell = new PdfPCell(col4);
                        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        table.AddCell(cell);

                        Phrase col5 = new Phrase("Количество задач", headerfont);
                        col5.Font.Size = 12;
                        cell = new PdfPCell(col5);
                        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        table.AddCell(cell);

                        Phrase col6 = new Phrase("Статус", headerfont);
                        col6.Font.Size = 12;
                        cell = new PdfPCell(col6);
                        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        table.AddCell(cell);

                        iTextSharp.text.Font cellFont = new iTextSharp.text.Font(baseFont, 12, iTextSharp.text.Font.NORMAL);

                        foreach (var project in projects)
                        {
                            PdfPCell cellName = new PdfPCell(new Phrase(project.Name, cellFont));
                            PdfPCell cellDescription = new PdfPCell(new Phrase(project.Description, cellFont));
                            PdfPCell cellStartDate = new PdfPCell(new Phrase(project.StartDate.ToShortDateString(), cellFont));
                            PdfPCell cellDueDate = new PdfPCell(new Phrase(project.EndDate.ToShortDateString(), cellFont));
                            PdfPCell cellCountTasks = new PdfPCell(new Phrase(db.Tasks.Where(q => q.ProjectId == project.Id).Count().ToString(), cellFont));
                            PdfPCell cellStatus = new PdfPCell(new Phrase(ContextManager.ConvertProjectStatusRevert(project.Status), cellFont));

                            table.AddCell(cellName);
                            table.AddCell(cellDescription);
                            table.AddCell(cellStartDate);
                            table.AddCell(cellDueDate);
                            table.AddCell(cellCountTasks);
                            table.AddCell(cellStatus);
                        }

                        document.Add(table);
                        document.Close();

                        return new FileContentResult(stream.GetBuffer(), "application/pdf")
                        {
                            FileDownloadName = $"Отчет по всем проектам {user.Name} от {DateTime.Now.Date.ToString("D")}.pdf"
                        };
                    }
                }
            }
            return Redirect("~/Home/Reports");
        }

        public ActionResult ReportN5(int projid)
        {
            var project = db.Projects.FirstOrDefault(q => q.Id == projid);
            if (project == null)
                return Redirect("~/Home/Reports");

            using (MemoryStream stream = new MemoryStream())
            {
                Document document = new Document();
                PdfWriter writer = PdfWriter.GetInstance(document, stream);

                document.Open();

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Encoding.GetEncoding("windows-1252");

                BaseFont baseFont = BaseFont.CreateFont(@"C:\Windows\Fonts\ARIAL.ttf", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                iTextSharp.text.Font headerfont = new iTextSharp.text.Font(baseFont, iTextSharp.text.Font.DEFAULTSIZE, iTextSharp.text.Font.NORMAL);

                Paragraph paragraph = new Paragraph($"Отчет по проекту {project.Name}", headerfont);
                paragraph.Alignment = Element.ALIGN_CENTER;
                paragraph.Font.Size = 16;
                paragraph.Font.IsBold();
                paragraph.SpacingAfter = 10f;
                document.Add(paragraph);

                Paragraph pname = new Paragraph($"Наименование: {project.Name}", headerfont);
                pname.Alignment = Element.ALIGN_LEFT;
                pname.Font.Size = 12;
                pname.Font.IsStandardFont();
                pname.SpacingAfter = 6f;
                document.Add(pname);

                Paragraph pdesc = new Paragraph($"Описание: {project.Description}", headerfont);
                pdesc.Alignment = Element.ALIGN_LEFT;
                pdesc.Font.Size = 12;
                pdesc.Font.IsStandardFont();
                pdesc.SpacingAfter = 6f;
                document.Add(pdesc);

                Paragraph pstart = new Paragraph($"Дата создания: {project.StartDate.ToLongDateString()}", headerfont);
                pstart.Alignment = Element.ALIGN_LEFT;
                pstart.Font.Size = 12;
                pstart.Font.IsStandardFont();
                pstart.SpacingAfter = 6f;
                document.Add(pstart);

                Paragraph pend = new Paragraph($"Дата окончания: {project.EndDate.ToLongDateString()}", headerfont);
                pend.Alignment = Element.ALIGN_LEFT;
                pend.Font.Size = 12;
                pend.Font.IsStandardFont();
                pend.SpacingAfter = 6f;
                document.Add(pend);

                Paragraph ptasks = new Paragraph($"Задачи:", headerfont);
                ptasks.Alignment = Element.ALIGN_LEFT;
                ptasks.Font.Size = 12;
                ptasks.Font.IsStandardFont();
                ptasks.SpacingAfter = 0f;
                document.Add(ptasks);

                var headerStyle = new PdfPCell();
                headerStyle.BackgroundColor = BaseColor.WHITE;
                headerStyle.Border = Rectangle.BOX;
                headerStyle.HorizontalAlignment = Element.ALIGN_CENTER;

                float[] columnWidths = new float[] { 30f, 42f, 20f, 20f, 18f, 20f };
                PdfPTable table = new PdfPTable(columnWidths);
                table.SpacingAfter = 6f;

                Phrase col1 = new Phrase("Наименование", headerfont);
                col1.Font.Size = 12;
                col1.Font.IsBold();
                PdfPCell cell = new PdfPCell(col1);
                cell.BackgroundColor = BaseColor.WHITE;
                table.AddCell(cell);

                Phrase col2 = new Phrase("Описание", headerfont);
                col2.Font.Size = 12;
                col2.Font.IsBold();
                cell = new PdfPCell(col2);
                cell.BackgroundColor = BaseColor.WHITE;
                table.AddCell(cell);

                Phrase col3 = new Phrase("Дата начала", headerfont);
                col3.Font.Size = 12;
                col3.Font.IsBold();
                cell = new PdfPCell(col3);
                cell.BackgroundColor = BaseColor.WHITE;
                table.AddCell(cell);

                Phrase col4 = new Phrase("Дата завершения", headerfont);
                col4.Font.Size = 12;
                col4.Font.IsBold();
                cell = new PdfPCell(col4);
                cell.BackgroundColor = BaseColor.WHITE;
                table.AddCell(cell);

                Phrase col5 = new Phrase("Количество файлов", headerfont);
                col5.Font.Size = 12;
                col5.Font.IsBold();
                cell = new PdfPCell(col5);
                cell.BackgroundColor = BaseColor.WHITE;
                table.AddCell(cell);

                Phrase col6 = new Phrase("Приоритет", headerfont);
                col6.Font.Size = 12;
                col6.Font.IsBold();
                cell = new PdfPCell(col6);
                cell.BackgroundColor = BaseColor.WHITE;
                table.AddCell(cell);

                iTextSharp.text.Font cellFont = new iTextSharp.text.Font(baseFont, 12, iTextSharp.text.Font.NORMAL);
                var tasks = db.Tasks.Where(q => q.ProjectId == project.Id).ToList();
                foreach (var task in tasks)
                {
                    PdfPCell cellName = new PdfPCell(new Phrase(task.Name, cellFont));
                    PdfPCell cellDescription = new PdfPCell(new Phrase(task.Description, cellFont));
                    PdfPCell cellCreationDate = new PdfPCell(new Phrase(task.CreationDate.ToShortDateString(), cellFont));
                    PdfPCell cellDueDate = new PdfPCell(new Phrase(task.DueDate.ToShortDateString(), cellFont));
                    PdfPCell cellCountFiles = new PdfPCell(new Phrase(db.Files.Where(q => q.TaskId == task.Id).Count().ToString(), cellFont));
                    PdfPCell cellPriority = new PdfPCell(new Phrase(ContextManager.ConvertTaskPriorityRevert(task.Priority), cellFont));

                    table.AddCell(cellName);
                    table.AddCell(cellDescription);
                    table.AddCell(cellCreationDate);
                    table.AddCell(cellDueDate);
                    table.AddCell(cellCountFiles);
                    table.AddCell(cellPriority);
                }

                document.Add(table);

                Paragraph prologs = new Paragraph("ProLogs", headerfont);
                prologs.Alignment = Element.ALIGN_RIGHT;
                prologs.Font.Size = 12;
                prologs.Font.IsStandardFont();
                document.Add(prologs);
                document.Close();

                return new FileContentResult(stream.GetBuffer(), "application/pdf")
                {
                    FileDownloadName = $"Отчет по проекту {project.Name} от {DateTime.Now.Date.ToString("D")}.pdf"
                };
            }
        }

        public ActionResult Portfolio(int userid)
        {
            var tasks = db.Tasks.Where(q => q.UserId == userid && q.Status == Models.TaskStatus.Завершенный).ToList();
            var projects = db.Projects.Where(q => q.UserId == userid && q.Status == Models.ProjectStatus.Завершенный).ToList();

            var user = db.Users.FirstOrDefault(q => q.Id == userid);
            
            List<string> allSkills = new List<string>();

            foreach (var task in tasks)
            {
                List<string> skillsInTask = ContextManager.SearchSkill(task.Description);
                allSkills.AddRange(skillsInTask);
            }

            List<string> uniqueSkills = allSkills.Distinct().ToList();

            string strskills = string.Empty;
            foreach (var skill in uniqueSkills)
            {
                strskills += $" {skill},";
            }
            strskills = strskills.TrimEnd(',');

            string strproj = string.Empty;
            foreach (var proj in projects)
            {
                strproj += $"\n{proj.Name} — {proj.Description}";
            }

            string strtasks = string.Empty;
            foreach (var task in tasks)
            {
                strtasks += $"\n{task.Name} — {task.Description}";
            }

            using (MemoryStream stream = new MemoryStream())
            {
                Document document = new Document();
                PdfWriter writer = PdfWriter.GetInstance(document, stream);

                document.Open();
                document.Add(new Chunk(""));

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Encoding.GetEncoding("windows-1252");

                BaseFont baseFont = BaseFont.CreateFont(@"C:\Windows\Fonts\ARIAL.ttf", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                iTextSharp.text.Font headerfont = new iTextSharp.text.Font(baseFont, iTextSharp.text.Font.DEFAULTSIZE, iTextSharp.text.Font.NORMAL);

                Paragraph paragraph = new Paragraph("Портфолио", headerfont);
                paragraph.Alignment = Element.ALIGN_CENTER;
                paragraph.Font.Size = 16;
                paragraph.Font.IsBold();
                paragraph.SpacingAfter = 10f;
                document.Add(paragraph);

                Paragraph pname = new Paragraph($"Меня зовут {db.Users.FirstOrDefault(q => q.Id == userid).Name}, мои ключевые навыки:{strskills}", headerfont);
                pname.Alignment = Element.ALIGN_LEFT;
                pname.Font.Size = 12;
                pname.SpacingAfter = 0f;
                document.Add(pname);

                Paragraph pcontact = new Paragraph($"E-mail для связи {db.Users.FirstOrDefault(q => q.Id == userid).Email}", headerfont);
                pcontact.Alignment = Element.ALIGN_LEFT;
                pcontact.Font.Size = 12;
                pcontact.SpacingAfter = 10f;
                document.Add(pcontact);

                Paragraph pproj = new Paragraph($"Проекты: {strproj}", headerfont);
                pproj.Alignment = Element.ALIGN_LEFT;
                pproj.Font.Size = 12;
                pproj.SpacingAfter = 10f;
                document.Add(pproj);

                Paragraph ptasks = new Paragraph($"Задачи: {strtasks}", headerfont);
                ptasks.Alignment = Element.ALIGN_LEFT;
                ptasks.Font.Size = 12;
                ptasks.SpacingAfter = 10f;
                document.Add(ptasks);

                Paragraph prologs = new Paragraph("ProLogs", headerfont);
                prologs.Alignment = Element.ALIGN_RIGHT;
                prologs.Font.Size = 12;
                prologs.Font.IsStandardFont();
                document.Add(prologs);

                document.Close();

                return new FileContentResult(stream.GetBuffer(), "application/pdf")
                {
                    FileDownloadName = $"Портфолио {user.Name} от {DateTime.Now.Date.ToString("D")}.pdf"
                };
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

            
    }
}
