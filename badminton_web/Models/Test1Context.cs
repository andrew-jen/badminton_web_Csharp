using Microsoft.EntityFrameworkCore; // 匯入 Entity Framework Core 命名空間，提供 ORM 功能

namespace MemberSystemMVC.Models
{
    // Test1Context：資料庫內容類別，繼承自 DbContext，用來連接 MariaDB 資料庫
    // 這個類別代表整個資料庫的操作入口
    public partial class Test1Context : DbContext
    {
        // 無參數建構子 — 當沒有傳入設定時使用
        public Test1Context()
        {
        }

        // 建構子，允許透過依賴注入 (Dependency Injection) 傳入設定選項
        public Test1Context(DbContextOptions<Test1Context> options)
            : base(options)
        {
        }

        // DbSet 代表一張資料表，每個屬性對應一個資料表的集合
        public virtual DbSet<Member> members { get; set; } // 對應 Members 資料表 (會員)
        public virtual DbSet<Venue> Venues { get; set; } // 對應 Venues 資料表 (場地)
        public virtual DbSet<VenueSchedule> VenueSchedules { get; set; } // 對應 VenueSchedules 資料表 (場地時段)
        public virtual DbSet<Registration> Registrations { get; set; } // 對應 Registrations 資料表 (報名紀錄)
        public virtual DbSet<Program> Program { get; set; } // 對應 Program 教練課程 (報名)
        public DbSet<MembersProgramList> MembersProgramList { get; set; } // 對應 MembersProgramList 資料表 (會員課程清單)
        public virtual DbSet<ViewProgramList> ViewProgramList { get; set; } // 對應 ViewProgramList 檢視表 (課程清單)**這是 View，不是 Table
        public virtual DbSet<MembersCoach> MembersCoach { get; set; } // 對應 MembersCoach 資料表 (教練會員)

        // 設定資料庫連線字串與伺服器版本(現在移到appsettings.Development開發檔和Program.cs主執行程式裡面)
        /*protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseMySql(
                "server=127.0.0.1;port=3306;database=test1;user=root;password=Aa0910035817@;charset=utf8mb4",
                Microsoft.EntityFrameworkCore.ServerVersion.Parse("11.8.2-mariadb")); */


        // OnModelCreating：用來設定模型之間的關聯與資料表行為
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 設定資料庫使用的預設字元集與排序規則
            modelBuilder
                .UseCollation("utf8mb4_unicode_ci")
                .HasCharSet("utf8mb4");

            // ========================
            // 關聯設定
            // ========================

            // 一個 Venue（場地）對應多個 VenueSchedule（時段）
            // VenueSchedules 透過 VenueId 外鍵連回 Venue
            // 刪除 Venue 時會一併刪除對應的 VenueSchedules (Cascade)
            modelBuilder.Entity<Venue>()
                .HasMany(v => v.VenueSchedules) // Venue 有多個 VenueSchedule(v是變數代表Venue-因為上面有宣告了)
                .WithOne(vs => vs.VenueInfo) // 每個 VenueSchedule 對應一個 VenueInfo(vs也是變數代表v.VenueSchedules-因為上面有宣告了)
                .HasForeignKey(vs => vs.VenueId) // 外鍵欄位是 VenueId
                .OnDelete(DeleteBehavior.Cascade); // 刪除 Venue 時會連動刪除 VenueSchedule

            // 一個 Venue（場地）對應多個 Registration（報名紀錄）
            // Registration 透過 VenueId 外鍵連回 Venue
            // 刪除 Venue 時會連同刪除對應的報名紀錄
            modelBuilder.Entity<Venue>()
                .HasMany<Registration>() // Venue 有多個 Registration
                .WithOne(r => r.VenueInfo) // 每個 Registration 對應一個 VenueInfo
                .HasForeignKey(r => r.VenueId) // 外鍵欄位是 VenueId
                .OnDelete(DeleteBehavior.Cascade); // 刪除 Venue 時會連動刪除 Registration

            // 呼叫部分類別 (partial class) 內的擴充設定方法（若有額外設定會放在另一個檔案中）
            OnModelCreatingPartial(modelBuilder);

            // 這是 View，不是 Table
            modelBuilder
                .Entity<ViewProgramList>()
                .ToView("view_programlist")
                .HasNoKey();  // 因為 View 通常沒有主鍵
        }

        // partial 方法：允許在其他 partial 檔案中擴充模型設定
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}




