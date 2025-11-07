using System;
using System.ComponentModel.DataAnnotations;

namespace MemberSystemMVC.Models
{
    public class Member
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(100)]
        public string Sex { get; set; }

        [Required]
        public int Age { get; set; }

        [Required]
        public int During_player { get; set; }

        [Required]
        [MaxLength(255)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MaxLength(255)]
        public string Account { get; set; }

        [Required]
        [MaxLength(255)]
        public string Password { get; set; }

        public DateTime CreateTime { get; set; } = DateTime.Now;
    }
}

