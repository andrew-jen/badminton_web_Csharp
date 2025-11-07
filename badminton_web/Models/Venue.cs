using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MemberSystemMVC.Models
{
    [Table("venue")] //資料庫表名
    public class Venue
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(255)]
        public string Name { get; set; }

        [Required]
        public int Capacity { get; set; }

        [Required, MaxLength(255)]
        public string Address { get; set; }

        [Required]
        public decimal Fee { get; set; }

        // 導航屬性：一個場地可以有多個時段
        public ICollection<VenueSchedule> VenueSchedules { get; set; }
    }
}



