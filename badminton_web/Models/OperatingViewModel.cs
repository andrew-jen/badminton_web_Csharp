namespace MemberSystemMVC.Models
{
    public class OperatingViewModel
    {
        // 對應表單輸入欄位 (Program)
        public string VenueName { get; set; }
        public DateTime ScheduleDate { get; set; }
        public string TimeSlot { get; set; }
        public decimal Fee { get; set; }
        public int Capacity { get; set; }
        public string RecommendationLevel { get; set; }
        public string CoachPhone { get; set; }

        // 額外資料：場地選單
        public List<string> Venues { get; set; }
    }


}
