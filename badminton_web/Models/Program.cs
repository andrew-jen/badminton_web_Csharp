using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MemberSystemMVC.Models
{
    [Table("program")]
    public class Program
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("venue_name"), MaxLength(100)]
        public string VenueName { get; set; }

        [Column("schedule_date")]
        public DateTime ScheduleDate { get; set; }

        [Column("time_slot"), MaxLength(50)]
        public string TimeSlot { get; set; }

        [Column("coach_name"), MaxLength(100)]
        public string CoachName { get; set; }

        [Column("fee")]
        public decimal Fee { get; set; }

        [Column("capacity")]
        public int Capacity { get; set; }

        [Column("address"), MaxLength(200)]
        public string Address { get; set; }

        [Column("registered_count")]
        public int RegisteredCount { get; set; }

        [Column("remaining_slots")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int RemainingSlots { get; private set; } // EF Core 會自動抓取值，但不寫入


        [Column("recommendation_level"), MaxLength(50)]
        public string RecommendationLevel { get; set; }

        [Column("coach_phone"), MaxLength(20)]
        public string CoachPhone { get; set; }
    }
}



