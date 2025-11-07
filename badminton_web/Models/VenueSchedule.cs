using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MemberSystemMVC.Models
{
    [Table("venue_schedule")]  //資料庫表名
    public class VenueSchedule
    {
        [Key]
        public int Id { get; set; }

        [Column("month"), MaxLength(20)]
        public string Month { get; set; }  // 例如 "2025-10"

        [Column("venue_id")]
        public int VenueId { get; set; }

        [Column("schedule_date")]
        public DateTime ScheduleDate { get; set; }

        [Column("time_slot"), MaxLength(50)]
        public string TimeSlot { get; set; }

        [Column("fee")]
        public decimal Fee { get; set; }

        [Column("capacity")]
        public int Capacity { get; set; }

        [Column("registered_count")]
        public int RegisteredCount { get; set; }

        [Column("remaining_slots")]
        public int RemainingSlots { get; set; }

        // 導航屬性
        [ForeignKey("VenueId")]
        public Venue VenueInfo { get; set; }
    }
}


