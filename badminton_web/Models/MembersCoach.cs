using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MemberSystemMVC.Models
{
    [Table("members_coach")]
    public class MembersCoach
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("Name")]
        public string Name { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("Sex")]
        public string Sex { get; set; }

        [Required]
        [Column("Age")]
        public int Age { get; set; }

        [Required]
        [Column("During_player")]
        public int DuringPlayer { get; set; }

        [Required]
        [MaxLength(255)]
        [EmailAddress]
        [Column("Email")]
        public string Email { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("Account")]
        public string Account { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("Password")]
        public string Password { get; set; }

        [MaxLength(50)]
        [Column("Phone")]
        public string Phone { get; set; }

        [Column("CreateTime")]
        public DateTime CreateTime { get; set; } = DateTime.Now;
    }
}

