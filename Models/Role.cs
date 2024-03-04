using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace freelance.Models
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<User> Users { get; set; } // Связь с юзерами
    }
}
