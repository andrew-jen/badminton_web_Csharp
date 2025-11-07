using Microsoft.AspNetCore.Mvc;      // MVC 控制器與視圖支援
using Microsoft.EntityFrameworkCore; // EF Core，提供資料庫操作功能
using MemberSystemMVC.Models;        // 引入專案的模型，例如 Test1Context、VenueSchedule、Registration

namespace MemberSystemMVC.Controllers
{
    // 場地控制器，負責場地查詢與報名功能
    public class VenueController : Controller
    {
        private readonly Test1Context _context; // EF Core DbContext，用來操作資料庫

        // 建構子，透過依賴注入 (DI) 傳入 DbContext
        public VenueController(Test1Context context)
        {
            _context = context;
        }

        // 顯示場地預約表單
        public async Task<IActionResult> Reserve() //Reserve是前端頁面的名稱-Reserve.cshtml(在和Controller同名的Venue資料夾底下)
        {
            // 取得今天日期與下個月日期
            var today = DateTime.Today;
            var nextMonth = today.AddMonths(1);

            // 取得 4 週後的日期
            var fourWeeksLater = today.AddDays(60);

            // 從 VenueSchedules 表抓取今天到 8 週後的資料
            var schedules = await _context.VenueSchedules
                .Include(vs => vs.VenueInfo) // 載入關聯的 Venue
                .Where(vs => vs.ScheduleDate >= today && vs.ScheduleDate <= fourWeeksLater) // 篩選區間
                .OrderBy(vs => vs.ScheduleDate) // 依日期排序
                .ThenBy(vs => vs.TimeSlot)      // 同日期依時段排序
                .ToListAsync();

            // 傳當前月份給 View
            ViewBag.CurrentMonth = today.ToString("yyyy-MM");

            // 將 programs 資料傳給 View
            return View(schedules);
        }

        // 處理報名 POST 請求
        [HttpPost]
        public async Task<IActionResult> Register(int scheduleId) //Register是前端action的名稱，scheduleId是從前端傳來的參數(變數)
        {
            // 從 Session 取得會員帳號
            string account = HttpContext.Session.GetString("MemberAccount");

            // 如果會員尚未登入，導向登入頁
            if (string.IsNullOrEmpty(account))
            {
                return RedirectToAction("Login", "Member");
            }

            // 找出對應的場地時段資料
            var schedule = await _context.VenueSchedules.FindAsync(scheduleId);

            // 如果場次不存在或已額滿，顯示錯誤訊息並返回預約頁
            if (schedule == null || schedule.RemainingSlots <= 0)
            {
                TempData["Error"] = "該場次已額滿或不存在";
                return RedirectToAction("Reserve");
            }

            // ============================
            // 1️⃣ 新增報名資料
            // ============================
            var registration = new Registration
            {
                MemberAccount = account,           // 設定會員帳號
                VenueDate = schedule.ScheduleDate, // 場地日期
                VenueId = schedule.VenueId,        // 場地 ID
                TimeSlot = schedule.TimeSlot,      // 時段
                Paid = false                       // 尚未付款
            };

            // 將報名加入 DbContext
            _context.Registrations.Add(registration);

            // ============================
            // 2️⃣ 更新場地時段資訊
            // ============================
            schedule.RegisteredCount += 1;       // 已報名人數 +1
            schedule.RemainingSlots -= 1;        // 剩餘名額 -1

            // 儲存所有變更到資料庫
            await _context.SaveChangesAsync();

            // 設定成功訊息
            TempData["Success"] = "報名成功！";

            // 重新導向回預約頁
            return RedirectToAction("Reserve");
        }
    }
}



