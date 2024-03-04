using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;

namespace freelance.Models
{
    public class Task
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public string Description { get; set; }

        public TaskStatus Status { get; set; }

        [Required]
        public DateTime CreationDate { get; set; }

        public DateTime DueDate { get; set; }

        public TaskPriority Priority { get; set; }

        // Внешний ключ для пользователя, который выполняет задачу
        public int UserId { get; set; }
        public User User { get; set; }

        // Внешний ключ для проекта, к которому относится задача
        public int ProjectId { get; set; }
        public Project Project { get; set; }

        public ICollection<File> Files { get; set; } // Связь с задачами
    }

    public enum TaskStatus
    {
        Новый,
        Впроцессе,
        Завершенный
    }

    public enum TaskPriority
    {
        Низкий,
        Средний,
        Высокий
    }
}
