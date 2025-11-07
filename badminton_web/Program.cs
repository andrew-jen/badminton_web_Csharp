using Microsoft.EntityFrameworkCore;                 // EF Core 核心套件，提供資料庫操作功能
using MemberSystemMVC.Models;                         // 引入專案內的命名空間，讓程式可以使用 Test1Context

var builder = WebApplication.CreateBuilder(args);

// ==========================
// 1️⃣ 註冊服務到 DI 容器
// ==========================

// 註冊 MVC Controller + View 支援
builder.Services.AddControllersWithViews();

// 註冊授權服務 (UseAuthorization() 依賴)
builder.Services.AddAuthorization();

// 註冊 Session 所需的分散式快取 (IDistributedCache)
builder.Services.AddDistributedMemoryCache();

// 註冊 Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session 過期時間
    options.Cookie.HttpOnly = true;                // 防止 JS 存取 Cookie
    options.Cookie.IsEssential = true;             // GDPR 規範需要
});

// 註冊 DbContext (MariaDB)
builder.Services.AddDbContext<Test1Context>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));

// ==========================
// 2️⃣ 建立 WebApplication
// ==========================
var app = builder.Build();

// ==========================
// 3️⃣ 啟用中介軟體 (Middleware)
// ==========================

// 非開發環境，使用自訂錯誤頁面與 HSTS
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// 導向 HTTPS
app.UseHttpsRedirection();

// 靜態檔案支援
app.UseStaticFiles();

// 路由
app.UseRouting();

// 啟用 Session
app.UseSession();

// 授權
app.UseAuthorization();

// ==========================
// 4️⃣ 設定路由
// ==========================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ==========================
// 5️⃣ 啟動應用程式
// ==========================
app.Run();


