using MemberSystemMVC.Models;        // 引入專案的模型，例如 Test1Context、VenueSchedule、Registration
using Microsoft.AspNetCore.Mvc;      // MVC 控制器與視圖支援
using Microsoft.EntityFrameworkCore; // EF Core，提供資料庫操作功能

namespace MemberSystemMVC.Controllers
{
    // 場地控制器，負責場地查詢與報名功能
    public class ProgramController : Controller
    {
        private readonly Test1Context _context; // EF Core DbContext，用來操作資料庫

        // 建構子，透過依賴注入 (DI) 傳入 DbContext
        public ProgramController(Test1Context context)
        {
            _context = context;
        }

        // 顯示場地預約表單
        public async Task<IActionResult> Coach()
        {
            // 取得今天日期與下個月日期
            var today = DateTime.Today;
            var nextMonth = today.AddMonths(1);

            // 從 program 表中抓取當前月份及下個月份的課程資料
            var programs = await _context.Program
                .Where(p => p.ScheduleDate.Month == today.Month || p.ScheduleDate.Month == nextMonth.Month)
                .OrderBy(p => p.ScheduleDate)  // 依日期排序
                .ThenBy(p => p.TimeSlot)       // 同日期依時段排序
                .ToListAsync();                // 轉成 List

            // 從 Session 取得登入會員帳號
            string memberAccount = HttpContext.Session.GetString("MemberAccount");

            // 查詢該會員的報名紀錄
            var registrations = await _context.ViewProgramList
                .Where(r => r.學員帳號 == memberAccount)
                .ToListAsync();

            // 傳當前月份給 View
            ViewBag.CurrentMonth = today.ToString("yyyy-MM");

            // 先傳給 View
            var model = new ProgramPageViewModel
            {
                Courses = programs,
                Registrations = registrations
            };

            return View(model);
        }

        // 處理報名 POST 請求
        [HttpPost]
        public async Task<IActionResult> ReserveClass(int programId)
        {
            // 從 Session 取得會員帳號
            string account = HttpContext.Session.GetString("MemberAccount");

            var member = await _context.members
                .FirstOrDefaultAsync(m => m.Account == account);


            // 如果會員尚未登入，導向登入頁
            if (string.IsNullOrEmpty(account))
            {
                return RedirectToAction("Login", "Member");
            }

            // 查詢會員資料
            var themember = await _context.members.FirstOrDefaultAsync(m => m.Account == account);
            if (themember == null)
            {
                TempData["Error"] = "找不到會員資料。";
                return RedirectToAction("Coach");
            }

            // 查詢課程資料 (Program)
            var program = await _context.Program
                .FirstOrDefaultAsync(p => p.Id == programId);

            // ============================
            // 建立報名紀錄
            var registration = new MembersProgramList
            {
                MemberId = member.Id,
                VenueName = program.VenueName,
                ScheduleDate = program.ScheduleDate,
                TimeSlot = program.TimeSlot,
                CoachName = program.CoachName,
                RegisterTime = DateTime.Now
            };

            // 將報名加入 DbContext
            _context.MembersProgramList.Add(registration);

            // ============================
            // 2️⃣ 更新場地時段資訊
            // ============================
            program.RegisteredCount += 1;       // 已報名人數 +1

            // 儲存所有變更到資料庫
            await _context.SaveChangesAsync();

            // 設定成功訊息
            TempData["Success"] = "報名1位成功！";

            // 重新導向回報名頁
            return RedirectToAction("Coach");
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int registrationId)
        {
            // 從 Session 取得會員帳號
            string account = HttpContext.Session.GetString("MemberAccount");

            if (string.IsNullOrEmpty(account))
                return RedirectToAction("Login", "Member");

            // 查詢會員資料
            var member = await _context.members.FirstOrDefaultAsync(m => m.Account == account);
            if (member == null)
            {
                TempData["Error"] = "找不到會員資料。";
                return RedirectToAction("Coach");
            }

            // 查詢要刪除的報名紀錄
            var registration = await _context.MembersProgramList
                .FirstOrDefaultAsync(r => r.Id == registrationId && r.MemberId == member.Id);

            if (registration != null)
            {
                // 更新課程人數
                var program = await _context.Program
                    .FirstOrDefaultAsync(p => p.VenueName == registration.VenueName
                                           && p.ScheduleDate == registration.ScheduleDate
                                           && p.TimeSlot == registration.TimeSlot);

                if (program != null)
                {
                    program.RegisteredCount -= 1;                }

                // 刪除報名紀錄
                _context.MembersProgramList.Remove(registration);

                // 儲存所有變更
                await _context.SaveChangesAsync();

                TempData["Success"] = "取消報名成功！";
            }
            else
            {
                TempData["Error"] = "找不到報名紀錄。";
            }

            return RedirectToAction("Coach");
        }
    }
}



