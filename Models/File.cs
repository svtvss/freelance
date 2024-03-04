using System.ComponentModel.DataAnnotations;

namespace freelance.Models
{
    public class File
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string FileName { get; set; }

        [Required]
        [MaxLength(100)]
        public string FileType { get; set; }

        [Required]
        public string FilePath { get; set; }

        // Внешний ключ для связанной задачи
        public int TaskId { get; set; }
        public Task Task { get; set; }
    }
}
