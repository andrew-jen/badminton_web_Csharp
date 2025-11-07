using MemberSystemMVC.Models;            // 引入資料模型 (Member、Test1Context)
using MemberSystemMVC.Models.Utility;    // 引入 ValidationHelper，提供帳號/密碼/年齡等驗證工具
using Microsoft.AspNetCore.Http;         // 支援 Session 功能
using Microsoft.AspNetCore.Mvc;          // MVC Controller 與 View 支援
using System.Threading.Tasks;             // 支援非同步 Task
using System.Security.Claims;           // 支援 ClaimsPrincipal 用於 JWT 驗證

namespace MemberSystemMVC.Controllers
{
    // 會員控制器，處理會員註冊、登入、登出功能
    public class MemberController : Controller
    {
        private readonly Test1Context _context; // EF Core DbContext，用於操作會員資料表

        private readonly IConfiguration _configuration; // 用於讀取 appsettings.json 配置(這裡用來讀jwt)

        // 建構子，透過依賴注入注入 DbContext
        public MemberController(Test1Context context)
        {
            _context = context;
        }

        // =========================
        // 顯示註冊畫面
        // =========================
        public IActionResult Register()
        {
            return View(); // 回傳註冊頁面
        }

        // =========================
        // 接收註冊表單資料 (POST)
        // =========================
        /*
        下面public async Task<IActionResult> Register(Member member)這裡是呼叫方法，不是定義方法!!!-public async Task<IActionResult> Register
        這個方法在 MVC 的 Controller 裡稱為 Action，對應一個 URL 路徑。
        使用者在瀏覽器進入 /Member/Register → GET 請求
        框架找到 MemberController.Register()函式 → 呼叫它 → 回傳 View 給使用者
        使用者填完表單按送出 → POST 請求 /Member/Register
        框架找到 [HttpPost] Register(Member member) → 找到並自動建立 Member.cs 物件並填入表單資料 → 呼叫方法

        1.[HttpPost]

            表示這個方法處理 HTTP POST 請求（通常是表單送出）。

            對應前端 <form method="post" ...>。

        2.async Task<IActionResult> 方法名稱(在Controller裡面等於頁面名稱)

            async → 非同步方法，允許使用 await 等待資料庫操作。

            Task<IActionResult> (是回傳型別，不是方法名稱) → 回傳結果是 MVC 的 ActionResult，可以是：

                View() → 顯示頁面

                RedirectToAction() → 重新導向到其他 Action

                Json() → 回傳 JSON（API 用）

        3.Member member

            MVC Model Binding 自動把表單資料映射到 Member 類別物件。

            例如：前端表單有 <input name="Account" />，會對應到 member.Account。
         */


        [HttpPost] //「表單送出（submit）」或「Ajax 用 POST 送資料」的時候要加
        public async Task<IActionResult> Register(Member member)
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
            // 4️⃣ 年齡驗證
            // -------------------------
            string ageError = ValidationHelper.ValidateAge(member.Age);
            if (ageError != null)
            {
                ModelState.AddModelError("Age", ageError);
                return View(member);
            }

            // -------------------------
            // 5️⃣ 球齡驗證
            // -------------------------
            string duringError = ValidationHelper.ValidateDuringPlayer(member.During_player);
            if (duringError != null)
            {
                ModelState.AddModelError("During_player", duringError);
                return View(member);
            }

            // -------------------------
            // 6️⃣ 全部驗證通過，新增會員
            // -------------------------
            member.Id = 0;                 // 保證 EF 不會誤認為是現有資料
            _context.members.Add(member);  // 將會員加入 DbContext
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
            return RedirectToAction("Index", "Home"); // 導向首頁
        }

        // =========================
        // 顯示登入畫面 / 處理登入
        // =========================
        public IActionResult Login(string account, string password)
        {


            // 從資料庫找出帳號密碼符合的會員
            var member = _context.members
                .FirstOrDefault(m => m.Account == account);

            if (member != null)
            {
                bool passwordError = ValidationHelper.VerifyPassword(password ,member.Password);

                if (passwordError != false)
                {
                    // 使用jwt：產生 JWT (目前先註解，之後可開啟) !!!!
                    /*
                    var jwtHelper = new ValidationHelper.JwtHelper(_configuration);
                    string token = jwtHelper.GenerateToken(member.Account, member.Name);
                    HttpContext.Session.SetString("JwtToken", token); // 可存到 Session 或 Cookie
                    */

                    // 登入成功，將會員資訊存入 Session
                    HttpContext.Session.SetString("MemberName", member.Name);
                    HttpContext.Session.SetString("MemberAccount", member.Account);

                    // 導向首頁
                    return RedirectToAction("Index", "Home"); // 導向首頁
                    // 去驗證jwt
                    //return RedirectToAction("SecurePage");
                }
            }

            // 登入失敗
            ViewBag.Error = "帳號或密碼錯誤";
            return View();
        }


        /*驗證jwt
        private ClaimsPrincipal GetUserFromJwt()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                return null;

            try
            {
                var jwtHelper = new ValidationHelper.JwtHelper(_configuration);
                return jwtHelper.ValidateToken(token);
            }
            catch
            {
                return null; // JWT 無效或過期
            }
        }

        public IActionResult SecurePage()
        {
            var user = GetUserFromJwt();
            if (user == null)
                return RedirectToAction("Login", "Member");

            string account = user.FindFirstValue(ClaimTypes.NameIdentifier);
            string name = user.FindFirstValue(ClaimTypes.Name);

            ViewBag.Account = account;
            ViewBag.Name = name;

            return View();
        }
        */


        // =========================
        // 登出功能
        // =========================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // 清除所有 Session 資料
            return RedirectToAction("Index", "Home"); // 導回首頁
        }
    }
}


