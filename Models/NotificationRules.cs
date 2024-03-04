using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace freelance.Models
{
    public class NotificationRules
    {
        public int Id { get; set; }

        [Required]
        public string NotiFrom { get; set; }

        [Required]
        public string NotiName { get; set; }

        [Required]
        public bool isWork { get; set; }

        // Внешний ключ для пользователя, который управляет задачами
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
