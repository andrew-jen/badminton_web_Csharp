// ==========================================
// 🏠 HomeController 教學筆記版
// 功能：控制首頁、個人預約查詢、預約刪除（取消）
// MVC 層級：Controller 層
// 對應 View：Views/Home/Index.cshtml、Personal_reserve.cshtml
// 使用 Model：Registration、VenueInfo、VenueSchedule、ErrorViewModel
// ==========================================


// -----------------------------
// 📦 引用必要命名空間 (using)
// -----------------------------

using badminton_web.Models;                // 模型：可能包含場地 (Venue) 或時段資料 (Schedule)
using MemberSystemMVC.Models;              // 模型：包含 Member、Registration、VenueInfo 等資料表對應
using Microsoft.AspNetCore.Mvc;            // ASP.NET Core MVC 核心功能 (Controller / View)
using Microsoft.AspNetCore.Http;           // Session 功能 (儲存登入帳號)
using Microsoft.EntityFrameworkCore;       // EF Core 資料庫操作 (CRUD、LINQ)
using Microsoft.Extensions.Logging;        // 系統日誌工具 (用於記錄錯誤、事件)
using System.Diagnostics;                  // Activity 追蹤錯誤用


// -----------------------------
// 🧭 命名空間 (Namespace)
// -----------------------------
namespace MemberSystemMVC.Controllers
{
    // ------------------------------------------------------
    // 🧩 HomeController
    // 功能：處理首頁與會員個人預約相關操作
    // ------------------------------------------------------
    public class HomeController : Controller
    {
        // 🧾 記錄 Log 用的服務，例如：
        // _logger.LogInformation("頁面載入成功");
        private readonly ILogger<HomeController> _logger;

        // 💾 EF Core 的 DbContext，用於存取資料庫
        private readonly Test1Context _context;

        // ------------------------------------------------------
        // 🔧 建構子 (Constructor)
        // 功能：注入資料庫與日誌服務
        // 透過「依賴注入 (Dependency Injection)」傳入
        // ------------------------------------------------------
        public HomeController(Test1Context context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ------------------------------------------------------
        // 🏠 Index()
        // 功能：顯示首頁畫面
        // 對應 View：Views/Home/Index.cshtml
        // ------------------------------------------------------
        public IActionResult Index()
        {
            return View();
        }

        // ------------------------------------------------------
        // 🔒 Privacy()
        // 功能：顯示隱私政策頁面
        // 對應 View：Views/Home/Privacy.cshtml
        // ------------------------------------------------------
        public IActionResult Privacy()
        {
            return View();
        }

        // ------------------------------------------------------
        // ⚠️ Error()
        // 功能：顯示錯誤頁面（當程式發生例外狀況時）
        // 對應 View：Views/Shared/Error.cshtml
        // 使用模型：ErrorViewModel (顯示 RequestId)
        // ------------------------------------------------------
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        // ------------------------------------------------------
        // 👤 Personal_reserve()
        // 功能：顯示「會員個人預約清單」
        // 對應 View：Views/Home/Personal_reserve.cshtml
        // 執行流程：
        // 1️⃣ 讀取 Session["MemberAccount"]，確認登入
        // 2️⃣ 若未登入 → 導向 Member/Login
        // 3️⃣ 若登入 → 查詢該會員所有預約資料
        // 4️⃣ 回傳結果給 View 顯示
        // ------------------------------------------------------
        public async Task<IActionResult> Personal_reserve()
        {
            // 從 Session 取得目前登入的會員帳號
            string? memberAccount = HttpContext.Session.GetString("MemberAccount");

            // 若 Session 為空，代表未登入，導向登入頁面
            if (string.IsNullOrEmpty(memberAccount))
            {
                return RedirectToAction("Login", "Member");
            }

            // 🔍 查詢該會員的預約紀錄
            // Include()：載入關聯表 VenueInfo（否則會出現 null）
            var registrations = await _context.Registrations
                .Include(r => r.VenueInfo)  // 載入場地資料
                .Where(r => r.MemberAccount == memberAccount)
                .OrderBy(r => r.VenueDate)  // 依日期排序
                .ThenBy(r => r.TimeSlot)    // 再依時段排序
                .ToListAsync();

            // 將查詢結果傳給 View 顯示
            return View(registrations);
        }

        // ------------------------------------------------------
        // 🗑️ DeleteReservation()
        // 功能：刪除（取消）指定預約
        // 對應前端表單：form method="post" action="/Home/DeleteReservation"
        // 對應 View：Personal_reserve.cshtml 的刪除按鈕
        // 執行流程：
        // 1️⃣ 根據 id 找出預約紀錄
        // 2️⃣ 若無 → 顯示錯誤訊息
        // 3️⃣ 若有 → 刪除紀錄並更新場地時段名額
        // 4️⃣ 顯示成功提示並返回預約清單
        // ------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            // 1️⃣ 依 id 找出預約紀錄
            var reservation = await _context.Registrations.FindAsync(id);

            if (reservation == null)
            {
                TempData["Error"] = "找不到要刪除的資料。";
                return RedirectToAction("Personal_reserve");
            }

            // 2️⃣ 找出該預約所屬的場地時段（為了釋出名額）
            var schedule = await _context.VenueSchedules
                .FirstOrDefaultAsync(vs =>
                    vs.VenueId == reservation.VenueId &&
                    vs.ScheduleDate == reservation.VenueDate &&
                    vs.TimeSlot == reservation.TimeSlot);

            // 3️⃣ 刪除預約紀錄
            _context.Registrations.Remove(reservation);

            // 4️⃣ 若場地時段存在，更新名額資訊
            if (schedule != null)
            {
                schedule.RegisteredCount -= 1; // 已登記人數 -1
                schedule.RemainingSlots += 1;  // 可用名額 +1
            }

            // 5️⃣ 儲存變更
            await _context.SaveChangesAsync();

            // 6️⃣ 顯示提示訊息（TempData：僅存在一次請求）
            TempData["Success"] = "已成功取消預約，場地名額已釋出。";

            // 7️⃣ 返回個人預約頁面
            return RedirectToAction("Personal_reserve");
        }
    }
}

