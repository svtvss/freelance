using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace freelance.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public int RoleId { get; set; }
        public Role Role { get; set; }

        [Required]
        public bool isRemoved { get; set; }

        public ICollection<Task> Tasks { get; set; } // Связь с задачами
        public ICollection<Project> Projects { get; set; } // Связь с проектами
    }
}
