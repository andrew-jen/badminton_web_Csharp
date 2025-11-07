using badminton_web.Models;
using MemberSystemMVC.Models;            // 引入資料模型 (Member、Test1Context)
using MemberSystemMVC.Models.Utility;    // 引入 ValidationHelper，提供帳號/密碼/年齡等驗證工具
using Microsoft.AspNetCore.Http;         // 支援 Session 功能
using Microsoft.AspNetCore.Mvc;          // MVC Controller 與 View 支援
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;           // 支援 ClaimsPrincipal 用於 JWT 驗證
using System.Security.Principal;
using System.Threading.Tasks;             // 支援非同步 Task
namespace badminton_web.Controllers
{
    public class CoachController : Controller
    {
        private readonly Test1Context _context; // EF Core DbContext，用來操作資料庫

        private readonly IConfiguration _configuration; // 用於讀取 appsettings.json 配置(這裡用來讀jwt)

        // 建構子，透過依賴注入 (DI) 傳入 DbContext
        public CoachController(Test1Context context)
        {
            _context = context;
        }
        public IActionResult INorUP()
        {
            return View(); // 對應 Views/Coach/INorUP.cshtml
        }

        [HttpGet]
        public async Task<IActionResult> Operating()
        {
            string account = HttpContext.Session.GetString("MemberAccount");

            // 取得今天日期與下個月日期
            var today = DateTime.Today;
            var nextMonth = today.AddMonths(1);

            // 查詢場地資料
            var venues = await _context.Venues
                .Select(v => v.Name)   // 只選擇場地名稱
                .ToListAsync();

            // 查詢會員資料
            var member = await _context.MembersCoach
                .FirstOrDefaultAsync(m => m.Account == account);

            // 如果會員尚未登入，導向登入頁
            if (string.IsNullOrEmpty(account))
            {
                return RedirectToAction("Login", "Member");
            }

            var model = new OperatingViewModel
            {
                Venues = venues
            };


            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> CoachCancel()
        {             
            string coachname = HttpContext.Session.GetString("MemberName");

            // 查詢該教練的所有課程
            var programs = await _context.Program
                .Where(p => p.CoachName == coachname)
                .ToListAsync();

            return View(programs);
        }


        [HttpPost]
        public async Task<IActionResult> Operating(string venuename, DateTime scheduledate, string timeslot, decimal fee, int capacity, string recommendationlevel, string coachphone)
        {
            string account = HttpContext.Session.GetString("MemberAccount");

            // 取得今天日期與下個月日期
            var today = DateTime.Today;
            var nextMonth = today.AddMonths(1);

            // 查詢會員資料
            var member = await _context.MembersCoach
                .FirstOrDefaultAsync(m => m.Account == account);

            // 如果會員尚未登入，導向登入頁
            if (string.IsNullOrEmpty(account))
            {
                return RedirectToAction("Login", "Member");
            }

            var venues = await _context.Venues
                 .Select(v => v.Name)   // 只選擇場地名稱
                 .ToListAsync();

            var model = new OperatingViewModel
            {
                Venues = venues,
                VenueName = venuename,
                ScheduleDate = scheduledate,
                TimeSlot = timeslot,
                Fee = fee,
                Capacity = capacity,
                RecommendationLevel = recommendationlevel,
                CoachPhone = coachphone
            };

            // 查詢場地資料
            var theaddress = await _context.Venues.FirstOrDefaultAsync(v => v.Name == venuename);

            // ============================
            // 建立課程紀錄
            var registration = new MemberSystemMVC.Models.Program
            {
                VenueName = venuename,
                ScheduleDate = scheduledate,
                TimeSlot = timeslot,
                Fee = fee,
                Capacity = capacity,
                RecommendationLevel = recommendationlevel,
                CoachName = member.Name,      // 教練姓名
                CoachPhone = coachphone,
                RegisteredCount = 0,
                Address = theaddress.Address   // 場地地址
            };

            _context.Program.Add(registration);
            await _context.SaveChangesAsync();

            // 傳當前月份給 View
            ViewBag.CurrentMonth = today.ToString("yyyy-MM");

            // 將 programs 資料傳給 View
            return View(model);
        }

        public async Task<IActionResult> CoachKey(string coachkey)
        {
            // 呼叫驗證輔助類別檢查金鑰
            string keyError = ValidationHelper.ValidateKey(coachkey);

            if (keyError != null)
            {
                // 驗證失敗時，顯示錯誤訊息
                ModelState.AddModelError("CoachKey", keyError);
                return View(); // 回到同一頁顯示錯誤
            }

            // ✅ 驗證成功 → 導向教練註冊頁面
            TempData["VerifiedCoachKey"] = coachkey;
            return RedirectToAction("CoachSignup");
        }

        public async Task<IActionResult> CoachSignup(MembersCoach member)
        {
            // 檢查模型狀態是否有效 (DataAnnotation 驗證)
            if (!ModelState.IsValid)
            {
                return View(member); // 若驗證失敗，重新顯示表單
            }

            // -------------------------
            // 1️⃣ 帳號驗證
            // -------------------------
            string usernameError = await ValidationHelper.ValidateUsernameAsync(member.Account, _context);
            if (usernameError != null)
            {
                ModelState.AddModelError("Account", usernameError);
                return View(member);
            }

            // -------------------------
            // 2️⃣ 密碼驗證
            // -------------------------
            string passwordError = ValidationHelper.ValidatePassword(member.Password);
            if (passwordError != null)
            {
                ModelState.AddModelError("Password", passwordError);
                return View(member);
            }

            // 密碼通過驗證後再雜湊
            member.Password = ValidationHelper.HashPassword(member.Password);

            // -------------------------
            // 3️⃣ 性別驗證
            // -------------------------
            string sexError = ValidationHelper.ValidateSex(member.Sex);
            if (sexError != null)
            {
                ModelState.AddModelError("Sex", sexError);
                return View(member);
            }
       
            // -------------------------
            // 5️⃣ 球齡驗證
            // -------------------------
            string duringError = ValidationHelper.ValidateDuringPlayer(member.DuringPlayer);
            if (duringError != null)
            {
                ModelState.AddModelError("During_player", duringError);
                return View(member);
            }

            //確認是否有欄位出錯
            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    Console.WriteLine($"{state.Key} - {string.Join(", ", state.Value.Errors.Select(e => e.ErrorMessage))}");
                }
                return View(member);
            }



