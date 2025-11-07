using MemberSystemMVC.Models; // 引入資料模型，包含 Member、Venue、Registration 等
using Microsoft.AspNetCore.Cryptography.KeyDerivation; // 提供 PBKDF2 加密方法
using Microsoft.EntityFrameworkCore; // 提供 EF Core 資料庫操作功能
using Microsoft.Extensions.Configuration; // 讀取 appsettings.json 配置
using Microsoft.IdentityModel.Tokens; // 提供 JWT 加密簽章
using System;
using System.IdentityModel.Tokens.Jwt; // 生成與驗證 JWT
using System.Security.Claims; // JWT Claims
using System.Security.Cryptography; // 提供隨機數與加密功能
using System.Text; // 文字編碼
using System.Text.RegularExpressions; // 正則表達式處理
using System.Threading.Tasks;

namespace MemberSystemMVC.Models.Utility
{
    /// <summary>
    /// 驗證與工具類別 Helper
    /// 包含帳號密碼驗證、密碼加密、JWT 生成與驗證等功能
    /// </summary>
    public static class ValidationHelper
    {
        // --------------------------------------
        // 帳號驗證規則
        // 至少一個大寫字母、一個小寫字母、一個特殊符號，長度 8~16
        // --------------------------------------
        private static readonly Regex UsernamePattern =
            new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*[*/@/&/$/^/%/#/!]).{8,16}$");

        /// <summary>
        /// 僅驗證帳號格式，回傳 true 表示合法
        /// </summary>
        /// <param name="username">使用者輸入帳號</param>
        public static bool IsValidUsernameFormat(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false; // 空字串不合法
            return UsernamePattern.IsMatch(username); // 符合正則表達式即合法
        }

        /// <summary>
        /// 驗證帳號格式並檢查資料庫是否已存在相同帳號
        /// </summary>
        /// <param name="username">使用者輸入帳號</param>
        /// <param name="dbContext">EF Core DbContext 物件，用於查詢資料庫</param>
        /// <returns>null = 合法, 非 null = 錯誤訊息</returns>
        public static async Task<string> ValidateUsernameAsync(string username, Test1Context dbContext)
        {
            if (string.IsNullOrWhiteSpace(username))
                return "帳號不能為空";

            if (username.Length < 8 || username.Length > 16)
                return "帳號長度需介於 8 到 16 個字元";

            if (!Regex.IsMatch(username, @"[A-Z]"))
                return "帳號需包含至少一個大寫字母";

            if (!Regex.IsMatch(username, @"[a-z]"))
                return "帳號需包含至少一個小寫字母";

            if (!Regex.IsMatch(username, @"[*/@/&/$/^/%/#/!]"))
                return "帳號需包含至少一個特殊符號（*/@/&/$/^/%/#/!）";

            // 資料庫檢查：確認帳號是否已存在
            bool exists = await dbContext.members.AnyAsync(m => m.Account == username);
            if (exists)
                return "此帳號已被使用";

            return null; // 全部檢查通過，帳號合法
        }

        /// <summary>
        /// 驗證密碼格式
        /// 密碼需包含小寫字母、數字，長度 8~16
        /// </summary>
        public static string ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return "密碼不能為空";

            if (password.Length < 8 || password.Length > 16)
                return "密碼長度需介於 8 到 16 個字元";

            if (!Regex.IsMatch(password, @"[a-z]"))
                return "密碼需包含至少一個小寫字母";

            if (!Regex.IsMatch(password, @"\d"))
                return "密碼需包含至少一個數字";

            return null; // 密碼合法
        }

        /// <summary>
        /// 驗證性別是否正確
        /// 僅允許 "男"、"女"、"不透露"
        /// </summary>
        public static string ValidateSex(string sex)
        {
            if (string.IsNullOrWhiteSpace(sex))
                return "性別不能為空";

            var validSexes = new string[] { "男", "女", "不透露" };
            if (!Array.Exists(validSexes, s => s == sex))
                return "性別只能是: 男、女、不透露";

            return null;
        }

        /// <summary>
        /// 驗證年齡範圍 18~70
        /// </summary>
        public static string ValidateAge(int age)
        {
            if (age < 18 || age > 70)
                return "年齡必須介於 18 到 70 歲";
            return null;
        }

        /// <summary>
        /// 驗證球齡不能為負數
        /// </summary>
        public static string ValidateDuringPlayer(int years)
        {
            if (years < 0)
                return "球齡不能小於 0";
            return null;
        }
        public static string ValidateKey(string key)
        {
            if (key != "BadmintonCoach2024") 
                return "驗證碼錯誤";
            return null;
        }

        // --------------------------------------
        // 密碼加密與驗證
        // --------------------------------------

        /// <summary>
        /// 使用 PBKDF2 + 隨機鹽值將密碼加密
        /// 儲存時格式：Base64(salt).Base64(hash)
        /// </summary>
        public static string HashPassword(string password)
        {
            // 生成 16 bytes 隨機鹽
            byte[] salt = RandomNumberGenerator.GetBytes(16);

            // 使用 PBKDF2 算法生成 hash
            byte[] hash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: 100000, // 迭代次數
                numBytesRequested: 32); // hash 長度

            // 將 salt 和 hash 以 "." 分隔儲存
            return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        /// <summary>
        /// 驗證輸入密碼是否與加密密碼匹配
        /// </summary>
        /// <param name="password">使用者輸入密碼</param>
        /// <param name="hashedPassword">資料庫存的 hash 值</param>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            var parts = hashedPassword.Split('.'); // 分離 salt 和 hash
            var salt = Convert.FromBase64String(parts[0]);

            // 重新計算 hash
            var hash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: 100000,
                numBytesRequested: 32);

            return Convert.ToBase64String(hash) == parts[1];
        }

        // --------------------------------------
        // JWT 功能 (目前未啟用)
        // 可將金鑰與過期時間放在 appsettings.json
        // --------------------------------------
        public class JwtHelper
        {
            private readonly string _secretKey; // JWT 簽章金鑰
            private readonly int _expireMinutes; // 過期時間 (分鐘)

            public JwtHelper(IConfiguration configuration)
            {
                _secretKey = configuration["JwtSettings:SecretKey"];
                _expireMinutes = int.Parse(configuration["JwtSettings:ExpireMinutes"]);
            }

            /// <summary>
            /// 生成 JWT Token
            /// </summary>
            /// <param name="account">會員帳號</param>
            /// <param name="name">會員姓名</param>
            public string GenerateToken(string account, string name)
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_secretKey);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, account), // JWT Sub
                        new Claim(ClaimTypes.Name, name) // 用戶名稱
                    }),
                    Expires = DateTime.UtcNow.AddMinutes(_expireMinutes), // 過期時間
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }

            /// <summary>
            /// 驗證 JWT Token
            /// </summary>
            public ClaimsPrincipal ValidateToken(string token)
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_secretKey);

                var parameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true, // 檢查是否過期
                    ValidateIssuerSigningKey = true, // 驗證簽章
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };

                return tokenHandler.ValidateToken(token, parameters, out SecurityToken validatedToken);
            }

            //從 Session 取出 JWT 並驗證
            public ClaimsPrincipal ValidateJwtFromSession(HttpContext httpContext)
            {
                string token = httpContext.Session.GetString("JwtToken");
                if (string.IsNullOrEmpty(token))
                    return null;

                try
                {
                    return ValidateToken(token); // 調用你已經有的 ValidateToken 方法
                }
                catch
                {
                    return null; // 驗證失敗或過期
                }
            }

        }
    }
}
