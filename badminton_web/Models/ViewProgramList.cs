using Microsoft.AspNetCore.Mvc;

namespace MemberSystemMVC.Models
{
    public class ViewProgramList
    {
        public int Id { get; set; }
        public string 學員姓名 { get; set; }
        public string 學員性別 { get; set; }
        public int 學員年紀 { get; set; }
        public int 學員球齡 { get; set; }
        public string 學員帳號 { get; set; }
        public string 場地名稱 { get; set; }
        public DateTime 日期 { get; set; }
        public string 時段 { get; set; }
        public string 教練名稱 { get; set; }
        public DateTime 報名時間 { get; set; }
        public int UserRegisteredCount { get; set; }
    }

}
