using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MemberSystemMVC.Models
{
    [Table("members_programlist")]
    public class MembersProgramList
    {
        [Key]
        public int Id { get; set; }

        [Column("member_id")]  // 指定對應資料庫欄位名稱
        public int MemberId { get; set; }

        [Required, Column("場地名稱")]
        public string VenueName { get; set; }

        [Required, Column("日期")]
        public DateTime ScheduleDate { get; set; }

        [Required, Column("時段")]
        public string TimeSlot { get; set; }

        [Required, Column("教練名稱")]
        public string CoachName { get; set; }

        [Column("報名時間")]
        public DateTime RegisterTime { get; set; } = DateTime.Now;

        // 關聯到會員
        [ForeignKey("MemberId")]
        public Member Member { get; set; }
    }
}