            // -------------------------
            // 6️⃣ 全部驗證通過，新增會員
            // -------------------------
            member.Id = 0;                 // 保證 EF 不會誤認為是現有資料
            _context.MembersCoach.Add(member);  // 將會員加入 DbContext
            await _context.SaveChangesAsync(); // 儲存到資料庫

            // 使用jwt：產生 JWT (目前先註解，之後可開啟) !!!!
            /*
            var jwtHelper = new ValidationHelper.JwtHelper(_configuration);
            string token = jwtHelper.GenerateToken(member.Account, member.Name);
            HttpContext.Session.SetString("JwtToken", token); // 可存到 Session 或 Cookie
            */

            // 登入成功，將會員資訊存入 Session
            HttpContext.Session.SetString("MemberName", member.Name);
            HttpContext.Session.SetString("MemberAccount", member.Account);

            // 去驗證jwt
            //return RedirectToAction("SecurePage");

            ViewBag.Message = "註冊成功！";  // 顯示成功訊息
            return RedirectToAction("Login", "Member"); // 導向登入頁面
        }

        [HttpPost]
        public async Task<IActionResult> CoachCancel(int id)
        {             
            string coachname = HttpContext.Session.GetString("MemberName");
            // 查詢會員資料
            var programs = await _context.Program
                .FirstOrDefaultAsync(p => p.Id == id && p.CoachName == coachname);

            // 刪除課程
            _context.Program.Remove(programs);
            await _context.SaveChangesAsync();

            return RedirectToAction("Operating", "Coach");
        }
    }
}
