using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace freelance.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public DateTime InvoiceDate { get; set; }

        [Required]
        public bool isRemoved { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
    }
}
