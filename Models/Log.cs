using System.ComponentModel.DataAnnotations;
using System;

namespace freelance.Models
{
    public class Log
    {
        public int Id { get; set; }

        [Required]
        public string Action { get; set; } // Создание, Изменение, Удаление и т. д.

        [Required]
        public DateTime Timestamp { get; set; }

        // Внешний ключ для пользователя, совершившего действие
        public int UserId { get; set; }
        public User User { get; set; }

        public string Description { get; set; }
    }
}
