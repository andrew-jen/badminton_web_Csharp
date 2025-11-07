🏸 羽球場地預約與教練課程管理系統
📘 專案簡介

本專案為 羽球場地預約與教練課程管理系統，旨在提供使用者友善的課程與場地管理平台，讓教練能方便地開課、取消課程，學員則能清楚查看可預約時段與場地資訊。

系統採用 ASP.NET MVC 架構 開發，後端以 C# 搭配 Entity Framework Core 實作資料存取層，前端則使用 Razor View 結合 Bootstrap，打造響應式與互動性高的網頁介面。

⚙️ 系統架構

後端技術：ASP.NET Core MVC、C#

資料庫：SQL Server / MariaDB

ORM 框架：Entity Framework Core

前端技術：Razor View、Bootstrap、JavaScript

驗證與安全性：

密碼以 雜湊加密 (Hashing) 儲存於資料庫

實作 JWT (JSON Web Token) 認證機制以強化安全性

使用 Session 管理登入狀態

🧩 系統功能

🔑 教練註冊驗證 — 驗證金鑰後方可進行註冊

📅 開課管理 — 教練可設定課程日期、時段、費用與場地

❌ 課程取消功能 — 支援查詢與刪除既有課程

🏟 場地時段篩選 — 自動顯示可預約時段，限制為未來四週內

🧠 會員與教練資料管理 — 具備完整 CRUD 操作與登入驗證

🗂️ 資料庫設計

資料表包含以下主要實體：

Members：會員與教練資料

Program：課程資訊（場地、日期、時段、費用、人數等）

Venues：場地資訊與地址

VenueSchedules：場地開放與預約時段

資料庫連線設定與帳密資訊儲存在開發用 appsettings.json 檔案中，於執行時由程式自動載入。

🚀 專案特色

採用 MVC 架構與 RESTful API 原則，方便維護與擴充

非同步處理 (async/await) 提升資料查詢效能

支援 JWT 與 Session 雙重登入驗證流程

介面設計簡潔直覺，具良好使用體驗

🧰 執行方式

於 Visual Studio 開啟專案

修改 appsettings.json 中的資料庫連線字串

執行資料庫遷移（若使用 EF Code First）：

dotnet ef database update


執行專案並透過瀏覽器開啟：

https://localhost:7051


若需外部測試，可使用 ngrok：

ngrok http https://localhost:7051

👨‍💻 作者

開發者：hung-yi jen
技術領域：Web 全端開發、資料庫設計、後端系統整合
