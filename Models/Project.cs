using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;

namespace freelance.Models
{
    public class Project
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public ProjectStatus Status { get; set; }

        // Внешний ключ для пользователя, который создал проект
        public int UserId { get; set; }
        public User User { get; set; }

        public ICollection<Task> Tasks { get; set; } // Связь с задачами
    }

    public enum ProjectStatus
    {
        Запланированный,
        Впроцессе,
        Завершенный
    }
}
