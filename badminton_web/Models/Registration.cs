using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MemberSystemMVC.Models
{
    [Table("registration")]
    public class Registration
    {
        [Key]
        public int Id { get; set; }

        [Column("member_account"), Required, MaxLength(255)]
        public string MemberAccount { get; set; }

        [Column("venue_date")]
        public DateTime VenueDate { get; set; }

        [Column("venue_id")]
        public int VenueId { get; set; }

        [Column("time_slot"), MaxLength(50)]
        public string TimeSlot { get; set; }

        [Column("registration_date")]
        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        [Column("paid")]
        public bool Paid { get; set; } = false;

        [Column("payment_date")]
        public DateTime? PaymentDate { get; set; }

        // 導航屬性
        [ForeignKey("VenueId")]
        public Venue VenueInfo { get; set; }
    }
}





